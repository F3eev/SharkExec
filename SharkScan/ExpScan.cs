using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using SharkScan.SMB;
using SharkScan.Exp.Ms17010;

namespace SharkScan
{
    class ExpScan
    {
        
        private  int _jobCount;
        private  int _timeout;

        private  List<IpObj> _expParams = new List<IpObj>();
        private  object _lockObj = new object();
        private  ManualResetEvent _doneEvent = new ManualResetEvent(false);
        private List<IpPortExpResObj> _IpPortExpResObjList = new List<IpPortExpResObj>() { };

        public List<string> ExpScanLog = new List<string>() { };

        public ExpScan(List<IpObj> expParams,int maxThreadCount)
        {
            _expParams = expParams;
            _jobCount = expParams.ToArray().Length;
            ThreadPool.SetMinThreads(150, 150);
            ThreadPool.SetMaxThreads(maxThreadCount, maxThreadCount);
        }

        private void ThreadPoolCheck(Object paramsInfo)
        {
            IpPortExpObj paramObj = (IpPortExpObj)paramsInfo;
            TestResult testresult = new TestResult();
            lock (_lockObj)
            {
                testresult = Tester.TestIP(paramObj.Ip.ToString(), _timeout);
            }
            string res;
            if (testresult.IsVulnerable)
            {
                res = "Is  Vulnerable";
                ShowInfo(paramObj.Ip.ToString() + " ms17010 " + res);
            }
            else if (testresult.Active && !testresult.IsVulnerable)
            {
                res = "Is  Not Vulnerable";
                ShowInfo(paramObj.Ip.ToString() + " ms17010 " + res, "[-]");
            }
            else
            {
                res = "Is  Not Alive";
                //ShowInfo(paramObj.Ip.ToString() + " ms17010 " + res, "[-]",false);
                
            }
            /*lock (_lockObj)
            {
                _IpPortExpResObjList.Add(new IpPortExpResObj(paramObj.Ip.ToString(), 445, "ms17010", res));
            }*/
            if (Interlocked.Decrement(ref _jobCount) == 0)
            {
                _doneEvent.Set();
            }
            return;
        }
        public List<IpPortExpResObj> Check(string exp,int timeout=1000)
        {
            ShowInfo("Start Scanning MS17010 ", "[*]");
            _timeout = timeout;
            foreach (IpObj p in _expParams)
            {
                IpPortExpObj IpPortExpObj = new IpPortExpObj(p.Ip.ToString(), 445, "ms17017");
                ThreadPool.QueueUserWorkItem(ThreadPoolCheck, IpPortExpObj);
            }

            Debug.Write(3, "ExpScan Main Over");
            _doneEvent.WaitOne();
            ShowInfo("Scan MS17010 End", "[*]");
            return _IpPortExpResObjList;
        }

        private void ShowInfo(string message, string flag = "[+]", bool isShow = true)
        {
            string str = Utils.Message(message, flag);
            lock (_lockObj)
            {
                ExpScanLog.Add(str);
            }
            if (isShow)
            {
                Console.WriteLine(str);
            }
        }
    }
}
