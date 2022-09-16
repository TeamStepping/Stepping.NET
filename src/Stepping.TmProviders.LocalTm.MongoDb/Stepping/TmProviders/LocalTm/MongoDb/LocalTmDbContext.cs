using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Stepping.TmProviders.LocalTm.MongoDb;

public class LocalTmMongoDbContext
{
    protected IMongoClient Client { get; }

    protected LocalTmMongoDbOptions Options { get; }

    public LocalTmMongoDbContext(IOptions<LocalTmMongoDbOptions> options)
    {
        Options = options.Value;
        Client = new MongoClient(Options.ConnectionString);
    }

    public virtual IMongoClient GetMongoClient()
    {
        return Client;
    }

    public virtual IMongoDatabase GetMongoDatabase()
    {
        return Client.GetDatabase(Options.DatabaseName);
    }

    public virtual IMongoCollection<TmTransactionDocument> GetTmTransactionCollection()
    {
        return GetMongoDatabase().GetCollection<TmTransactionDocument>("TmTransactions");
    }
}
