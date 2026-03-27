using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventProjectionService.Infrastructure.Persistence.Mongo;

public class StoredEvent
{
    [BsonId]
    [BsonGuidRepresentation(GuidRepresentation.Standard)]
    public Guid Id { get; set; }

    public string Type { get; set; } = default!;

    public string Payload { get; set; } = default!;

    public DateTime ReceivedAt { get; set; }
}
