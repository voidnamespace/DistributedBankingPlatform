namespace AuditLogService.Infrastructure.Persistence.Mongo;

public class MongoOptions
{
    public string ConnectionString { get; set; } = default!;

    public string Database { get; set; } = default!;
    public string Collection { get; set; } = default!;
}
