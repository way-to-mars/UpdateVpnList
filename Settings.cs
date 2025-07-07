namespace UpdateVpnList;

internal class Settings
{
    //private static readonly string staticUrl = "https://vpnobratno.info/russia.html";
    private static readonly string staticUrl = "https://vpnobratno.info/russia_server_list.html";
    private static readonly string fileName = "settings.ini";

    public string Url { get; private set; }
    public bool Udp {  get; private set; }
    public bool Tcp { get; private set; }
    public bool Sstp { get; private set; }


    public Settings()
    {
        var fileName = "settings.ini";

        try
        {
            using StreamReader sr = new(fileName);
            
            Url = sr.ReadLine() ?? staticUrl;

            Udp = (sr.ReadLine() ?? "").ToLower().Contains("true");
            Tcp = (sr.ReadLine() ?? "").ToLower().Contains("true");
            Sstp = (sr.ReadLine() ?? "").ToLower().Contains("true");
        }
        catch {
            // Restore defaults
            SetDefaults();
        }
    }

    private void SetDefaults() {
        // set defaults
        Url = staticUrl;
        Udp = true;
        Tcp = false;
        Sstp = false;
    }

}


