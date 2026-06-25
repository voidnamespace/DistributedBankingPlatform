using TransactionKafkaBridge.Infrastructure.MasstransitConsumers;
using TransactionKafkaBridge.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddConsuming(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
