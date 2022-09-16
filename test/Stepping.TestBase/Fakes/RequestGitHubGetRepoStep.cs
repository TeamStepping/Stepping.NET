using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

[StepName(RequestGitHubGetRepoStepName)]
public class RequestGitHubGetRepoStep : HttpRequestStep
{
    public const string RequestGitHubGetRepoStepName = "RequestGitHubGetRepo";

    public RequestGitHubGetRepoStep(string orgName, string repoName) : base(
        new HttpRequestStepArgs($"https://github.com/{orgName}/{repoName}", HttpMethod.Get))
    {
    }
}
