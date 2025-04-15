using DataProcessor.Data;
using DataProcessor.Services;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddDbContext<DeviceContext>(options =>
            options.UseSqlServer(hostContext.Configuration.GetConnectionString("DefaultConnection")));
            
        services.AddHostedService<MessageConsumerService>();
    })
    .Build();

await host.RunAsync();
