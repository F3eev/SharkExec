using System;
using Microsoft.Win32;
using System.Security.Principal;
using System.Collections.Generic;
using System.Collections;
using SharkInfo.lib;
using System.Net;

namespace SharkInfo
{
    class Browser
    {

        public static ArrayList GetIEip(int num)
        {

            Common.showMessage("[*]", "Try to get IE ip");

            RegistryKey rk = Registry.Users;
            ArrayList IEIp = new ArrayList();
            ArrayList allSID = GetSID(rk);
            foreach (string s in allSID)
            {
                SecurityIdentifier sid = new SecurityIdentifier(s);
                int i = 0;
                try
                {
                    RegistryKey UrlKey = Registry.Users.OpenSubKey(sid + @"\Software\\Microsoft\\Internet Explorer\\TypedURLs");
                   foreach (string value in UrlKey.GetValueNames())
                    {
                        if (i == num)
                        {
                            break;
                        }
                        i = i + 1;

                        string url = UrlKey.GetValue(value).ToString();

                        try
                        {
                            var uri = new Uri(url);
                            string Host = uri.Host;
                            try
                            {
                                IPAddress[] IPs = Dns.GetHostAddresses(Host);
                                foreach (IPAddress ip in IPs)
                                {
                                    if (!IEIp.Contains(ip))
                                    {
                                        IEIp.Add(ip.ToString());
                                        Common.showMessage("[+]", url + " ---> " + ip.ToString());
                                    }
                                }
                            }
                            catch (System.Net.Sockets.SocketException)
                            {
                                Common.showMessage("[-]", url + " ---> timeout");
                            }
                        }
                        catch (System.UriFormatException)
                        {
                            Common.showMessage("[-]", url + " --->  timeout" );
                        }
                    }
                }
                catch (System.Security.SecurityException )
                {
                }
            }
            Common.showMessage("[*]", "Try to get IE ip end");
            return IEIp;
        }
        static ArrayList GetSID(RegistryKey rkey)
        {

            ArrayList allSID = new ArrayList();
            String[] names = rkey.GetSubKeyNames();
            foreach (String s in names)
            {
                if (s.ToString().Length == 45)
                {
                    allSID.Add(s.ToString());
                }
            }
            return allSID;
        }
    }
}
