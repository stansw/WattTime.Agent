using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WattTime.Agent.Services;

namespace WattTime.Agent.Controllers
{
    [Route("api/[controller]")]
    public class SkillController : Controller
    {
        private static readonly JsonSerializerSettings TelemetrySerializationSettings =
            new JsonSerializerSettings()
            {
                ContractResolver = new TelemetryContractResolver()
            };

        private readonly IHostingEnvironment _environment;
        private readonly TelemetryClient _logger;
        private readonly ISkillService _skillService;

        public SkillController(
            IHostingEnvironment environment,
            IConfiguration configuration,
            TelemetryClient logger,
            ISkillService skillService)
        {
            _environment = environment;
            _logger = logger;
            _skillService = skillService;
        }

        [HttpGet]
        public Task<IActionResult> Handle()
        {
            return Handle(new SkillRequest()
            {
                Session = new Session()
                {
                    User = new User()
                    {
                        UserId = Guid.NewGuid().ToString()
                    }
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> Handle([FromBody]SkillRequest request)
        {
            var response = default(SkillResponse);
            var requestText = default(string);
            try
            {
                // Serialize request at the entry point in case handlers mutate it as a side effect
                // of processing it. It should be rare, but it can happen.
                requestText = JsonConvert.SerializeObject(request, TelemetrySerializationSettings);

                // Set telemetry context.
                _logger.Context.User.Id = request.Session.User.UserId;
                _logger.Context.Session.Id = request.Session.SessionId;

                response = await _skillService.HandleAsync(request);

                _logger.TrackEvent("SkillRequest", new Dictionary<string, string>()
                {
                    ["request"] = requestText,
                    ["response"] = JsonConvert.SerializeObject(response),
                });
            }
            catch (Exception ex)
            {
                if (_environment.IsDevelopment())
                {
                    // Rethrow when in development to show details in developer exception page.
                    throw;
                }
                _logger.TrackException(ex, new Dictionary<string, string>()
                {
                    ["request"] = requestText,
                });
                response = await _skillService.HandleServerErrorAsync(ex, request);
            }
            return Json(response);
        }

        /// <summary>
        /// Contract resolver which ignores sensitive properties in the request which should not
        /// be saved to the logs.
        /// </summary>
        public class TelemetryContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if ((property.DeclaringType == typeof(Alexa.NET.Request.User)
                        && (StringComparer.Ordinal.Equals(property.PropertyName, nameof(Alexa.NET.Request.User.AccessToken))
                            || StringComparer.Ordinal.Equals(property.PropertyName, nameof(Alexa.NET.Request.User.Permissions))))
                    || (property.DeclaringType == typeof(Alexa.NET.Request.AlexaSystem)
                        && (StringComparer.Ordinal.Equals(property.PropertyName, nameof(Alexa.NET.Request.AlexaSystem.ApiAccessToken)))))
                {
                    property.ShouldSerialize = instance => false;
                }

                return property;
            }
        }
    }
}
