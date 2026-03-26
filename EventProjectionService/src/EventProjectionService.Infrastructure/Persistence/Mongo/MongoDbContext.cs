using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventProjectionService.Infrastructure.Persistence.Mongo;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoOptions> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);

        _database = client.GetDatabase(options.Value.DatabaseName);
    }

    public IMongoCollection<StoredEvent> Events =>
        _database.GetCollection<StoredEvent>("events");
}