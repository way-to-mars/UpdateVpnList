﻿using System.IO;
using System.Text;
using static UpdateVpnList.FileSaver;


namespace UpdateVpnList;

public class FileSaver
{
    public enum ResultType {
        FAIL,
        NEW,
        UPDATE
    }

    public struct SaveResult {
        public string FileName;
        public ResultType State;

        public SaveResult(string fileName, ResultType resultType) : this()
        {
            FileName = fileName;
            State = resultType;
        }
    }


    private static readonly string fileExtension = "ovpn";

    /// <summary>
    /// Writes text data to .ovpn file. The name of that file corresponds to vpn server (written inside)
    /// </summary>
    /// <param name="data">.ovpn data</param>
    /// <param name="errorMessage">receives exception message</param>
    /// <returns>The name of a file if success or empty string</returns>
    public static SaveResult WriteFile(string data, out string errorMessage) {
        var serverName = ParseServerName(data);
        var fileName = $"{serverName}.{fileExtension}";

        try
        {
            ResultType resultType = File.Exists(fileName) ? ResultType.UPDATE : ResultType.NEW;

            using StreamWriter writer = new(fileName, false, Encoding.UTF8);
            writer.Write(data);

            errorMessage = "";
            return new SaveResult(fileName, resultType);
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return new SaveResult(fileName, ResultType.FAIL);
        }
    }


    ///// <summary>
    ///// Writes text data to .ovpn file if it contains 'proto udp' property. The name of that file corresponds to vpn server (written inside)
    ///// </summary>
    ///// <param name="data">.ovpn data</param>
    ///// <param name="errorMessage">receives exception message</param>
    ///// <returns>The name of a file if success or empty string</returns>
    //public static string WriteFileIfUDP(string data, out string errorMessage)
    //{
    //    if (!IsUdpProtocol(data))
    //    {
    //        errorMessage = "Не является протоколом UDP";
    //        return string.Empty;
    //    }

    //    var serverName = ParseServerName(data);
    //    var fileName = $"{serverName}.{fileExtension}";

    //    try
    //    {
    //        using StreamWriter writer = new(fileName, true, Encoding.UTF8);
    //        writer.Write(data);
    //        errorMessage = string.Empty;
    //        return fileName;
    //    }
    //    catch (Exception ex)
    //    {
    //        errorMessage = ex.Message;
    //        return string.Empty;
    //    }
    //}

    private static string ParseServerName(string data)
    {
        const string shortPrefix = "remote ";
        const string commonPrefix = $"\n{shortPrefix}";

        int startIndex;

        if (data.StartsWith(shortPrefix)) startIndex = shortPrefix.Length;
        else {
            startIndex = data.IndexOf(commonPrefix);
            if (startIndex == -1 ) return String.Empty;
            startIndex += commonPrefix.Length;
        }
        /**
         * EndOfLine:
         *   GNU/Linux — \n;
         *   Apple Macintosh(Mac) — \r;
         *   Microsoft Windows — \r\n.
         *  Assume the Apple format is not used at all. So '\n' is always present and '\r' is optional
         */
        int endIndex = data.IndexOf('\n', startIndex);
        if (endIndex == -1) endIndex = data.Length;

        return data[startIndex..endIndex].Replace('.', '_').Replace(' ', '_').Trim('\r');  
    }


}
