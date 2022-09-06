using System;
using System.Collections.Generic;
using System.Threading;
namespace SharkScan
{
    class PingScan
    {
        private static  int _jobCount;
        private static int _timeout;

        private static ManualResetEvent _doneEvent = new ManualResetEvent(false);
        private static object _lockObj = new object();
        private static List<IpObj> _pingParams = new List<IpObj>();

        public List<string> PingScanLog = new List<string>();
        public List<string> OnlineHost = new List<string>();

        public PingScan(List<IpObj> pingParams, int maxThreadCount)
        {
            _pingParams = pingParams;
            _jobCount = pingParams.ToArray().Length;
            ThreadPool.SetMinThreads(150, 150);
            ThreadPool.SetMaxThreads(maxThreadCount, maxThreadCount);
        }

        public List<string> Ping(int timeout = 1500)
        {
            ShowInfo("Start Scanning Alive", "[*]");
            _timeout = timeout;
            foreach (IpObj p in _pingParams)
            {
                ThreadPool.QueueUserWorkItem(ThreadPoolOnlyPing, p);
            }
            Debug.Write(3, "PingScan Main Over");
            _doneEvent.WaitOne();
            ShowInfo("Scan Alive End", "[*]");
            return OnlineHost;

        }
        private void ThreadPoolOnlyPing(Object paramsInfo)
        {
            IpObj paramObj = (IpObj)paramsInfo;
            Debug.Write(1, paramObj.Ip.ToString());
            if (Utils.onlin_ping(paramObj.Ip.ToString(), _timeout))
            {
                ShowInfo(paramObj.Ip.ToString() + " Alive");
                lock (_lockObj)
                {
                    OnlineHost.Add(paramObj.Ip.ToString());
                }
            }
            Debug.Write(1, paramObj.Ip.ToString()+" 0");
            if (Interlocked.Decrement(ref _jobCount) == 0)
            {
                _doneEvent.Set();
            }
            return;
        }

        private void ShowInfo(string message, string flag = "[+]",bool isShow=true)
        {
            string str = Utils.Message(message, flag);

            lock (_lockObj)
            {
                PingScanLog.Add(str);
            }
            if (isShow)
            {
                Console.WriteLine(str);
            }
        }
    }
}
