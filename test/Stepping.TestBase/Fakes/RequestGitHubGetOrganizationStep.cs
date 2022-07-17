using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

[StepName(RequestGitHubGetOrganizationStepName)]
public class RequestGitHubGetOrganizationStep : HttpRequestStep
{
    public const string RequestGitHubGetOrganizationStepName = "RequestGitHubGetOrganizationStep";

    public RequestGitHubGetOrganizationStep(string organizationName) : base(
        new HttpRequestStepArgs($"https://api.github.com/orgs/{organizationName.TrimStart('/')}", HttpMethod.Get))
    {
    }
}