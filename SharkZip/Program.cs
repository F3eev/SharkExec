using System;
using System.Reflection;



namespace SharkZip
{
    class Program
    {
        public static void showMessage(string msg, string flag = "[+]")
        {
            Console.WriteLine(flag + ": " + msg);
        }
        static void Usage()
        {
            string banner = @"
    Usage:
        .\SharkZip.exe action type args  
    Arguments:
        action     u|z
        type       dir|file  
       
    eg: SharkZip.exe u  d:\\1.zip d:\\file
        SharkZip.exe z dir d:\\files\ d:\\1.zip
        SharkZip.exe z file d:\\1.txt d:\1.zip
    ";
            Console.WriteLine(banner);
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs e)
        {
            //项目的命名空间为SharkZip, 嵌入dll资源在Resources文件夹下，所以这里用的命名空间为： SharkZip.Resources.

            string _resName = "SharkZip.Resources." + new AssemblyName(e.Name).Name + ".dll";
            using (var _stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(_resName))
            {
                byte[] _data = new byte[_stream.Length];
                _stream.Read(_data, 0, _data.Length);
                return Assembly.Load(_data);
            }
        }


        static void Main(string[] args)
        {
         

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            if (args.Length < 3)
            {
                //ZipHelper.UnZip(@"C:\Users\Fucku\Desktop\bx1\aspx.zip", @"C:\Users\Fucku\Desktop\bx1\111\");
                Usage();
                return;
            }
            string c = args[0];

            if (c == "u" && args.Length == 3) { 

                string files = args[1];
                string Dfiles = args[2];
                ZipHelper.UnZip(files, Dfiles);
                showMessage("Unzip to directory "+Dfiles, "[+]");
            }
            else if(c == "z")
            {
                string a = args[1];
               
                if (a == "file" && args.Length ==4)
                {
                    string files = args[2];
                    string Dfiles = args[3];
                    ZipHelper.ZipFile(files, Dfiles);
                    showMessage("Zip to directory "+Dfiles,"[+]");
                }
                else if (a == "dir" && args.Length == 4)
                {
                    string files = args[2];
                    string Dfiles = args[3];
                    ZipHelper.ZipDir(files, Dfiles);
                    showMessage("Zip to directory "+Dfiles, "[+]");
                }
                else
                {
                    Usage();
                    return;
                }
            }
            
        }
    }
}
