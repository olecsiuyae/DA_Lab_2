using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedModels;
using DataProcessor.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using IModel = RabbitMQ.Client.IModel;

namespace DataProcessor.Services
{
    public class MessageConsumerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "devices_queue";
        private readonly string _username = "guest";
        private readonly string _password = "guest";
        private IConnection _connection;
        private IModel _channel;

        public MessageConsumerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private void InitializeRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(queue: _queueName,
                                   durable: true,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null);

                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing RabbitMQ: {ex.Message}");
                throw;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_connection == null || !_connection.IsOpen)
                    {
                        InitializeRabbitMQ();
                    }

                    var consumer = new EventingBasicConsumer(_channel);
                    consumer.Received += async (ch, ea) =>
                    {
                        try
                        {
                            var content = Encoding.UTF8.GetString(ea.Body.ToArray());
                            var device = JsonSerializer.Deserialize<Device>(content);

                            await HandleMessage(device);

                            _channel.BasicAck(ea.DeliveryTag, false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing message: {ex.Message}");
                            _channel.BasicNack(ea.DeliveryTag, false, true);
                        }
                    };

                    _channel.BasicConsume(queue: _queueName,
                                       autoAck: false,
                                       consumer: consumer);

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in message consumer: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        private async Task HandleMessage(Device device)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DeviceContext>();

            try
            {
                var existingDevice = await context.Devices.FindAsync(device.Id);

                if (existingDevice == null)
                {
                    await context.Devices.AddAsync(device);
                }
                else
                {
                    context.Entry(existingDevice).CurrentValues.SetValues(device);
                    context.Entry(existingDevice).Property(x => x.Type).CurrentValue = device.Type;
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving device to database: {ex.Message}");
                throw;
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
} 