using System;


namespace SharkMonitor
{
    class Program
    {
        static void Usage()
        {
            string banner = @"
        
    Arguments:
        
        -t   Time interval per scan( default:3000)

    eg: SharkMonitor.exe -t 50000 
    ";
            Console.WriteLine(banner);
        }
        private static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Usage();
                return;
            }
            string arg0 = args[0];
            string arg1 = args[1];
            int num = 3000;
            if (arg0 == "-t")
            {
                if (int.TryParse(arg1, out num))
                {
                    num = Convert.ToInt32(args[1]);
                }

            }
            MonitorUser.MonitorUserMountDriver(num);
            Console.ReadLine();
        }
        
    }
}
