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

    await migrator.MigrateAsync();

    if (builder.Configuration["Seeding:Segments:Enabled"] == "true")
    {
        await SegmentsSeeder.SeedAsync(dbContext);
    }
    if (builder.Configuration["Seeding:UserMetrics:Enabled"] == "true")
    {
        await UserMetricsSeeder.SeedAsync(dbContext);
    }
    
}

app.Run();
