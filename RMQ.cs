using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Diagnostics;
using RabbitMQ.Client.Events;

namespace PingPongMessaging_cmd
{
    class RMQ
    {
        public ConnectionFactory connectionFactory;
        public IConnection connection;
        public IModel channel;

        public void InitRMQConnection(string host = "rmq2.pptik.id", int port = 5672, string user = "TMDG2022", string pass = "TMDG2022", string vhost = "/TMDG2022")
        {
            connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = host;
            connectionFactory.Port = port;
            connectionFactory.UserName = user;
            connectionFactory.Password = pass;
            connectionFactory.VirtualHost = vhost;
        }

        public void CreateRMQConnection()
        {
            connection = connectionFactory.CreateConnection();
            Console.WriteLine("Koneksi " + (connection.IsOpen ? "Berhasil!" : "Gagal!"));
        }

        public void CreateRMQChannel(string queue_name, string routingKey = "", string exchange_name = "")
        {
            if (connection.IsOpen)
            {
                channel = connection.CreateModel();
                Console.WriteLine("Channel " + (channel.IsOpen ? "Berhasil!" : "Gagal!"));
            }

            if (channel.IsOpen)
            {
                channel.QueueDeclare(queue: queue_name,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
                Console.WriteLine("Queue telah dideklarasikan..");
            }
        }

        public void WaitingMessage(string queue_name)
        {
            string cmd = "a";
            do
            {
                if (channel.IsOpen)
                {
                    channel.QueueDeclare(queue: queue_name,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);


                    var consumer = new EventingBasicConsumer(channel);
                    string tujuan, inputMsg;
                
                
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());
                        Console.WriteLine(" [x] Pesan diterima: {0}", message);
                       
                    };
                    channel.BasicConsume(queue: queue_name,
                                    autoAck: true,
                                    consumer: consumer);

                    Console.WriteLine("Listening... ");
                    Console.WriteLine("Type 'q' to quit, 's' to send message");
                    cmd = Console.ReadLine();
                    

                    if (cmd == "q")
                    {
                        Disconnect();
                    }
                    else if (cmd == "s")
                    {
                        Console.Write("Masukkan pesan yang akan dikirim atau 'exit' to close.\n>>");
                        Console.Write(">> tujuan: ");
                        tujuan = Console.ReadLine();
                        Console.Write(">> pesan: ");
                        inputMsg = Console.ReadLine();
                        SendMessage(tujuan, inputMsg);
                    }
                 } 
                
            } while (cmd != "q");
            if (cmd == "q") { Disconnect(); }
        }

        public void SendMessage(string tujuan, string msg = "send")
        {
            byte[] responseBytes = Encoding.UTF8.GetBytes(msg);// konversi pesan dalam bentuk string menjadi byte

            channel.BasicPublish(exchange: "",
                                    routingKey: tujuan,
                                    basicProperties: null,
                                    body: responseBytes);
            Console.WriteLine("Pesan: '" + msg + "' telah dikirim.");
        }

        public void Disconnect()
        {
            channel.Close();
            channel = null;
            Console.WriteLine("Channel ditutup!");
            if (connection.IsOpen)
            {
                connection.Close();
            }

            Console.WriteLine("Koneksi diputus!");
            connection.Dispose();
            connection = null;
        }
    }
}
