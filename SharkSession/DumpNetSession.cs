using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
namespace SharkSession
{

    class sessionDataObj
    {
        public sessionDataObj(string target, string cname, string clinet)
        {
            Target = target;
            Cname = cname;
            Client = clinet;
        }
        public string Cname;
        public string Client;
        public string Target;
    }



    class DumpNetSession
    {

        
        private  int _jobCount;
        private StreamWriter _streamw;
        private FileStream _fstream;
        private bool _showAll;
        private bool _wlog = false;
        private object _lockObj = new object();
        private AutoResetEvent _doneEvent = new AutoResetEvent(false);
        private string[] _userArray= { };

        public List<string> DumpSessionLog = new List<string>() { };


        public  void Run(List<string> HostNames, string[] userlist, int ThreadNum,string logfilepath = "", bool showall=false)
        {
           
            _userArray = userlist;
            _showAll = showall;

            if (logfilepath!="")
            {
                _wlog = true;
                try
                {
                    if (File.Exists(logfilepath))
                    {
                        _fstream = new FileStream(logfilepath, FileMode.Append, FileAccess.Write);
                    }
                    else
                    {
                        _fstream = new FileStream(logfilepath, FileMode.Create, FileAccess.Write);
                    }
                    _streamw = new StreamWriter(_fstream, Encoding.Default);
                }
                catch (System.UnauthorizedAccessException)
                {
                    Console.WriteLine("[-] : " + logfilepath + " AccessException");
                    return;
                }
                catch (System.IO.IOException)
                {
                    Console.WriteLine("[-] : "+ logfilepath + " File Error ");
                }
            }
            _jobCount = HostNames.ToArray().Length;
            ThreadPool.SetMinThreads(0, 0);
            ThreadPool.SetMaxThreads(ThreadNum, ThreadNum);
            foreach(string hostname in HostNames)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadMethod), new object[] { hostname });
            }
            _doneEvent.WaitOne();
            if (_wlog)
            {
                _streamw.Close();
                _fstream.Close();
            }
           

        }

        // https://docs.microsoft.com/en-us/windows/desktop/api/lmshare/nf-lmshare-netsessionenum
        // https://msdn.microsoft.com/en-us/library/cc247273.aspx
        // https://pinvoke.net/default.aspx/netapi32/NetSessionEnum.html

        public  void ThreadMethod(object obj)
        {
            var hostname = ((object[])obj)[0];
            string message;

            //Console.WriteLine(string.Format("{0}:第{1}个线程", DateTime.Now.ToString(), hostname));
            List<sessionDataObj> SessionData = GetSession(hostname.ToString());
            foreach (sessionDataObj sessionData in SessionData)
            {
                int id = Array.IndexOf(_userArray, sessionData.Client);
                if (id != -1)
                {
                    message = string.Format("[+] : {0} {1} > cname {2} clinet {3}", DateTime.Now.ToString(), sessionData.Target, sessionData.Cname, sessionData.Client);
                    Console.WriteLine(message);
                }
                else
                {
                    message = string.Format("[*] : {0} {1} > cname {2} clinet {3}", DateTime.Now.ToString(), sessionData.Target, sessionData.Cname, sessionData.Client);
                   // Console.WriteLine(message);
                    if (_showAll)
                    {
                        Console.WriteLine(message);
                    }
                }

                lock (_lockObj)
                {
                    if (_wlog)
                    {
                        _streamw.WriteLine(message);
                        _streamw.Flush();
                    }
                    DumpSessionLog.Add(message);
                }
            }

            if (Interlocked.Decrement(ref _jobCount) == 0)
            {
                _doneEvent.Set();
            }

        }

      

           public  List<sessionDataObj> GetSession(string serverName)
        {
            var entriesRead = 0;
            var totalEntries = 0;
            var level = 10;
            var ptrInfo = IntPtr.Zero;
            var resumeHandle = IntPtr.Zero;
            


            var result = NetSessionEnum(serverName, null, null, level, out ptrInfo, MAX_PREFERRED_LENGTH, out entriesRead, out totalEntries, ref resumeHandle);

            if (result != NET_API_STATUS.NERR_Success)
            {
                Console.WriteLine("ERROR(0x{0:X}, {0}): {1}", (int)result, result);
            }

            var entries = new SESSION_INFO_10[entriesRead];

            var iter = ptrInfo;

            for (var i = 0; i < entriesRead; i++)
            {
                entries[i] = (SESSION_INFO_10)Marshal.PtrToStructure(iter, typeof(SESSION_INFO_10));
                iter = (IntPtr)(iter.ToInt64() + Marshal.SizeOf(typeof(SESSION_INFO_10)));
            }

            NetApiBufferFree(ptrInfo);
            
            List<sessionDataObj> SessionDataObjList = new List<sessionDataObj>();
            foreach (var entry in entries)
            {
                var cname = entry.sesi10_cname;
                var username = entry.sesi10_username;
                var time = entry.sesi10_time;
                var idle_time = entry.sesi10_time;

                SessionDataObjList.Add(new sessionDataObj(serverName, cname, username));
            }
            return SessionDataObjList;
        }

        [DllImport("NetAPI32.dll")]
        private static extern NET_API_STATUS NetSessionEnum(
            [MarshalAs(UnmanagedType.LPWStr)] string ServerName,
            [MarshalAs(UnmanagedType.LPWStr)] string UncClientName,
            [MarshalAs(UnmanagedType.LPWStr)] string UserName,
            int Level,
            out IntPtr bufptr,
            int prefmaxlen,
            out int entriesread,
            out int totalentries,
            ref IntPtr resume_handle);

        [DllImport("NetAPI32.dll")]
        private static extern int NetApiBufferFree(
            IntPtr Buff);

        [StructLayout(LayoutKind.Sequential)]
        public struct SESSION_INFO_10
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string sesi10_cname;
            [MarshalAs(UnmanagedType.LPWStr)] public string sesi10_username;
            public uint sesi10_time;
            public uint sesi10_idle_time;
        }

        public const int MAX_PREFERRED_LENGTH = -1;

        public enum NET_API_STATUS
        {
            NERR_Success = 0x0,
            ERROR_ACCESS_DENIED = 0x5,
            ERROR_NOT_ENOUGH_MEMORY = 0x8,
            ERROR_BAD_NETPATH = 0x35,
            ERROR_NETWORK_BUSY = 0x36,
            ERROR_INVALID_PARAMETER = 0x57,
            ERROR_INVALID_LEVEL = 0x7C,
            ERROR_MORE_DATA = 0xEA,
            NERR_UserNotFound = 0x8AD,
            NERR_ClientNameNotFound = 0x908,
            NERR_InvalidComputer = 0x92F
        }
    }

}
