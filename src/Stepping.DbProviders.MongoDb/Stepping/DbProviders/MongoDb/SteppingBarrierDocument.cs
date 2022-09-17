using MongoDB.Bson.Serialization.Attributes;

namespace Stepping.DbProviders.MongoDb;

[BsonIgnoreExtraElements]
public class SteppingBarrierDocument
{
    [BsonElement("trans_type")]
    public string TransType { get; set; } = null!;

    [BsonElement("gid")]
    public string Gid { get; set; } = null!;

    [BsonElement("branch_id")]
    public string BranchId { get; set; } = null!;

    [BsonElement("op")]
    public string Op { get; set; } = null!;

    [BsonElement("barrier_id")]
    public string BarrierId { get; set; } = null!;

    [BsonElement("reason")]
    public string Reason { get; set; } = null!;

    protected SteppingBarrierDocument()
    {
    }

    public SteppingBarrierDocument(
        string transType, string gid, string branchId, string op, string barrierId, string reason)
    {
        TransType = transType;
        Gid = gid;
        BranchId = branchId;
        Op = op;
        BarrierId = barrierId;
        Reason = reason;
    }
}