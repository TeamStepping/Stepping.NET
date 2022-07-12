using Stepping.Core.Steps;

namespace Stepping.TmProviders.Dtm.Grpc.Steps;

public interface IStepToDtmStepConverter
{
   Task<bool> CanConvertAsync(IStep step);
   
   Task<DtmStepInfoModel> ConvertAsync(string stepName, object? args);
}