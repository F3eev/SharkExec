using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using SharkScan.NetShare;
namespace SharkScan
{
    class NetShareScan
    {
        private int _jobCount;
        private int _timeout;

        private List<IpObj> _netShareParams = new List<IpObj>();
        private object _lockObj = new object();
        private ManualResetEvent _doneEvent = new ManualResetEvent(false);
        private List<string> _netShareScanResList = new List<string>() { };

        public List<string> NetShareScanLog = new List<string>() { };
        public List<string> NetShareHost = new List<string>();



        public NetShareScan(List<IpObj> netShareParams, int maxThreadCount)
        {
            _netShareParams = netShareParams;
            _jobCount = netShareParams.ToArray().Length;
            if (_jobCount == 0)
            {
                _doneEvent.Set();
            }
            ThreadPool.SetMinThreads(150, 150);
            ThreadPool.SetMaxThreads(maxThreadCount, maxThreadCount);
        }
        public List<string> Scan(int timeout = 1500)
        {
            ShowInfo("Start Scanning NetShare", "[*]");
            _timeout = timeout;
            foreach (IpObj p in _netShareParams)
            {
                ThreadPool.QueueUserWorkItem(ThreadPoolNetShare, p);
            }
            Debug.Write(3, "PingScan Main Over");
            _doneEvent.WaitOne();
            ShowInfo("Scan NetShare End", "[*]");
            return NetShareHost;

        }
        private void ThreadPoolNetShare(Object paramsInfo)
        {
            IpObj paramObj = (IpObj)paramsInfo;
            //Console.WriteLine(paramObj.Ip);
            List<string> shareUNC = NetShare.NetShare.GetshareUNC(paramObj.Ip.ToString());
            foreach (String netName in shareUNC)
            {
                string line = @"\\" + paramObj.Ip.ToString() + @"\" + netName;
                string result = identifynetshare(line);
                ShowInfo(line+" "+ result);
                lock (_lockObj)
                {
                    NetShareHost.Add(paramObj.Ip.ToString());
                }
            }
            if (Interlocked.Decrement(ref _jobCount) == 0)
            {
                _doneEvent.Set();
            }
            return;
        }

        private void ShowInfo(string message, string flag = "[+]", bool isShow = true)
        {
            string str = Utils.Message(message, flag);

            lock (_lockObj)
            {
                NetShareScanLog.Add(str);
            }
            if (isShow)
            {
               Console.WriteLine(str);
            }
        }
        public static string identifynetshare(string smburl)
        {
            string cmd = @"net use " + smburl;
            string result = "";
            string cmd1out = excution(cmd);

            if (cmd1out.IndexOf("The password is invalid for") >= 0)
            {
                //Console.WriteLine(smburl + " require password");
                result = "requirePassword";
            }
            else if (cmd1out.IndexOf("The command completed") >= 0)
            {
                string cmd2out = excution("dir " + smburl);


                if (cmd2out.IndexOf("Directory of") >= 0)
                {
                    //Console.WriteLine(smburl + " successful");
                    result = "Available";
                }
                else
                {
                    //Console.WriteLine(cmd2out);
                    result = "UnAvailable";
                }
                excution(cmd + " /del");

            }
            else
            {
                result = "Timeout";
                //Console.WriteLine(cmd1out);
            }
            return result;
        }
        public static string excution(string cmd)
        {
            try
            {
                //创建一个进程
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;
                //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序

                //向cmd窗口发送输入信息

                p.StandardInput.WriteLine(cmd + "& exit");
                p.StandardInput.AutoFlush = true;
                string output = p.StandardOutput.ReadToEnd();
                while (!p.HasExited)
                {
                    p.WaitForExit(20);
                    p.Kill();
                }
                p.StandardInput.AutoFlush = true;

                //获取cmd窗口的输出信息

                //等待程序执行完退出进程

                p.Close();
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
                return "";
            }
        }
    }
}
