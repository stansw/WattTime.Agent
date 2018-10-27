using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Microsoft.AspNetCore.Mvc;

namespace WattTime.Agent.Controllers
{
    [Route("api/[controller]")]
    public class SkillController : Controller
    {
        [HttpGet]
        public Task<SkillResponse> Handle()
        {
            return this.Handle(new SkillRequest());
        }

        [HttpPost]
        public Task<SkillResponse> Handle([FromBody]SkillRequest request)
        {
            return Task.FromResult(new SkillResponse()
            {
                Version = "1.0",
                Response = new ResponseBody()
                {
                    OutputSpeech = new PlainTextOutputSpeech()
                    {
                        Text = "It works",
                    }
                }
            });
        }
    }
}
