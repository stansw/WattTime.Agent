using Alexa.NET.Request;
using Alexa.NET.Response;
using System;
using System.Threading.Tasks;

namespace WattTime.Agent.Handlers
{
    public class UnknownIntentHandler : IRequestHandler
    {
        public bool CanHandle(IServiceProvider context, SkillRequest request)
        {
            // Unknown can handle all the requests.
            return true;
        }

        public Task<SkillResponse> Handle(IServiceProvider context, SkillRequest request)
        {
            return Task.FromResult(new SkillResponse()
            {
                // Pass through session state intact.
                SessionAttributes = request.Session.Attributes,
                Response = new ResponseBody()
                {
                    ShouldEndSession = false,
                    OutputSpeech = new PlainTextOutputSpeech()
                    {
                        Text = "Sorry, I didn't understand this command. Please say it again."
                    },
                    Reprompt = new Reprompt()
                    {
                        OutputSpeech = new PlainTextOutputSpeech()
                        {
                            Text = "Please say it again."
                        }
                    }
                }
            });
        }
    }
}
