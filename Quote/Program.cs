using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace QuoteServer
{
    internal class Program
    {
        private const int PORT = 5555;
        private static List<string> quotes = new List<string>
        {
            "Не ешь желтый снег",
            "Носи двое штанов зимой",
            "Без труда не выловишь и рыбку и пруда",
            "Когда рак на горе свистнет"
        };

        //запуск сервера
        public static async Task StartServerAsync()
        {
            try
            {
                TcpListener tcp = new TcpListener(System.Net.IPAddress.Any, PORT);
                tcp.Start();
                Console.WriteLine("Сервер запущен. Ожидаем подключения клиентов");

                while (true)
                {
                    TcpClient client = await tcp.AcceptTcpClientAsync();

                    await HandleClientAsync(client).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            Console.WriteLine($"Ошибка обработки " +
                                $"запроса от клиента. {t.Exception}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сервера: " + ex.Message);
            }

        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = null;
            string clientId = Guid.NewGuid().ToString().Substring(0, 8);

            try
            {
                stream = client.GetStream();
                bool isConected = true;
                LogEvent($"Клиент {clientId} подключился.");
                int countRequest = 0;
                while (isConected && countRequest < 3)
                {
                    byte[] buffer = new byte[256];
                    int bytesRead = await stream
                        .ReadAsync(buffer, 0, buffer.Length);

                    string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    byte[] responseBytes = null;
                    string response = "";
                    if (request.Equals("quote")) 
                    {
                        response = GetRandomQuote();                  
                        LogEvent($"Клиенту {clientId} отправлена цитата {response}");                    
                        countRequest++;

                        if (countRequest == 3)
                        {
                            response = "Закончен лимит цитат ";
                            Console.WriteLine("Закончен лимит");
                        }
                    }
                    else if(request.Equals("exit"))
                    {
                        isConected = false;
                        LogEvent($"Клиент {clientId} отправил запрос на отключение");
                    }
                    else
                    {
                        response = "Запрос некорректный. " +
                            "Отправьте \"qoute\" или \"exit\"";               
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

                        LogEvent($"Клиент {clientId} отправил неверный запрос");
                    }

                    responseBytes = Encoding.UTF8.GetBytes(response);
                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
            catch(Exception ex)
            {
                LogEvent($"Ошибка обработки клиента {client}: {ex.Message}");
            }
            finally
            {
                stream.Close();
                client.Close();
                LogEvent($"Клиент {clientId} отключился");
            }
        }

        private static string GetRandomQuote()
        {
            Random random = new Random();
            return quotes[random.Next(quotes.Count)];
        }

        private static void LogEvent(string message)
        {
            string filePath = "C:/top/log.txt";
            string logEntry = $"{DateTime.Now}: {message}";

            File.AppendAllText(filePath, logEntry + Environment.NewLine);
            Console.WriteLine(logEntry);
        }

        static async Task Main(string[] args)
        {
            await StartServerAsync();
        }
    }
}
