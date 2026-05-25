using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

    await migrator.MigrateAsync(CancellationToken.None);

    if (bool.TryParse(builder.Configuration["Seeding:Segments:Enabled"], out var segmentsSeedingEnabled)
    && segmentsSeedingEnabled)
    {
        await SegmentsSeeder.SeedAsync(dbContext, CancellationToken.None);
    }

    if (bool.TryParse(builder.Configuration["Seeding:UserMetrics:Enabled"], out var userMetricsSeedingEnabled)
        && userMetricsSeedingEnabled)
    {
        await UserMetricsSeeder.SeedAsync(dbContext, CancellationToken.None);
    }

}

app.Run();
