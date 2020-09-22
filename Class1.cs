using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pinger
{
    public class nt
    {
        public long ping(string address)
        {
            long PingResult;
            try
            {
                Ping ping = new Ping();

                PingReply reply = ping.Send(address, 10000);
                PingResult = reply.RoundtripTime;
            }
            catch (Exception)
            {
                PingResult = -1;
            }
            return PingResult;
        }

        public string tracert(string ipAddressOrHostName)
        {
            IPAddress ipAddress = Dns.GetHostEntry(ipAddressOrHostName).AddressList[0];
            StringBuilder traceResults = new StringBuilder();
            using (Ping pingSender = new Ping())
            {
                PingOptions pingOptions = new PingOptions();
                Stopwatch stopWatch = new Stopwatch();
                byte[] bytes = new byte[32];
                pingOptions.DontFragment = true;
                pingOptions.Ttl = 1;
                int maxHops = 30;
                traceResults.AppendLine($"Начало трассировки: {DateTime.Now:HH:mm:ss}{Environment.NewLine}Трассировка до: {ipAddressOrHostName}({ipAddress}){Environment.NewLine}Максимальное число прыжков {maxHops}{Environment.NewLine}");

                traceResults.AppendLine();
                for (int hop = 1; hop < maxHops + 1; hop++)
                {
                    stopWatch.Reset();
                    stopWatch.Start();
                    PingReply pingReply = pingSender.Send(ipAddress, 10000, new byte[32], pingOptions);
                    stopWatch.Stop();
                    traceResults.AppendLine($"{hop}\t{stopWatch.ElapsedMilliseconds} ms\t{pingReply.Address}");

                    if (pingReply.Status == IPStatus.Success)
                    {
                        traceResults.AppendLine();
                        traceResults.AppendLine($"Трассировки завершена: {DateTime.Now:HH:mm:ss}");
                        break;
                    }
                    pingOptions.Ttl++;
                }
            }
            return traceResults.ToString();
        }

        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        public static extern int SendARP(int DestinationIP, int SourceIP, [Out] byte[] pMacAddr, ref int PhyAddrLen);

        public static string ConvertIpToMAC(IPAddress ip)
        {
            byte[] addr = new byte[6];
            int length = addr.Length;

            SendARP(ip.GetHashCode(), 0, addr, ref length);

            return BitConverter.ToString(addr, 0, 6);
        }

        private string getIpMac()
        {
            StringBuilder ansv = new StringBuilder();
            IPHostEntry ip2 = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var item in ip2.AddressList)
            {

                if (item.AddressFamily == AddressFamily.InterNetwork)
                    ansv.AppendLine("IP адрес:" + item.ToString() + " Маска:") ;// + GetSubnetMask(item).ToString()
                else
                    ansv.AppendLine("IP адрес:" + item.ToString() + " Область:" + item.ScopeId.ToString());

                ansv.AppendLine();
            }
            return ansv.ToString();
        }
    }
}
