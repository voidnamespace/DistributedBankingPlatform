using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TransactionService.API.Middleware;
using TransactionService.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TransactionDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")));

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(TransactionService.Application.Commands.CreateTransfer.CreateTransferCommand).Assembly));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();
    db.Database.Migrate();
}

app.UseGlobalExceptionHandling();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
