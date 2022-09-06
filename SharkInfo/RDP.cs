using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Win32;
using SharkInfo.lib;

namespace SharkInfo

{
    class RDP
    {
        [Obsolete]
        public static ArrayList GetLogonEventIp(int num)
        {
            /*      4624: An account was successfully logged on.
                    4625: An account failed to log on.
                    4648: A logon was attempted using explicit credentials.
                    4675: SIDs were filtered.
            */
            ArrayList allIp = new ArrayList();
            int i = 0;
            Common.showMessage("[*]", "Try to get log ip");
            if (IsHighIntegrity())
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
                        String Source = GetOneByPattern(entry.Message, @"源网络地址:\s+(.*?)\s+");

                        if (!allIp.Contains(Source))
                        {
                            allIp.Add(Source);
                            Common.showMessage("[+]", Source);
                        }
                    }
                }
            }
            else
            {
                Common.showMessage("[-]", "Permission not allowed");
            }
            Common.showMessage("[*]", "Try to get log ip end");

            return allIp;
        }

        public static bool IsHighIntegrity()
        {
            // returns true if the current process is running with adminstrative privs in a high integrity context
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
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

        public static ArrayList GetMstscIp(int num)
        {
            ArrayList allIp = new ArrayList();
            RegistryKey rk = Registry.Users;
            ArrayList allSID = GetSID(rk);
            Common.showMessage("[*]", "Try to get mstsc ip");
            foreach (string s in allSID)
            {
                SecurityIdentifier sid = new SecurityIdentifier(s);
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
                        if (!allIp.Contains(h))
                        {
                            allIp.Add(h);
                            Common.showMessage("[+]", h);
                        }
                    }
                }
                catch (System.Security.SecurityException)
                {
                }
                catch (System.NullReferenceException)
                {
                }
            }
            Common.showMessage("[*]", "Try to get mstsc ip end");

            return allIp;
        }
        private static ArrayList GetSID(RegistryKey rkey)
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
