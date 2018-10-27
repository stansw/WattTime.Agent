using Alexa.NET.Request;
using Alexa.NET.Response;
using System;
using System.Threading.Tasks;

namespace WattTime.Agent.Services
{
    public interface ISkillService
    {
        Task<SkillResponse> HandleAsync(SkillRequest request);

        Task<SkillResponse> HandleServerErrorAsync(Exception exception, SkillRequest request);
    }
}
