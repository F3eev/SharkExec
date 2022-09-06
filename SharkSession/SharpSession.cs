using System;
using System.IO;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;

namespace SharkSession
{
    class SharpSession
    {


        public static void Run(string ips,string[] userlist,string logpath="", bool showall=false)
        {
           
            List<string> _dcHostNames = new List<string>();
            List<string> _ips = new List<string>();
            var domain = Utils.GetDomainObj();
            //List<domainComputerObj> DomainComputerList = Utils.GetComputersInDomain(domain.Name);
            /*
                            List<DomainController> DomainComputerName = Utils.GetDomainControllers();
                            foreach (DomainController dc in DomainComputerName)
                            {
                                if (!Utils.DoPing(dc.Name, 445)) continue;
                                _dcHostNames.Add(dc.Name);
                            }
            */
            _ips = Utils.ParaIps(ips);

            DumpNetSession dumpSession = new DumpNetSession();
            dumpSession.Run(_ips, userlist, 100, logpath, showall);
        }
        /*
         * 
         * log file
         */
       
    }

}
