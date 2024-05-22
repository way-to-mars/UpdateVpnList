using static UpdateVpnList.Logger;

namespace UpdateVpnList
{
    internal class Program
    {
        private static readonly string listUrl = "https://vpnobratno.info/russia.html";

        static void Main(string[] _)
        {
            Log("Проверка доступа к интернету...");
            Web web = new();

            int internetQuality = web.CheckInternetConnection();
            Log($"Доступность соединения: {internetQuality}%");

            string listUrlData = web.LoadUrlAsString(listUrl, out string notification);
            if (listUrlData.Length == 0) {
                Log($"Не удалось загрузить страницу {listUrl}. Текст ошибки:\n{notification}");
                return;
            }

            IReadOnlyList<string> serversList = Parser.AllServersList(listUrlData);

            serversList.AsParallel<string>().ForAll(server =>
            {
                var data = web.LoadUrlAsString(server, out notification);

                if (data.Length == 0)
                {
                    Log($"Ошибка доступа к {server}\n{notification}");
                }
                else
                {
                    string fileName = FileSaver.WriteFileIfUDP(data, out var errorMessage);
                    if (fileName == string.Empty)
                    {
                        Log($"{server} -> Ошибка: {errorMessage}");
                    }
                    else
                    {
                        Log($"{server} -> Ok: {fileName}");
                    }
                }
            });

            web.Dispose();
            FinalCountDown(10);
        }

        static void FinalCountDown(int seconds = 5) {
            string measure = (seconds % 10) switch
                            {
                                1 => "секунду",
                                2 or 3 or 4 => "секунды",
                                _ => "секунд",
                            };

            Console.Write($"Окно автоматически закроется через {seconds} {measure}");
            while (seconds > 0) { 
                Console.Write('.');
                Task.Delay(999).Wait();
                seconds--;
            }
            Console.WriteLine();        
        }
    }
}
