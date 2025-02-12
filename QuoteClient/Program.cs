using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QuoteClient
{
    internal class Program
    {
        private const string serverIp = "127.0.0.1";
        private const int port = 5555;

        public static async Task RunClientAsync()
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    Console.WriteLine("Подключение к серверу...");
                    await client.ConnectAsync(serverIp, port);
                    Console.WriteLine("Подключение установлено");

                    NetworkStream stream = client.GetStream();

                    while(true)
                    {
                        Console.WriteLine("Введите команду (quote или exit)");
                        string command = Console.ReadLine().Trim().ToLower();

                        byte[] requestBytes = Encoding.UTF8.GetBytes(command);
                        await stream.WriteAsync(requestBytes, 0, requestBytes.Length);

                        byte[] buffer = new byte[1024];
                        int readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);

                        string quote = Encoding.UTF8.GetString(buffer, 0, readBytes);

                        if (command.Equals("exit"))
                            break;

                        if (command.Equals("quote")) {     
                            Console.WriteLine(quote);
                        }
                        else
                        {
                            Console.WriteLine("Неверная команда");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static async Task Main(string[] args)
        {
            await RunClientAsync();
        }
    }
}
