using UserSegmentationService.API;
using UserSegmentationService.Infrastructure.Persistence.Database;
using UserSegmentationService.Infrastructure.Persistence.Seeding;


var builder = WebApplication.CreateBuilder(args);

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, builder.Environment);

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var migrator = scope.ServiceProvider.GetRequiredService<DatabaseMigrator>();
    var dbContext = scope.ServiceProvider.GetRequiredService<SegmentationDbContext>();

    await migrator.MigrateAsync();

    await SegmentsSeeder.SeedAsync(dbContext);
    await UserMetricsSeeder.SeedAsync(dbContext);
}

app.Run();
