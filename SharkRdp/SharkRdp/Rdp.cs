using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Shark.lib;
using System.Security.Principal;
using Microsoft.Win32;
namespace Shark
{
    class Rdp
    {
        [Obsolete]
        public static void GetLog(int num)
        {
            /*      4624: An account was successfully logged on.
                    4625: An account failed to log on.
                    4648: A logon was attempted using explicit credentials.
                    4675: SIDs were filtered.
            */
            Common.showMessage("[*]", string.Format("Try to get rdp log  num: {0}", num.ToString()));
            int i = 0;
            if (Common.IsHighIntegrity())
            {
                EventLog eventlog = new EventLog();
                eventlog.Log = "Security";
                EventLogEntryCollection eventLogEntryCollection = eventlog.Entries;
                foreach (EventLogEntry entry in eventLogEntryCollection)
                {
                    if (entry.EventID == 4624 || entry.EventID == 4625)
                    {
                        if (i == num)
                        {
                            break;
                        }
                        i = i + 1;
                        String UserName = GetOneByPattern(entry.Message, @"帐户名称:\s+(.*?)\s+");
                        String Domain = GetOneByPattern(entry.Message, @"帐户域:\s+(.*?)\s+");
                        String Source = GetOneByPattern(entry.Message, @"源网络地址:\s+(.*?)\s+");
                        Common.showMessage("[+]", string.Format("USERNAME: {0} DOAMIN:{1}  IP:{2} ", UserName, Domain, Source));
                    }
                }
            }
            else
            {
                Common.showMessage("[-]", "Permission not allowed");
            }
        }
        


          private static string GetOneByPattern(string input, string pattern)
        {
            Regex regex = new Regex(pattern);
            MatchCollection matchCollection = regex.Matches(input);
            ArrayList resList = new ArrayList();
            foreach (Match item in matchCollection)
            {

                return item.Groups[1].Value;
            }
            return "";
        }


        public static void GetMstsc(int num)
        {
            RegistryKey rk = Registry.Users;
            ArrayList allSID = GetSID(rk);
            foreach (string s in allSID)
            {
                SecurityIdentifier sid = new SecurityIdentifier(s);
                string Username = GetNameFromSID(sid);
                Common.showMessage("[*]", string.Format("Try to get {0} MSTSC Histroy num: {1}", Username, num));
                int i = 0;
                try
                {
                    RegistryKey hostKey = Registry.Users.OpenSubKey(sid + @"\Software\Microsoft\Terminal Server Client\Servers");

                    String[] hosts = hostKey.GetSubKeyNames();
                    foreach (String h in hosts)
                    {
                        if (i == num)
                        {
                            break;
                        }
                        i = i + 1;
                        Common.showMessage("[+]", h);

                    }
                }
                catch (System.Security.SecurityException )
                {
                    Common.showMessage("[-]", string.Format("Access forbidden"));
                }
                catch (System.NullReferenceException)
                {
                }
                Common.showMessage("[*]", string.Format("Try to get {0} MSTSC Histroy num: {1} End", Username, num));
            }
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
        static string GetNameFromSID(SecurityIdentifier sid)
        {
            NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
            return ntAccount.ToString();
        }
    }

}
