using Alexa.NET.Request;
using Alexa.NET.Response;
using System;
using System.Threading.Tasks;

namespace WattTime.Agent.Handlers
{
    public interface IRequestHandler
    {
        bool CanHandle(IServiceProvider context, SkillRequest request);

        Task<SkillResponse> Handle(IServiceProvider context, SkillRequest request);
    }
}
