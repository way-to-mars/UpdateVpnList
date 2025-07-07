namespace UpdateVpnList;

internal class Parser
{
    private static readonly string blockHeader = "<div class=\"server\"><p>";
    private static readonly string hrefHeader = "href=\"";

    public static IReadOnlyList<string> ServersListFirstInGroup(string data) {
        var resultList = new List<string>();

        var index = 0;

        while (true)
        {
            var blockIndex = data.IndexOf(blockHeader, index);
            if (blockIndex == -1) break;

            index = blockIndex + blockHeader.Length;

            var hrefIndex = data.IndexOf(hrefHeader, index);
            if (hrefIndex == -1) break;

            index = hrefIndex + hrefHeader.Length;
            var secondQuoteIndex = data.IndexOf('\"', index);
            if (secondQuoteIndex == -1) break;

            string url = data[index..secondQuoteIndex];
            resultList.Add(url);
        }

        return resultList;
    }

    public static IReadOnlyList<string> AllServersList(string data)
    {
        var resultList = new List<string>();

        var index = 0;

        while (true)
        {
            var hrefIndex = data.IndexOf(hrefHeader, index);
            if (hrefIndex == -1) break;

            index = hrefIndex + hrefHeader.Length;
            var secondQuoteIndex = data.IndexOf('\"', index);
            if (secondQuoteIndex == -1) break;

            string url = data[index..secondQuoteIndex];

            if (url.Contains("download_id"))
                resultList.Add(url);
        }

        return resultList;
    }

    public static bool IsUdpProtocol(string data)
    {
        if (data.StartsWith("proto udp")) return true;
        return data.Contains("\nproto udp");
    }

    public static bool IsTcpProtocol(string data)
    {
        if (data.StartsWith("proto tcp")) return true;
        return data.Contains("\nproto tcp");
    }

    public static bool IsSstpProtocol(string data)
    {
        if (data.StartsWith("proto sstp")) return true;
        return data.Contains("\nproto sstp");
    }
}
