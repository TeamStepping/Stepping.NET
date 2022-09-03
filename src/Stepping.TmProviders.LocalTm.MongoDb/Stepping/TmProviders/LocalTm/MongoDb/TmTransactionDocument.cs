using MongoDB.Bson.Serialization.Attributes;

namespace Stepping.TmProviders.LocalTm.MongoDb;

[BsonIgnoreExtraElements]
public class TmTransactionDocument
{
    [BsonElement("Gid")]
    public string Gid { get; set; } = null!;

    [BsonElement("Status")]
    public string Status { get; set; } = null!;

    [BsonElement("Steps")]
    public string Steps { get; set; } = null!;

    [BsonElement("CreationTime")]
    public DateTime CreationTime { get; set; }

    [BsonElement("UpdateTime")]
    public DateTime? UpdateTime { get; set; }

    [BsonElement("FinishTime")]
    public DateTime? FinishTime { get; set; }

    [BsonElement("RollbackReason")]
    public string? RollbackReason { get; set; }

    [BsonElement("RollbackTime")]
    public DateTime? RollbackTime { get; set; }

    [BsonElement("NextRetryInterval")]
    public int? NextRetryInterval { get; set; }

    [BsonElement("NextRetryTime")]
    public DateTime? NextRetryTime { get; set; }

    [BsonElement("SteppingDbContextLookupInfo")]
    public string SteppingDbContextLookupInfo { get; set; } = null!;

    [BsonElement("ConcurrencyStamp")]
    public string? ConcurrencyStamp { get; set; }
}
