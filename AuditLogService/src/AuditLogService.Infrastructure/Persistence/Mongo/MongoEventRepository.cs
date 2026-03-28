using MongoDB.Driver;

namespace AuditLogService.Infrastructure.Persistence.Mongo;

public class MongoEventRepository
{
    private readonly IMongoCollection<StoredEvent> _collection;

    public MongoEventRepository(MongoDbContext context)
    {
        _collection = context.Events;
    }

    public async Task SaveAsync(StoredEvent evt)
    {
        await _collection.InsertOneAsync(evt);
    }
}