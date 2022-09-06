using System;
using System.Collections.Generic;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.IO;
using System.Collections;

namespace SharkScan
{
    public class Utils
    {

        private static List<string> NetworkToIpRange(string sNetwork)
        {

            uint ip,        /* ip address */
                mask,       /* subnet mask */
                broadcast,  /* Broadcast address */
                network;    /* Network address */

            int bits;
            long startIP;
            long endIP;
            List<string> ipList = new List<string>();

            string[] elements = sNetwork.Split(new Char[] { '/' });
            ip = IPToInt(elements[0]);
            bits = Convert.ToInt32(elements[1]);
            mask = ~(0xffffffff >> bits);
            network = ip & mask;
            broadcast = network + ~mask;
            var usableIps = (bits > 30) ? 0 : (broadcast - network - 1);
            if (usableIps <= 0)
            {
                startIP = endIP = 0;
            }
            else
            {
                startIP = network + 1;
                endIP = broadcast - 1;
            }
            for (long i = startIP; i < endIP; i++)
            {
                string p = IntToIp(i);
                ipList.Add(p);
                //Console.WriteLine(p);
            }
            return ipList;
        }
        public static uint IPToInt(string IPNumber)
        {
            uint ip = 0;
            string[] elements = IPNumber.Split(new Char[] { '.' });
            if (elements.Length == 4)
            {
                ip = Convert.ToUInt32(elements[0]) << 24;
                ip += Convert.ToUInt32(elements[1]) << 16;
                ip += Convert.ToUInt32(elements[2]) << 8;
                ip += Convert.ToUInt32(elements[3]);
            }
            return ip;
        }

        public static string IntToIp(long ipInt)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append((ipInt >> 24) & 0xFF).Append(".");
            sb.Append((ipInt >> 16) & 0xFF).Append(".");
            sb.Append((ipInt >> 8) & 0xFF).Append(".");
            sb.Append(ipInt & 0xFF);
            return sb.ToString();
        }




        public static List<string> ParaIps(string ips)
        {
            List<string> ipList = new List<string>();

            if (ips.IndexOf("-") > 0)
            {
                string[] ipsArray = ips.Split(new char[1] { '-' });
                string startIp = (ipsArray[0]);
                string endIp = (ipsArray[1]);
                for (long i = IPToInt(startIp); i < IPToInt(endIp) + 1; i++)
                {
                    string p = IntToIp(i);
                    ipList.Add(p);
                }
            }
            else if (ips.IndexOf("/") > 0)
            {
                ipList = NetworkToIpRange(ips);
            }
            else if (ips.IndexOf(",") > 0)
            {
                string[] ipsArray = ips.Split(new char[1] { ',' });
                foreach (string s in ipsArray)
                {
                    ipList.Add(s);
                }
            }
            else
            {
                ipList.Add(ips);
            }
            for (int i = 0; i < ipList.Count; i++)  //外循环是循环的次数
            {
                for (int j = ipList.Count - 1; j > i; j--)  //内循环是 外循环一次比较的次数
                {
                    if (ipList[i] == ipList[j])
                    {
                        ipList.RemoveAt(j);
                    }

                }
            }
            return ipList;
        }

         
        public static List<int> ParaPorts(string ports)
        {
            List<int> portList = new List<int>();
            string[] sArray = ports.Split(new char[1] { ',' });
            foreach (string s in sArray)
            {
                if (s.IndexOf("-") > 0)
                {
                    string[] pArray = s.Split(new char[1] { '-' });
                    int startPort = Convert.ToInt32(pArray[0]);
                    int endPort = Convert.ToInt32(pArray[1]);
                    for (int i = startPort; i < endPort + 1; i++)
                    {
                        portList.Add(i);
                    }
                }
                else
                {
                    portList.Add(Convert.ToInt32(s));
                }
            }
            return portList;
        }

        public static byte[] ArrayConcat(byte[] array1, byte[] array2)
        {
            int len = array1.Length + array2.Length;
            byte[] temp = new byte[len];

            for (int i = 0; i <= array1.Length; i++)
            {
                temp[i] = array1[i];
            }
            int t = array1.Length;

            for (int j = 0; j <= array2.Length; j++)
            {
                temp[t] = array2[j];
            }
            return temp;
        }

