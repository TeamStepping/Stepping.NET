using Stepping.Core.Steps;

namespace Stepping.TmProviders.Dtm.Grpc.Steps;

public interface IStepToDtmStepConvertResolver
{
    Task<DtmStepInfoModel> ResolveAsync(IStep step);
}