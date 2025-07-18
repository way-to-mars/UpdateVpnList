using System.Net.Http.Headers;
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

            int updated = 0;
            int created = 0;

            serversList
                .AsParallel()
                .Select((s, i) => new { Name = WithPadding(i + 1), Url = s })
                .ForAll(it =>
            {
                var data = web.LoadUrlAsString(it.Url, out string localNotification);

                if (Parser.IsUdpProtocol(data) && settings.Udp ||
                    Parser.IsTcpProtocol(data) && settings.Tcp ||
                    Parser.IsSstpProtocol(data) && settings.Sstp)
                {
                    var saveResult = FileSaver.WriteFile(data, out var errorMessage);

                    switch (saveResult.State)
                    {
                        case FileSaver.ResultType.FAIL:
                            Log($"{it.Name} -> Failed: {errorMessage}");
                            break;
                        case FileSaver.ResultType.NEW:
                            Log($"{it.Name} -> Новый: {saveResult.FileName}");
                            Interlocked.Increment(ref created);
                            break;
                        case FileSaver.ResultType.UPDATE:
                            Log($"{it.Name} -> Обновлен: {saveResult.FileName}");
                            Interlocked.Increment(ref updated);
                            break;
                    }

                }
                else {
                    Log($"{it.Name} -> Пропущен: {ProtoType(data)}");
                }
            });

            web.Dispose();
            Log($"Итого создано {created}, обновлено {updated}");
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

        static string ProtoType(string data)
        {
            if (Parser.IsUdpProtocol(data)) return "UDP";
            if (Parser.IsTcpProtocol(data)) return "TCP";
            if (Parser.IsSstpProtocol(data)) return "SSTP";
            return "Unknown";
        }

        static string WithPadding(int number)
        {
            return number.ToString().PadLeft(3, '.');
        }
    }
}
