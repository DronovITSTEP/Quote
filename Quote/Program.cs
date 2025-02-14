using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
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

        private static int maxCountClient = 2;
        private static int activeClient = 0;
        private static object clientObject = new object();

        private static Dictionary<string, string> users = new Dictionary<string, string>()
        {
            {"user1", "1234" },
            { "user2", "4321" }
        };

        //запуск сервера
        public static  void StartServerAsync()
        {
            try
            {
                TcpListener tcp = new TcpListener(System.Net.IPAddress.Any, PORT);
                tcp.Start();
                Console.WriteLine("Сервер запущен. Ожидаем подключения клиентов");

                while (true)
                {
                    TcpClient client = tcp.AcceptTcpClient();
                    lock (clientObject)
                    {
                        if (activeClient >= maxCountClient)
                        {
                            using (NetworkStream stream = client.GetStream())
                            {
                                using (StreamWriter writer = new StreamWriter(stream) { AutoFlush = true })
                                {
                                    writer.WriteLine("disconnect");
                                    writer.WriteLine("Сервер перегружен. Повторите попытку позже");
                                }
                            }
                            client.Close();
                            continue;
                        }
                        activeClient++;
                    }

                    Thread thread = new Thread(() => 
                        HandleClientAsync(client));

                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка сервера: " + ex.Message);
            }

        }

        private static void HandleClientAsync(TcpClient client)
        {
            NetworkStream stream = null;
            StreamReader reader = null;
            StreamWriter writer = null;

            string clientId = Guid.NewGuid().ToString().Substring(0, 8);
            try
            { 
                stream = client.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                writer.WriteLine("Введите логин");
                string username = reader.ReadLine();

                writer.WriteLine("Введите пароль");
                string password = reader.ReadLine();

                if (!users.ContainsKey(username) || users[username] != password)
                {
                    writer.WriteLine("Неверный логи или пароль");
                    throw new Exception("неверные данные");
                }

                writer.WriteLine("access");

                bool isConected = true;
                LogEvent($"Клиент {clientId} подключился.");
                int countRequest = 0;
                while (isConected && countRequest < 3)
                {                   
                    string request = reader.ReadLine();

                    string response = "";
                    if (request?.Equals("quote") == true) 
                    {
                        response = GetRandomQuote();                  
                        LogEvent($"Клиенту {clientId} отправлена цитата {response}");                    
                        countRequest++;

                        if (countRequest == 3)
                        {
                            response = "Закончен лимит цитат ";
                            Console.WriteLine("Закончен лимит");
                        }
                        writer.WriteLine(response);
                    }
                    else if(request?.Equals("exit") == true)
                    {
                        isConected = false;
                        LogEvent($"Клиент {clientId} отправил запрос на отключение");
                    }
                    else
                    {
                        response = "Запрос некорректный. " +
                            "Отправьте \"qoute\" или \"exit\"";
                        writer.WriteLine(response);

                        LogEvent($"Клиент {clientId} отправил неверный запрос");
                    }

                    
                }
            }
            catch(Exception ex)
            {
                LogEvent($"Ошибка обработки клиента {client}: {ex.Message}");
            }
            finally
            {
                reader.Close();
                writer.Close();
                stream.Close();
                client.Close();
                activeClient--;
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

        static void Main(string[] args)
        {
            StartServerAsync();
        }
    }
}
