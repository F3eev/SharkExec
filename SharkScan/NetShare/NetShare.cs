using System;
using System.Collections.Generic;
using System.Text;

namespace SharkScan.NetShare
{
    class NetShare
    {
        static public List<string> GetshareUNC(string servername)
        {
            List<string> shareUNC = new List<string>() { };
            ShareCollection shi = ShareCollection.LocalShares;
            if (servername != null && servername.Trim().Length > 0)
            {
                shi = ShareCollection.GetShares(servername);
                if (shi != null)
                {
                    foreach (Share si in shi)
                    {
                        string netName = si.NetName;
                        shareUNC.Add(si.NetName);
                    }
                }
                else
                    Console.WriteLine("Unable to enumerate the shares on {0}.\n"
                        + "Make sure the machine exists, and that you have permission to access it.",
                        servername);
            }
            return shareUNC;
        }
    }
}
