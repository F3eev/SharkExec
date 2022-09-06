using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;


namespace SharkMonitor
{
    class MonitorUser
    {

        public static void MonitorUserLogon(int time)
        {

            Console.WriteLine("[*] : Start Monitoring User Login Pid:{0}", Process.GetCurrentProcess().Id.ToString());
            Dictionary<string, string> oldLoignUser = GetQueryUser();
            while (true)
            {
                Console.WriteLine(time);

                Thread.Sleep(time);
                Dictionary<string, string> newLoignUser = GetQueryUser();

                foreach (var item in newLoignUser)
                {
                    if (oldLoignUser.TryGetValue(item.Key,out string status))
                    {
                        if(item.Value != status)
                        {
                            Console.WriteLine("[+] : {0} User Login Status " + item.Key + "  -> "+ item.Value, DateTime.Now.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine("[+] : {0} New User " + item.Key + " Login ", DateTime.Now.ToString());
                    }
                }
                oldLoignUser = newLoignUser;
            }
        }

        public static void MonitorUserMountDriver(int time)
        {
            Console.WriteLine("[*] : Start Monitoring User {0} Login Pid:{1}", System.Security.Principal.WindowsIdentity.GetCurrent().Name,Process.GetCurrentProcess().Id.ToString());
            while (true)
            {
                Thread.Sleep(time);
                string[] lines = RunCmd("net use").Split(new string[] { "\r\n" }, StringSplitOptions.None);
                if (lines.Length > 5)
                {
                    Console.WriteLine("[+] : {0} User {1} Mount Driver  ", System.Security.Principal.WindowsIdentity.GetCurrent().Name, DateTime.Now.ToString());
                }
            }

        }
    


        private static string RunCmd(string cmd)
        {
            //Console.WriteLine("请输入要执行的命令:");
            string strInput = cmd;
            Process p = new Process();
            //(1)设置要启动的应用程序
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "/c" + cmd;
            //(2)是否使用操作系统shell启动
            p.StartInfo.UseShellExecute = false;
            //(3)接受来自调用程序的输入信息
            p.StartInfo.RedirectStandardInput = true;
            //(4)输出信息
            p.StartInfo.RedirectStandardOutput = true;
            //(5)输出错误
            p.StartInfo.RedirectStandardError = true;
            //(6)不显示程序窗口
           
            p.StartInfo.CreateNoWindow = true;
            //(7)启动程序
            p.Start();
         
            string strOuput = p.StandardOutput.ReadToEnd();

            //(10)等待程序执行完退出进程
            p.WaitForExit();
            p.Close();
            return strOuput;
        }

        private static Dictionary<string,string> GetQueryUser()
        {
            Dictionary<string, string> LoignUser = new Dictionary<string, string>() { };
            string cmdout = RunCmd("query user");
            string[] lines = cmdout.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            string status = "";
            for (int i = 1; i < lines.Length-1; i++)
            {
                if (Regex.IsMatch(lines[i], @"\w"))
                {
                    string[] l = lines[i].Split(new string[] { "  " }, StringSplitOptions.None);
                    string strRegex = @"\d+\s+(?<status>\S+)\s+\S+\s+\S+\s+\S+";
                    Regex myRegex = new Regex(strRegex, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    foreach (Match myMatch in myRegex.Matches(lines[i]))
                    {
                       status = myMatch.Groups["status"].Value;
                    }
                    string username = l[0].Replace(">", "").Replace(" ", "");

                    LoignUser.Add(username, status);
                }
            }
            //Console.WriteLine(LoignUser.Count);
            return LoignUser;
        }
        
    }
}
