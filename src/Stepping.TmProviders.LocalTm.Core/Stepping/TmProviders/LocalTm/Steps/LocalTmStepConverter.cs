using System.Text;
using Stepping.Core.Steps;

namespace Stepping.TmProviders.LocalTm.Steps;

public class LocalTmStepConverter : ILocalTmStepConverter
{
    protected IStepNameProvider StepNameProvider { get; }
    protected IStepArgsSerializer StepArgsSerializer { get; }

    public LocalTmStepConverter(IStepNameProvider stepNameProvider, IStepArgsSerializer stepArgsSerializer)
    {
        StepNameProvider = stepNameProvider;
        StepArgsSerializer = stepArgsSerializer;
    }

    public virtual async Task<LocalTmStepModel> ConvertAsync(List<IStep> steps)
    {
        var result = new LocalTmStepModel();
        foreach (var step in steps)
        {
            var stepType = step.GetType();
            if (typeof(HttpRequestStep).IsAssignableFrom(stepType))
            {
                stepType = typeof(HttpRequestStep);
            }
            result.Steps.Add(new LocalTmStepInfoModel(StepNameProvider.Get(stepType), await StepArgsSerializeAsync(step)));
        }
        return result;
    }

    protected virtual async Task<string?> StepArgsSerializeAsync(IStep step)
    {
        if (step is not IStepWithArgs withArgs)
        {
            return null;
        }
        var args = withArgs.GetArgs();

        return Encoding.UTF8.GetString(await StepArgsSerializer.SerializeAsync(args));
    }
}
