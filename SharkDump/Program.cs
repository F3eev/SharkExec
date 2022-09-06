using System;
using SharkDump.TeamViewer;

namespace SharkDump
{
    class Program
    {

        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkDump.exe -p tv  
    Arguments:
        -p        tv
        
    eg: SharkDump.exe -p tv
        
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
            if (arg0 == "-p")
            {
                switch (arg1.ToLower())
                {
                    case "tv":
                        TVDump.GetPass();
                        break;
                    default:
                        Usage();
                        break;
                }
            }
            else
            {
                Usage();
                return;
            }
        }
    }
}
