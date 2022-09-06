using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Forward;

namespace SharkTools
{
    class Program
    {

        static void Usage()
        {
            string banner = @"
        
    Arguments:
        
        -a        pf
        -lp:      8081
        -rh       123.1.1.1 
        -rp        8081

    eg: SharkTools.exe  -a pf -lp 8081 -rh 123.1.1.1 -rp 8081 
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
            if (args.Length != 8 )
            {

                Usage();
                return;
            }
            string cmdline = String.Join("", args);
            string a="",lp="",rh="",rp="";

           

            if (cmdline.Contains("-a"))
            {
                int i = ArrayIndexOf(args, "-a");
                if (i != -1)
                {
                    a = args[i + 1];
                }
            }
            if (cmdline.Contains("-lp"))
            {
                int i = ArrayIndexOf(args, "-lp");
                if (i != -1)
                {
                    lp = args[i + 1];
                }
            }
            if (cmdline.Contains("-rh"))
            {
                int i = ArrayIndexOf(args, "-rh");
                if (i != -1)
                {
                    rh = args[i + 1];
                }
            }

            if (cmdline.Contains("-rp"))
            {
                int i = ArrayIndexOf(args, "-rp");
                if (i != -1)
                {
                    rp = args[i + 1];
                }
            }
            if(a == "pf" && lp != "" && rh !="" && rp!="")
            {
                Tran.start(int.Parse(lp), rh, int.Parse(rp));

                while(true){
                    Thread.Sleep(999999999);
                }

            }
            else
            {
                Usage();
            }

        }
    }
}
