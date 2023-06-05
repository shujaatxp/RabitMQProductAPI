using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;


// Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
ConnectionFactory factory = new();
factory.Uri = new Uri("amqp://guest:guest@localhost:5672");
factory.ClientProvidedName = "Rabbit Receiver1 App";

IConnection connection = factory.CreateConnection();
IModel channel = connection.CreateModel();

string exchangename = "demoexchange";
string routingkey = "demo-routing-key";
string queuename = "demo-queue";

channel.ExchangeDeclare(exchangename, ExchangeType.Direct);
channel.QueueDeclare(queuename, true, false, false, 
    new Dictionary<string, object>
    {
        {"x-dead-letter-exchange","demo-dead-letter-exchange" },
        {"x-queue-mode","lazy" }
    }
    );
channel.QueueBind(queuename, exchangename, routingkey, null);
channel.BasicQos(0, 1, false);

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) => {
    
    //Task.Delay(TimeSpan.FromSeconds(1)).Wait();
    
    var body = args.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    

    //send message to database at this level
    Console.WriteLine($"Message received by receiver1: {message}");

    if (Convert.ToInt32(message) == 5)
    {
        // channel.BasicReject(args.DeliveryTag,true);
        channel.BasicNack(args.DeliveryTag, false, false);
        Console.WriteLine($"Message Failed by receiver1: {message}");
    }
    else
    {
        channel.BasicAck(args.DeliveryTag, false);
        Console.WriteLine($"Message Processed by receiver1: {message}");
    }
};

string consumertag = channel.BasicConsume(queuename,false,consumer);

Console.ReadLine();

channel.BasicCancel(consumertag);

channel.Close();
connection.Close();