        public static byte[] ByteTake(byte[] data, int start, int length)
        {
            byte[] temp = new byte[length];
            int j = 0;
            for (int i = start; i < start + length; i++)
            {
                temp[j] = data[i];
                j = j + 1;
            }
            return (temp);
        }

        public static bool onlin_ping(string Ip, int timeout = 1500)
        {
            Ping ping = new Ping();
            PingReply reply = ping.Send(Ip, timeout);
            bool HostStatus;
            if (reply.Status == IPStatus.Success)
            {
                Debug.Write(3, "onlin_ping ping ok" + Ip);
                HostStatus = true;
            }
            else
            {
                if (PortStatus(Ip, 445, timeout))
                {
                    HostStatus = true;
                }
                else if (PortStatus(Ip, 22, timeout))
                {
                    HostStatus = true;
                }
                else
                {
                    HostStatus = false;
                }
            }
            return HostStatus;
        }

        public static bool PortStatus(string Ip, int Port, int timeout = 1500)
        {
            bool PortStatus;
            try
            {
                TcpClient client = new TcpClient();
                IAsyncResult oAsyncResult = client.BeginConnect(Ip, Port, null, null);
                oAsyncResult.AsyncWaitHandle.WaitOne(timeout, true);
                if (client.Connected)
                {
                    PortStatus = true;
                    Debug.Write(3, "PortStatus  tcpclient " + Port + " open" + Ip);
                }
                else
                {
                    Debug.Write(3, "PortStatus  tcpclient " + Port + " close" + Ip);
                    PortStatus = false;
                }
                client.Close();
            }
            catch (SocketException e)
            {
                Debug.Write(3, "PortStatus e.Message");
                PortStatus = false;
            }
            return PortStatus;
        }
        
        public static string Message(string msg,string flag ="[+]")
        {
            //Console.WriteLine(flag + " : " + msg);
            return flag + " : " + msg;
        }

        public static void SaveToFile(string content, string filePath, bool appand)
        {
            FileStream fs;
            if (File.Exists(filePath) && appand)
            {
                fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            }
            StreamWriter sw = new StreamWriter(fs, Encoding.Default);
            sw.Write(content);
            sw.Flush();
            sw.Close();
            fs.Close();
        }
        public static void SaveToFile(List<string> content, string filePath, bool appand)
        {
            FileStream fs;
            try
            {
                if (File.Exists(filePath) && appand)
                {
                    fs = new FileStream(filePath, FileMode.Append, FileAccess.Write);
                }
                else
                {

                   fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

                 
                }
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                foreach (string l in content)
                {
                    sw.Write(l + "\r\n");
                }
                sw.Flush();
                sw.Close();
                fs.Close();
                Console.WriteLine(Message("Out File to "+filePath, "[*]"));
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine(Message(filePath+" File AccessException", "[-]"));
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine(Message(filePath + " File Error ", "[-]"));
            }
           
        }

        
        /*
        * 
        * 根据文件名读取ip
        * 
        */

        public static string ReadIp(string path)
        {
            try
            {
                FileStream fsRead = new FileStream(@path, FileMode.Open);
                int fsLen = (int)fsRead.Length;
                byte[] heByte = new byte[fsLen];
                int r = fsRead.Read(heByte, 0, heByte.Length);
                string text = Encoding.UTF8.GetString(heByte);
                fsRead.Close();
                ArrayList list = new ArrayList();
                string[] ss = text.Split(new string[] { "\n" }, StringSplitOptions.None);
                foreach (string i in ss)
                {
                    string nstr = i.Trim();
                    if (nstr != "")
                    {
                        list.Add(nstr);
                    }
                }
                return string.Join(",", (string[])list.ToArray(typeof(string)));
            }
            catch (System.IO.FileNotFoundException)
            {
                Console.WriteLine("[-]: host file error!");
                return "";
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("[-]: host file error!");
                return "";
            }
        }

    }
}
