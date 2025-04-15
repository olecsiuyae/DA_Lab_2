using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SharedModels;

namespace ApiService.Services
{
    public interface IRabbitMQService
    {
        void SendMessage(Device device);
    }

    public class RabbitMQService : IRabbitMQService
    {
        private readonly string _hostname = "localhost";
        private readonly string _queueName = "devices_queue";
        private readonly string _username = "guest";
        private readonly string _password = "guest";

        public void SendMessage(Device device)
        {
            try
            {
                var factory = new ConnectionFactory 
                { 
                    HostName = _hostname,
                    UserName = _username,
                    Password = _password
                };

                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();

                channel.QueueDeclare(queue: _queueName,
                                   durable: true,
                                   exclusive: false,
                                   autoDelete: false,
                                   arguments: null);

                var json = JsonSerializer.Serialize(device);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                   routingKey: _queueName,
                                   basicProperties: properties,
                                   body: body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message to RabbitMQ: {ex.Message}");
                throw;
            }
        }
    }
} 