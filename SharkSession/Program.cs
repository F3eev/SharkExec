using System;
using System.Collections.Generic;
using System.Threading;
using System.DirectoryServices;
using System.Threading;
using System.Timers;

namespace SharkSession
{
    class Program
    {
        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkSession.exe ip user -r time -out logfile 
    
:       ip      192.168.1.111
        -u     user1,user2
        -l      show all
        -r       6 
        -out    c;\1.log
    eg: SharkSession.exe 192.168.1.111  
        SharkSession.exe 192.168.1.111  -u user1,user2  -l  -r  6  -out C:\1.txt 
       
    ";
            Console.WriteLine(banner);
        }

        
        static int ArrayIndexOf(string [] strline,string flag)
        {
            int index = -1;
            for(int i=0;i< strline.Length; i++)
            {
                if (strline[i] == flag) return i;
            }
            return index;
        }
        static void Main(string[] args)
        {

            string logfile = "";
            string time="";
            string[] _userArray ;
            bool r = false;
            string usernames="";
            bool showall = false;
            if (args.Length < 1)
            {
                Usage();
                return;
            }
            string cmdline = String.Join("", args);

            if (cmdline.Contains("-l") || args.Length ==1 )
            {
                showall = true;
            }
            if (cmdline.Contains("-r"))
            {
                r = true;
                int i =ArrayIndexOf(args, "-r");
                if(i != -1)
                {
                     time = args[i + 1];
                }
            }
            if (cmdline.Contains("-u"))
            {
                r = true;
                int i = ArrayIndexOf(args, "-u");
                if (i != -1)
                {
                    usernames = args[i + 1];
                }
            }
            if (cmdline.Contains("-out"))
            {
                int i = ArrayIndexOf(args,"-out");
                if (i != -1)
                {
                     logfile = args[i + 1];
                }
            }
            string ips = args[0];
            _userArray = usernames.Split(',');
            Console.WriteLine("[*] : Start Dump Session");
            if (r)
            {
                while (true)
                {
                    SharpSession.Run(ips,_userArray, logfile, showall);
                    Thread.Sleep(Convert.ToInt32(time) * 1000);
                }
            }
            else
            {
                SharpSession.Run(ips, _userArray, logfile, showall);
            }
            Console.WriteLine("[*] : Dump Session End");

        }
    }
}
