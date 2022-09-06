using System;
using System.Collections.Generic;
using System.Text;

namespace SharkScan
{
    class Debug
    {

        public const int None = 0;
        public const int Error = 1;
        public const int Warning = 2;
        public const int Info = 3;
        public const int Buffer = 4;
        public const int Method = 5;

        public static int DebugLevel = 0;

        public const bool DebugOn = true;


        public static void Write(int level, string data)
        {
            if (DebugOn && DebugLevel >= level)
            {
                Console.WriteLine("debug: "+data);
            }
        }
    }
}
