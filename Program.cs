using static UpdateVpnList.Logger;

namespace UpdateVpnList
{
    internal class Program
    {
        private static readonly string listUrl = "https://vpnobratno.info/russia.html";

        static void Main(string[] args)
        {
            Log("Проверка доступа к интернету...");
            Web web = new();

            int internetQuality = (int)(web.CheckInternetConnection() * 100);
            Log($"Доступность соединения: {internetQuality}%");

            string listUrlData = web.LoadUrlAsString(listUrl, out string notification);
            if (listUrlData.Length == 0) {
                Log($"Не удалось загрузить страницу {listUrl}. Текст ошибки:\n{notification}");
                return;
            }

            var list = Parser.ServersList(listUrlData);

            foreach ( var server in list )
            {
                var id = getId(server);
                var data = web.LoadUrlAsString(server, out notification);

                if (data.Length == 0)
                {
                    Log($"Ошибка доступа к {server}\n{notification}");
                }
                else
                {
                    Log(server);
                    fileSaver.WriteFile(id, data);
                }
            }
        }

        static string getId(string url)
        {
            const string header = "download_id=";
            var index = url.IndexOf(header);

            if ( index >= 0 )
            {
                try
                {
                    return url.Substring(index + header.Length);
                }
                catch { /* Do nothing */ }
            }

            long time = DateTime.Now.ToBinary();
            return $"noID[{time}]";
        }
    }
}
