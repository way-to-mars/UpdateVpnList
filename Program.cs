using System.Net.Http.Headers;
using System.Text;
using static UpdateVpnList.Logger;

namespace UpdateVpnList
{
    internal class Program
    {
        static void Main(string[] _)
        {
            Settings settings = new();

            Log($"Используемые настройки:\n" +
                $"Url: {settings.Url}\n" +
                $"UDP: {settings.Udp}\n" +
                $"TCP: {settings.Tcp}\n" +
                $"SSTP: {settings.Sstp}");
            Log($"\nВыполняется подключение...");

            Web web = new();

            string listUrlData = web.LoadUrlAsString(settings.Url, out string notification);
            if (listUrlData.Length == 0)
            {
                Log($"Не удалось загрузить страницу {settings.Url}. Текст ошибки:\n{notification}");
                return;
            }

            Log($"Прочитано {listUrlData.Length} байт");

            IReadOnlyList<string> serversList = Parser.AllServersList(listUrlData);
            Log($"Обнаружено {serversList.Count} доступных серверов");

            int updated = 0;
            int created = 0;

            serversList
                .AsParallel()
                .Select((s, i) => new { Index = i, Url = s })
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
                            Log($"{StateString(it.Index, serversList.Count, 'F')} -> Failed: {errorMessage}");
                            break;
                        case FileSaver.ResultType.NEW:
                            Log($"{StateString(it.Index, serversList.Count, 'N')} -> Новый: {saveResult.FileName}");
                            Interlocked.Increment(ref created);
                            break;
                        case FileSaver.ResultType.UPDATE:
                            Log($"{StateString(it.Index, serversList.Count, 'U')} -> Обновлен: {saveResult.FileName}");
                            Interlocked.Increment(ref updated);
                            break;
                    }

                }
                else {
                    Log($"{StateString(it.Index, serversList.Count, 'S')} -> Пропущен: {ProtoType(data)}");
                }
            });

            web.Dispose();
            Log($"Итого создано {created}, обновлено {updated}\n");
            FinalCountDown(15);
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

        static string StateString(int position, int size, char state)
        {
            var sb = new StringBuilder();

            sb.Append('[');
            sb.Append('.', size);
            sb.Append("]");
            sb[position + 1] = state;

            return sb.ToString();
        }
    }
}
