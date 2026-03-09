using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using TransactionService.API;
using TransactionService.API.Middleware;
using TransactionService.API;

var builder = WebApplication.CreateBuilder(args);



var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, builder.Environment);

app.Run();