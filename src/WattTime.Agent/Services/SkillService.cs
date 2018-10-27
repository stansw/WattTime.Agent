using Alexa.NET.Request;
using Alexa.NET.Response;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WattTime.Agent.Handlers;
using WattTime.Agent.Interceptors;

namespace WattTime.Agent.Services
{
    public class SkillService : ISkillService
    {
        public const string ResponseVersion = "1.0";

        private static List<IRequestHandler> Handlers = new List<IRequestHandler>()
        {
            new UnknownIntentHandler(),
        };

        private static readonly List<IRequestInterceptor> RequestInterceptors = new List<IRequestInterceptor>()
        {
        };

        private static readonly List<IResponseInterceptor> ResponseInterceptors = new List<IResponseInterceptor>()
        {
        };

        private readonly TelemetryClient _logger;
        private readonly IServiceProvider _context;

        public SkillService(TelemetryClient logger, IServiceProvider context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<SkillResponse> HandleAsync(SkillRequest request)
        {
            await ProcessRequest(request);
            var response = await HandleRequest(request);
            await ProcessResponse(request, response);
            return response;
        }

        private async Task<SkillResponse> HandleRequest(SkillRequest request)
        {
            // Avoid nullability checks in interceptors and handlers by making sure
            // session attributes are always set.
            if (request.Session.Attributes == null)
            {
                request.Session.Attributes = new Dictionary<string, object>();
            }

            // There is always at least one handler for a given request. 
            var handler = Handlers.First(h => h.CanHandle(_context, request));

            var response = await handler.Handle(_context, request);

            // Keep track of the handler to help with debugging.
            response.SessionAttributes["Handler"] = handler.GetType().Name;

            // Make sure each response has a version.
            response.Version = ResponseVersion;

            return response;
        }

        private async Task ProcessRequest(SkillRequest request)
        {
            foreach (var interceptor in RequestInterceptors)
            {
                try
                {
                    await interceptor.ProcessAsync(_context, request);
                }
                catch (Exception exception)
                {
                    var telemetry = new ExceptionTelemetry(exception)
                    {
                        Message = "Interceptor failed to process response.",
                        SeverityLevel = SeverityLevel.Error,
                    };
                    telemetry.Properties["request"] = JsonConvert.SerializeObject(request);
                    _logger.TrackException(telemetry);
                }
            }
        }

        private async Task ProcessResponse(SkillRequest request, SkillResponse response)
        {
            foreach (var interceptor in ResponseInterceptors)
            {
                try
                {
                    await interceptor.ProcessAsync(_context, request, response);
                }
                catch (Exception exception)
                {
                    var telemetry = new ExceptionTelemetry(exception)
                    {
                        Message = "Interceptor failed to process response.",
                        SeverityLevel = SeverityLevel.Error,
                    };
                    telemetry.Properties["request"] = JsonConvert.SerializeObject(request);
                    telemetry.Properties["response"] = JsonConvert.SerializeObject(response);
                    _logger.TrackException(telemetry);
                }
            }
        }

        public Task<SkillResponse> HandleServerErrorAsync(Exception exception, SkillRequest request)
        {
            return Task.FromResult(new SkillResponse()
            {
                Version = ResponseVersion,
                // Pass through session state intact.
                SessionAttributes = request.Session.Attributes,
                Response = new ResponseBody()
                {
                    ShouldEndSession = false,
                    OutputSpeech = new SsmlOutputSpeech()
                    {
                        Ssml =
@"<speak>
  <p>
    There was a problem processing your request. 
    Please try again later.
  </p>
</speak>"
                    }
                }
            });
        }
    }
}
