using System;
using System.Collections.Generic;
using SharkScan.Exp.Ms17010;
namespace SharkScan
{

    
    class Program
    {
        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkScan.exe action [-ips|-ipf] -p -tp -A -ping
    Arguments:
        action     port | alive | netshare| ms17010
        -ips:      127.0.0.1-127.0.0.24 | 127.0.0.1/24 | 127.0.0.1,127.0.0.2 
        -ipf       c:\host.txt 
        -p         80,8080|80-88
        -tp        default 0
        -A         get server name
        -ping      ping 

    eg: SharkScan.exe  port -ips 192.168.220.1/24 -p 30,31 -tp 10 -out D:\12.txt
        SharkScan.exe  ms17010 -ipf c:\1.txt -out D:\12.txt
        SharkScan.exe  alive -ips 192.168.47.99-192.168.47.200 -out D:\12.txt
        SharkScan.exe  netshare -ips 192.168.47.99-192.168.47.200 -out D:\12.txt
    ";
            Console.WriteLine(banner);
        }

        static int ArrayIndexOf(string[] strline, string flag)
        {
            int index = -1;
            for (int i = 0; i < strline.Length; i++)
            {
                if (strline[i] == flag) return i;
            }
            return index;
        }
        static void Main(string[] args)
        {
            if (args.Length <3)
            {
              
                Usage();
                return;      
            }
            DateTime beforeDT = System.DateTime.Now;
            string outfile = "";
            string topports = "0";
            string ports = "";

            List<string> argsList = new List<string>(args);
            string action = args[0];
            string ipargv = args[1];
           

            bool GetServer = false;
            bool ping = false;
            string ips = "";


            if (ipargv == "-ips")
            {
                 ips = args[2];
            }else if(ipargv == "-ipf")
            {
                 ips = Utils.ReadIp(args[2]);
            }else
            {
                Usage();
                return;
            }

            SharkScan SS = new SharkScan(ips, 200);
            string cmdline = String.Join("", args);
            if (cmdline.Contains("-A"))
            {
                GetServer = true;
            }
            if (cmdline.Contains("-ping"))
            {
                ping = true;
            }

            if (cmdline.Contains("-out"))
            {
                outfile = args[args.Length - 1];
            }
            if (action == "alive")
            {
                SS.RunPing(outfile);
            }
            if (action == "netshare")
            {
                SS.RunNetShareScan(outfile);
            }

            else if (action == "port")
            {
                if (cmdline.Contains("-tp"))
                {
                   
                    int i = ArrayIndexOf(args, "-tp");
                    if (i != -1)
                    {
                        topports = args[i + 1];
                        if (int.Parse(topports) >= 1000)
                        {
                            topports = "1000";
                        }
                    }
                }
                if (cmdline.Contains("-p"))
                {

                    int i = ArrayIndexOf(args, "-p");
                    if (i != -1)
                    {
                        ports = args[i + 1];
                    }
                }
                if(topports == "0" && ports == "")
                {
                    Usage();
                    return;
                }


                SS.RunPort(ports, int.Parse(topports), ping, GetServer, outfile);
            }
            else if(action == "ms17010")
            {
                SS.RunExp("ms17010", outfile);
            }
            else
            {
                Usage();
                return;
            }

/*            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforeDT);
            Console.WriteLine("DateTime costed for Shuffle function is: {0}ms", ts.TotalMilliseconds);*/
        }
        
    }
}
