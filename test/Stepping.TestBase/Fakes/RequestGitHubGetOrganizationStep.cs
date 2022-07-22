using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

[StepName(RequestGitHubGetOrganizationStepName)]
public class RequestGitHubGetOrganizationStep : HttpRequestStep
{
    public const string RequestGitHubGetOrganizationStepName = "RequestGitHubGetOrganization";

    public RequestGitHubGetOrganizationStep(string orgName) : base(
        new HttpRequestStepArgs($"https://api.github.com/orgs/{orgName}", HttpMethod.Get))
    {
    }
}