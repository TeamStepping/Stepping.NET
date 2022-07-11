using MongoDB.Driver;

namespace Stepping.DbProviders.MongoDb;

public interface IBarrierCollectionProvider
{
    Task<IMongoCollection<SteppingBarrierDocument>> GetAsync(MongoDbSteppingDbContext context);
}