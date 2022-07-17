using Stepping.Core.Steps;

namespace Stepping.TestBase.Fakes;

[StepName(RequestGitHubRenderMarkdownName)]
public class RequestGitHubRenderMarkdownStep : HttpRequestStep
{
    public const string RequestGitHubRenderMarkdownName = "RequestGitHubRenderMarkdown";

    public RequestGitHubRenderMarkdownStep(string text) : base(
        new HttpRequestStepArgs(
            "https://api.github.com/markdown",
            HttpMethod.Post,
            new Dictionary<string, object>
            {
                { "text", text }
            }
        )
    )
    {
    }
}