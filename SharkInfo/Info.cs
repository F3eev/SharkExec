using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.NetworkInformation;
using System.Collections;
using System.Net.Sockets;
using SharkInfo.lib;

namespace SharkInfo
{

    class Info
    {
        public static ArrayList GetLocalIP()
        {
            Common.showMessage("[*]", "Try to get local ip");
            string name = Dns.GetHostName();
            ArrayList LocalIp = new ArrayList();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            foreach (IPAddress ipa in ipadrlist)
            {
                if (ipa.AddressFamily == AddressFamily.InterNetwork)
                {
                    LocalIp.Add(ipa.ToString());
                    Common.showMessage("[+]", ipa.ToString());
                }
            }
            Common.showMessage("[*]", "Try to get local ip end");
            return LocalIp;
            
        }

        public static ArrayList GetTcpConnectionsIp()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            ArrayList TcpIp = new ArrayList();
            Common.showMessage("[*]", "Try to get netstat ip");

            foreach (TcpConnectionInformation t in connections)
            {
                string val = t.RemoteEndPoint.ToString() + t.State;
                string[] sArray = val.Split(new char[1] { ':' });
                string ip = sArray[0];
                
                if (!TcpIp.Contains(ip))
                {
                    TcpIp.Add(ip);
                    Common.showMessage("[+]", ip);
                }
            }
            Common.showMessage("[*]", "Try to get netstat ip end");

            return TcpIp;
        }

        

        [Obsolete]
        public static void GetIP()
        {
            ArrayList MstscIp = RDP.GetMstscIp(200);
            ArrayList LogonEventIp = RDP.GetLogonEventIp(200);
            ArrayList TcpConnectionsIp = GetTcpConnectionsIp();
            ArrayList LocalIp = GetLocalIP();
            ArrayList IEIp = Browser.GetIEip(200);
            ArrayList AllIp = new ArrayList();
            ArrayList PrivateIp = new ArrayList();
            foreach (string ip in TcpConnectionsIp)
            {
                if (!AllIp.Contains(ip))
                {
                    AllIp.Add(ip);
                }
            }
            foreach (string ip in MstscIp)
            {
                if (!AllIp.Contains(ip))
                {
                    AllIp.Add(ip);
                }
            }
            foreach (string ip in LogonEventIp)
            {
                if (!AllIp.Contains(ip))
                {
                    AllIp.Add(ip);
                }
            }
            foreach (string ip in LocalIp)
            {
                if (!AllIp.Contains(ip))
                {
                    AllIp.Add(ip);
                }
            }
            foreach (string ip in IEIp)
            {
                if (!AllIp.Contains(ip))
                {
                    AllIp.Add(ip);
                }
            }
            Common.showMessage("[*]", "All addresses");
            foreach (string ip in AllIp)
            {
                string[] ipArray = ip.Split(new char[1] { '.' });
                Common.showMessage("[+]", ip);
                if (ipArray[0] == "172" || ipArray[0] == "192" || ipArray[0] == "10")
                {
                    string I = ipArray[0] +"."+ ipArray[1] + "." + ipArray[2] ;
                    if (!PrivateIp.Contains(I))
                    {
                        PrivateIp.Add(I);
                    }
                }
            }
            Common.showMessage("[*]", "Private addresses");
            foreach (string ip in PrivateIp)
            {

                Common.showMessage("[+]", ip+".0/24");

            }
            Common.showMessage("[*]", "All addresses end");

        }
    }
}
