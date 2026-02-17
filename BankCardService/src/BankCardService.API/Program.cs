using BankCardService.API.Extensions;
using BankCardService.API.GlobalExceptionMiddleware;
using BankCardService.Application.Commands.CreateBankCard;
using BankCardService.Application.Interfaces;
using BankCardService.Infrastructure.Data;
using BankCardService.Infrastructure.Messaging;
using BankCardService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BankCardDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BankCardDb")
    )
);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(CreateBankCardCommand).Assembly
    ));



builder.Services.AddScoped<IBankCardRepository, BankCardRepository>();

builder.Services.AddHostedService<UserCreatedConsumer>();

builder.Services.AddHealthChecksConfiguration(builder.Configuration);

builder.Services.AddRateLimitingConfiguration(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapHealthCheckEndpoints();

app.MapControllers();

app.Run();
