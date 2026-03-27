using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EventProjectionService.Infrastructure.Persistence.Mongo;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoOptions _options;

    public MongoDbContext(IOptions<MongoOptions> options)
    {
        _options = options.Value;

        var client = new MongoClient(_options.ConnectionString);

        _database = client.GetDatabase(_options.Database);
    }

    public IMongoCollection<StoredEvent> Events =>
        _database.GetCollection<StoredEvent>(_options.Collection);
}