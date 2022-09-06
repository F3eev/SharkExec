using System;
using System.Collections.Generic;
using System.Text;

namespace SharkInfo
{
    class Program
    {
        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkInfo.exe -a 
    Arguments:
         -a         ip
    eg: SharkInfo.exe -a ip
    ";
            Console.WriteLine(banner);
        }

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
            if (arg0 == "-a")
            {
                if (args.Length == 3 && !int.TryParse(args[2], out num))
                {
                    num = Convert.ToInt32(args[2]);
                }
                switch (arg1.ToLower())
                {
                    case "ip":
                        Info.GetIP();
                        break;
                    default:
                        break;
                }
            }
            
        }
    }
}
