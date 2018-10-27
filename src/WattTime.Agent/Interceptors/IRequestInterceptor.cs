using Alexa.NET.Request;
using System;
using System.Threading.Tasks;

namespace WattTime.Agent.Interceptors
{
    public interface IRequestInterceptor
    {
        Task ProcessAsync(IServiceProvider context, SkillRequest request);
    }
}
