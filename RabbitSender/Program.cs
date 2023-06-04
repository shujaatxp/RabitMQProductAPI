using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

//Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
ConnectionFactory factory = new ();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
factory.ClientProvidedName = "Rabbit Send App";

IConnection connection = factory.CreateConnection ();
IModel channel = connection.CreateModel();

string exchangename = "demoexchange";
string routingkey = "demo-routing-key";
string queuename = "demoqueue";

channel.ExchangeDeclare(exchangename, ExchangeType.Direct);
channel.QueueDeclare(queuename, false, false, false, null);
channel.QueueBind(queuename, exchangename, routingkey, null);

for (int i = 0; i <= 50; i++)
{
    string message = $"hello youtube {i}";
    Console.WriteLine($"Message sent to Receiver: {message}");

    var json = JsonConvert.SerializeObject(message);
    var messagebodybytes = Encoding.UTF8.GetBytes(json);
   
    channel.BasicPublish(exchangename, routingkey, null, messagebodybytes);

    Thread.Sleep(1000);
}


channel.Close ();
connection.Close ();