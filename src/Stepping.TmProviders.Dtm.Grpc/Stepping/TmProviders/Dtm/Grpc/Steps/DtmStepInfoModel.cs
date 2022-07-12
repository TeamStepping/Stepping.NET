using Google.Protobuf;

namespace Stepping.TmProviders.Dtm.Grpc.Steps;

public class DtmStepInfoModel
{
    public Dictionary<string, string> Step { get; set; }
    public ByteString BinPayload { get; set; }

    public DtmStepInfoModel(Dictionary<string, string> step, ByteString binPayload)
    {
        Step = step;
        BinPayload = binPayload;
    }
}