using Alexa.NET.Request;
using Alexa.NET.Response;
using System;
using System.Threading.Tasks;

namespace WattTime.Agent.Interceptors
{
    public interface IResponseInterceptor
    {
        Task ProcessAsync(IServiceProvider context, SkillRequest request, SkillResponse response);
    }
}
