using static UpdateVpnList.Logger;

namespace UpdateVpnList
{
    internal class Program
    {
        static void Main(string[] _)
        {
            Settings settings = new();

            Log($"Url: {settings.Url}");
            Log($"UDP: {settings.Udp}");
            Log($"TCP: {settings.Tcp}");
            Log($"SSTP: {settings.Sstp}");
            Log($"Выполняется подключение к URL...");

            Web web = new();

            string listUrlData = web.LoadUrlAsString(settings.Url, out string notification);
            if (listUrlData.Length == 0)
            {
                Log($"Не удалось загрузить страницу {settings.Url}. Текст ошибки:\n{notification}");
                return;
            }

            Log($"Прочитано {listUrlData.Length} байт");

            IReadOnlyList<string> serversList = Parser.AllServersList(listUrlData);
            Log($"Обнаружено {serversList.Count} серверов");

            serversList.AsParallel<string>().ForAll(server =>
            {
                var data = web.LoadUrlAsString(server, out string localNotification);

                if (Parser.IsUdpProtocol(data) && settings.Udp ||
                    Parser.IsTcpProtocol(data) && settings.Tcp ||
                    Parser.IsSstpProtocol(data) && settings.Sstp)
                {
                    string fileName = FileSaver.WriteFile(data, out var errorMessage);

                    if (fileName == string.Empty) Log($"{server} -> Ошибка: {errorMessage}");
                    else Log($"{server} -> Ok: {fileName}");
                }
                else {
                    Log($"{server} -> Протокол не соответствует настройкам");
                }
            });

            web.Dispose();
            FinalCountDown(10);
        }

        static void FinalCountDown(int seconds = 5)
        {
            string measure = (seconds % 10) switch
            {
                1 => "секунду",
                2 or 3 or 4 => "секунды",
                _ => "секунд",
            };

            Console.Write($"Окно автоматически закроется через {seconds} {measure}");
            while (seconds > 0)
            {
                Console.Write('.');
                Task.Delay(999).Wait();
                seconds--;
            }
            Console.WriteLine();
        }
    }
}
