using Stepping.Core.Steps;

namespace Stepping.TmProviders.LocalTm.Steps;

public interface ILocalTmStepConverter
{
    Task<LocalTmStepModel> ConvertAsync(List<IStep> steps);
}