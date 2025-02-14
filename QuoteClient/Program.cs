using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace QuoteClient
{
    internal class Program
    {
        private const string serverIp = "127.0.0.1";
        private const int port = 5555;

        private static NetworkStream stream;
        private static StreamWriter writer;
        private static StreamReader reader;

        public static void RunClientAsync()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    Console.WriteLine("Подключение к серверу...");
                    client.Connect(serverIp, port);
                    Console.WriteLine("Подключение установлено");

                    NetworkStream stream = client.GetStream();
                    StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };
                    StreamReader reader = new StreamReader(stream);

                    Console.WriteLine(reader.ReadLine());
                    string username =  Console.ReadLine();
                    writer.WriteLine(username);

                    Console.WriteLine(reader.ReadLine());
                    string password = Console.ReadLine();
                    writer.WriteLine(password);

                    string connect = reader.ReadLine();
                    if (connect == "disconnect")
                    {
                        Console.WriteLine(reader.ReadLine());
                        throw new Exception("error");
                    }

                    if (connect != "access")
                    {
                        Console.WriteLine(reader.ReadLine());
                        throw new Exception("error");
                    }

                    while (true)
                    {
                        Console.WriteLine("Введите команду (quote или exit)");
                        string command = Console.ReadLine().Trim().ToLower();

                        writer.WriteLine(command);

                        string quote = reader.ReadLine();

                        Console.WriteLine(quote);
                    }                
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void Main(string[] args)
        {
            RunClientAsync();
        }
    }
}
