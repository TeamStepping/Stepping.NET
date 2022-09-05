using Stepping.Core.Exceptions;
using Stepping.Core.Infrastructures;
using Stepping.Core.Steps;

namespace Stepping.TmProviders.LocalTm.Steps;

public class LocalTmStepExecutor : ILocalTmStepExecutor
{
    protected IStepResolver StepResolver { get; }

    protected IStepExecutor StepExecutor { get; }

    protected ISteppingJsonSerializer SteppingJsonSerializer { get; }

    protected IHttpClientFactory HttpClientFactory { get; }

    public LocalTmStepExecutor(
            IStepResolver stepResolver,
            IStepExecutor stepExecutor,
            ISteppingJsonSerializer steppingJsonSerializer,
            IHttpClientFactory httpClientFactory)
    {
        StepResolver = stepResolver;
        StepExecutor = stepExecutor;
        SteppingJsonSerializer = steppingJsonSerializer;
        HttpClientFactory = httpClientFactory;
    }

    public virtual async Task ExecuteAsync(string gid, LocalTmStepInfoModel stepInfoModel, CancellationToken cancellationToken = default)
    {
        var step = StepResolver.Resolve(stepInfoModel.StepName, stepInfoModel.StepArgs);

        var stepType = step.GetType();

        if (typeof(IExecutableStep).IsAssignableFrom(stepType))
        {
            await StepExecutor.ExecuteAsync(gid, stepInfoModel.StepName, stepInfoModel.StepArgs, cancellationToken);
            return;
        }

        if (typeof(HttpRequestStep).IsAssignableFrom(stepType))
        {
            var httpRequestStep = (HttpRequestStep)step;
            var response = await HttpClientFactory.CreateClient(LocalTmConst.LocalTmHttpClient)
                .SendAsync(httpRequestStep.CreateHttpRequestMessage(SteppingJsonSerializer));
            response.EnsureSuccessStatusCode();
            return;
        }

        throw new SteppingException($"Unknown step type: {stepType.FullName}.");
    }
}
