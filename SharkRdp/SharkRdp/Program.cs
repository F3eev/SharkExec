using System;
using System.Collections.Generic;
using System.Text;

namespace Shark
{

    class Program
    {
        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkRdp.exe -r all 10
    Arguments:
         -r        - all,log,mstsc default num :10 
    eg: SharkRdp.exe -r all 
        SharkRdp.exe -r mstsc 10
    ";
            Console.WriteLine(banner);
        }

        [Obsolete]
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Usage();
                return;
            }
            string arg0 = args[0];
            string arg1 = args[1];
            int num = 10;
            if (arg0 == "-r")
            {
                if(args.Length == 3 && !int.TryParse(args[2], out num))
                {
                    num = Convert.ToInt32(args[2]);
                }
                switch (arg1.ToLower())
                {
                    case "all":
                        Rdp.GetLog(num);
                        Rdp.GetMstsc(num);
                        break;
                    case "log":
                        Rdp.GetLog(num);
                        break;
                    case "mstsc":
                        Rdp.GetMstsc(num);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
