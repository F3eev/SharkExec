using Shark.Credentials;
using System;


namespace Shark
{
    class Program
    {

        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkCredentials.exe -c all
    Arguments:
         -c        - all,web,windows 
    eg: SharkCredentials.exe -c web
        SharkCredentials.exe all
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
            if (arg0 == "-c")
            {
                switch (arg1.ToLower())
                {
                    case "all":
                        Web.GetLogins();
                        Windows.GetLogins();
                        break;
                    case "web":
                        Web.GetLogins();
                        break;
                    case "windows":
                        Windows.GetLogins();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
