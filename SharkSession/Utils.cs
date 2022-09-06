using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Sockets;
using System.DirectoryServices;
using System.Text;

namespace SharkSession
{

    class domainComputerObj
    {
        public domainComputerObj(string hostname, string os)
        {
            Hostname = hostname;
            Os = os;

        }
        public string Hostname;
        public string Os;
    }

    internal class Utils
    {
        private static readonly Dictionary<string, Domain> _domainCache = new Dictionary<string, Domain>();
        private static readonly Dictionary<string, List<DomainController>> _domainDcCache = new Dictionary<string, List<DomainController>>();

        /*
         * 获取域控主机
         * 
         */
        public static List<DomainController> GetDomainControllers(string domainName = "")
        {
            var domain = GetDomainObj(domainName);
            if (_domainDcCache.TryGetValue(domain.Name, out List<DomainController> DomainDcListcache))
            {
                return DomainDcListcache;
            }
          
            List<DomainController> DomainDcList = new List<DomainController> { };
           
            
            // Try the PDC first
            if (domain == null)
            {
                _domainCache.Add(domainName, null);
            }

            //If the PDC isn't reachable loop through the rest
            foreach (DomainController domainController in domain.DomainControllers)
            {
                var name = domainController.Name;
                //Console.WriteLine(name);
                //if (!DoPing(name, port)) continue;
                //Console.WriteLine($"Found usable Domain Controller for {domain.Name} : {name}");

                DomainDcList.Add(domainController);
            }
            _domainDcCache.Add(domain.Name, DomainDcList);

            return DomainDcList;
        }

       
        /*
         * 探测389开放说明是域控
         * 探测445判断主机是不是在线
         */

        public static bool DoPing(string hostname, int port = 445)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var result = client.BeginConnect(hostname, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(2000);
                    if (!success)
                    {
                        //Console.WriteLine($"{hostname} did not respond to ping");
                        return false;
                    }
                    client.EndConnect(result);
                }
            }
            catch
            {
                Console.WriteLine($"{hostname} did not respond to ping");
                return false;
            }
            return true;
        }
        

        /*
         * 
         * 返回域内计算机的对象 
         */

        public static List<domainComputerObj> GetComputersInDomain(string domainName,string OS="")
        {
            List<domainComputerObj> domainComputerObjList = new List<domainComputerObj>();
            DirectoryEntry de = new DirectoryEntry("LDAP://" + domainName);
            DirectorySearcher ADSearcher = new DirectorySearcher(de);
           
            if (OS == "")
            {
                ADSearcher.Filter = "(objectClass=computer)";
            }
            else
            {
                ADSearcher.Filter = "(&(objectClass=computer)(operatingSystem="+ OS + "*))";
            }
            
            foreach (SearchResult SR in ADSearcher.FindAll())
            {
               domainComputerObjList.Add(new domainComputerObj(SR.Properties["name"][0].ToString(), SR.Properties["operatingsystem"][0].ToString()));
            }        
            return domainComputerObjList;
        }
        /*
         * 获取当前域对象
         * domain为空时获取当前domain
         * ldap存在时通过ldap查询
         */
        public static Domain GetDomainObj(string ldapUsername = null,string ldapPass= null, string domainName = null)
        {
            var key = domainName ?? "UNIQUENULL";
            if (_domainCache.TryGetValue(key, out Domain domainObj))
            {
                //Console.WriteLine("_domainCache get cache");
                return domainObj;
            }
            try
            {
                //Console.WriteLine("_domainCache");
                if (domainName == null)
                {
                     domainObj = Domain.GetCurrentDomain();
                }
                else
                {
                    var context = ldapUsername != null
                        ? new DirectoryContext(DirectoryContextType.Domain, domainName, ldapUsername,
                            ldapPass)
                        : new DirectoryContext(DirectoryContextType.Domain, domainName);

                    domainObj = Domain.GetDomain(context);
                }
            }
            catch
            {
                domainObj =  null;
            }
            _domainCache.Add(key, domainObj);
            return domainObj;
        }
        public static List<string> ParaIps(string ips)
        {
            List<string> ipList = new List<string>();

            if (ips.IndexOf("-") > 0)
            {
                string[] ipsArray = ips.Split(new char[1] { '-' });
                string startIp = (ipsArray[0]);
                string endIp = (ipsArray[1]);
                ;
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
    }

   
}
