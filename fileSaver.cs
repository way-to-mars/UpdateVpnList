using static UpdateVpnList.Logger;
using System.Text;


namespace UpdateVpnList
{
    internal class fileSaver
    {

        private static readonly string filePrefix = "vpnobratno.info";
        private static readonly string fileExtention = "ovpn";

        public static void WriteFile(string id, string data) {
            var fileName = $"{filePrefix}_{id}.{fileExtention}";
            try
            {
                using (StreamWriter writer = new StreamWriter(fileName, false, Encoding.UTF8))
                {
                    writer.Write(data);
                    Log($"Данные сохранены в {fileName}");
                }
            } catch(Exception ex)
            {
                Log($"Ошибка записи в файл: {ex.Message}");
            }
        }
    }
}
