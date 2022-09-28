using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Text;

namespace Common
{
    public class SettingsManager
    {
        public static string IpServer { get; set; }
        public static string IpClient { get; set; }
        public static string PortServer { get; set; }
        public static string PortClient { get; set; }
        public static void SetupConfiguration(NameValueCollection appSettings)
        {         

            string claveIp = "ip_server";

            string clavePort = "port_server";

            string clavePortClient = "port_client";

            string claveIpClient = "ip_client";

            IpServer = appSettings[claveIp] ?? string.Empty;

            PortServer = appSettings[clavePort] ?? string.Empty;

            PortClient = appSettings[clavePortClient] ?? string.Empty;

            IpClient = appSettings[claveIpClient] ?? string.Empty;

            if (IpServer == string.Empty || PortServer == string.Empty)
            {
                throw new Exception("Not valid Ip or Port");
            }
        }

    }
}
