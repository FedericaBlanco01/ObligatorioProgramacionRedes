using RabbitMQ.Client;
using System.Text.Json;
using System.Text;

namespace ServerGrpc.Clases
{
    public class LogPublisher
    {
        public static void Message(IModel channel, string userEmail, string eventDone)
        { 
            var log = new LogModel
            {
                Date = DateTime.Now,
                UserEmail = userEmail,
                Event = eventDone
            };

            string messsage = JsonSerializer.Serialize(log);
            var body = Encoding.UTF8.GetBytes(messsage);
            channel.BasicPublish(exchange: "",
                routingKey: "log",
                basicProperties: null,
                body: body);
        }
    }
}
