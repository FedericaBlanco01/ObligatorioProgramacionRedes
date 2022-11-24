using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using ServerLog.Data;
using ServerLog.Model;

namespace ServerLog.Service
{
    public class MQService
    {
        public MQService() {

            // Conexión con RabbitMQ local: 
            var factory = new ConnectionFactory() { HostName = "localhost" }; // Defino la conexion

             var connection = factory.CreateConnection();
             var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "log", // en el canal, definimos la Queue de la conexion
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            //Defino el mecanismo de consumo
            var consumer = new EventingBasicConsumer(channel);
            //Defino el evento que sera invocado cuando llegue un mensaje 
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
                LogModel log = JsonSerializer.Deserialize<LogModel>(message);

                var data = DataAccess.GetInstance();
                data.AddLog(log);
            };

            //"PRENDO" el consumo de mensajes
            channel.BasicConsume(queue: "log",
                autoAck: true,
                consumer: consumer);


        }
    }
}
