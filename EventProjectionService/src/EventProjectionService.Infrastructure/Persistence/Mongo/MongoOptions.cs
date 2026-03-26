namespace EventProjectionService.Infrastructure.Persistence.Mongo;

public class MongoOptions
{
    public string ConnectionString { get; set; } = default!;

    public string DatabaseName { get; set; } = default!;
}
