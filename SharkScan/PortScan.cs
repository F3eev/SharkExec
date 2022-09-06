using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using SharkScan.SMB;
using System.Net;

namespace SharkScan
{
    class PortScan
    {

        private static int _jobCount;

        private static int _timeout;
        private static bool _isping;
        private static bool _getServer;

        private static object _lockObj = new object();
        private static ManualResetEvent _doneEvent = new ManualResetEvent(false);
        private static List<IpPortObj> _portParams = new List<IpPortObj>();
        private static Dictionary<string, bool> _HostStatusCache = new Dictionary<string, bool>() { };

        public List<string> PortScanLog = new List<string>() { };
        // public List<IpPortObj> OpenPortObjList = new List<IpPortObj>() { };

        //public List<IpPortObj> ClosePortObjList = new List<IpPortObj>();
        //public List<string> OnlineHost = new List<string>();

        public List<IpPortDataObj> OpenPortDataObjList = new List<IpPortDataObj>() { };

        private static List<string> PROBES = new List<string> {
            "\r\n\r\n",
            "GET / HTTP/1.0\r\n\r\n",
            "GET / \r\n\r\n",
            "\x01\x00\x00\x00\x01\x00\x00\x00\x08\x08",
            "\x80\0\0\x28\x72\xFE\x1D\x13\0\0\0\0\0\0\0\x02\0\x01\x86\xA0\0\x01\x97\x7C\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0",
            "\x03\0\0\x0b\x06\xe0\0\0\0\0\0",
            "\0\0\0\xa4\xff\x53\x4d\x42\x72\0\0\0\0\x08\x01\x40\0\0\0\0\0\0\0\0\0\0\0\0\0\0\x40\x06\0\0\x01\0\0\x81\0\x02PC NETWORK PROGRAM 1.0\0\x02MICROSOFT NETWORKS 1.03\0\x02MICROSOFT NETWORKS 3.0\0\x02LANMAN1.0\0\x02LM1.2X002\0\x02Samba\0\x02NT LANMAN 1.0\0\x02NT LM 0.12\0",
            "\x80\x9e\x01\x03\x01\x00u\x00\x00\x00 \x00\x00f\x00\x00e\x00\x00d\x00\x00c\x00\x00b\x00\x00:\x00\x009\x00\x008\x00\x005\x00\x004\x00\x003\x00\x002\x00\x00/\x00\x00\x1b\x00\x00\x1a\x00\x00\x19\x00\x00\x18\x00\x00\x17\x00\x00\x16\x00\x00\x15\x00\x00\x14\x00\x00\x13\x00\x00\x12\x00\x00\x11\x00\x00\n\x00\x00\t\x00\x00\x08\x00\x00\x06\x00\x00\x05\x00\x00\x04\x00\x00\x03\x07\x00\xc0\x06\x00@\x04\x00\x80\x03\x00\x80\x02\x00\x80\x01\x00\x80\x00\x00\x02\x00\x00\x01\xe4i<+\xf6\xd6\x9b\xbb\xd3\x81\x9f\xbf\x15\xc1@\xa5o\x14,M \xc4\xc7\xe0\xb6\xb0\xb2\x1f\xf9)\xe8\x98",
            "\x16\x03\0\0S\x01\0\0O\x03\0?G\xd7\xf7\xba,\xee\xea\xb2`~\xf3\0\xfd\x82{\xb9\xd5\x96\xc8w\x9b\xe6\xc4\xdb<=\xdbo\xef\x10n\0\0(\0\x16\0\x13\0\x0a\0f\0\x05\0\x04\0e\0d\0c\0b\0a\0`\0\x15\0\x12\0\x09\0\x14\0\x11\0\x08\0\x06\0\x03\x01\0",
            "< NTP/1.2 >\n",
            "< NTP/1.1 >\n",
            "< NTP/1.0 >\n",
            "\0Z\0\0\x01\0\0\0\x016\x01,\0\0\x08\0\x7F\xFF\x7F\x08\0\0\0\x01\0 \0:\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\04\xE6\0\0\0\x01\0\0\0\0\0\0\0\0(CONNECT_DATA=(COMMAND=version))",
            "\x12\x01\x00\x34\x00\x00\x00\x00\x00\x00\x15\x00\x06\x01\x00\x1b\x00\x01\x02\x00\x1c\x00\x0c\x03\x00\x28\x00\x04\xff\x08\x00\x01\x55\x00\x00\x00\x4d\x53\x53\x51\x4c\x53\x65\x72\x76\x65\x72\x00\x48\x0f\x00\x00",
            "\0\0\0\0\x44\x42\x32\x44\x41\x53\x20\x20\x20\x20\x20\x20\x01\x04\0\0\0\x10\x39\x7a\0\x01\0\0\0\0\0\0\0\0\0\0\x01\x0c\0\0\0\0\0\0\x0c\0\0\0\x0c\0\0\0\x04",
            "\x01\xc2\0\0\0\x04\0\0\xb6\x01\0\0\x53\x51\x4c\x44\x42\x32\x52\x41\0\x01\0\0\x04\x01\x01\0\x05\0\x1d\0\x88\0\0\0\x01\0\0\x80\0\0\0\x01\x09\0\0\0\x01\0\0\x40\0\0\0\x01\x09\0\0\0\x01\0\0\x40\0\0\0\x01\x08\0\0\0\x04\0\0\x40\0\0\0\x01\x04\0\0\0\x01\0\0\x40\0\0\0\x40\x04\0\0\0\x04\0\0\x40\0\0\0\x01\x04\0\0\0\x04\0\0\x40\0\0\0\x01\x04\0\0\0\x04\0\0\x40\0\0\0\x01\x04\0\0\0\x02\0\0\x40\0\0\0\x01\x04\0\0\0\x04\0\0\x40\0\0\0\x01\0\0\0\0\x01\0\0\x40\0\0\0\0\x04\0\0\0\x04\0\0\x80\0\0\0\x01\x04\0\0\0\x04\0\0\x80\0\0\0\x01\x04\0\0\0\x03\0\0\x80\0\0\0\x01\x04\0\0\0\x04\0\0\x80\0\0\0\x01\x08\0\0\0\x01\0\0\x40\0\0\0\x01\x04\0\0\0\x04\0\0\x40\0\0\0\x01\x10\0\0\0\x01\0\0\x80\0\0\0\x01\x10\0\0\0\x01\0\0\x80\0\0\0\x01\x04\0\0\0\x04\0\0\x40\0\0\0\x01\x09\0\0\0\x01\0\0\x40\0\0\0\x01\x09\0\0\0\x01\0\0\x80\0\0\0\x01\x04\0\0\0\x03\0\0\x80\0\0\0\x01\0\0\0\0\0\0\0\0\0\0\0\0\x01\x04\0\0\x01\0\0\x80\0\0\0\x01\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\x01\0\0\x40\0\0\0\x01\0\0\0\0\x01\0\0\x40\0\0\0\0\x20\x20\x20\x20\x20\x20\x20\x20\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\x01\0\xff\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\xe4\x04\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\0\x7f",
            "\x41\0\0\0\x3a\x30\0\0\xff\xff\xff\xff\xd4\x07\0\0\0\0\0\0test.$cmd\0\0\0\0\0\xff\xff\xff\xff\x1b\0\0\0\x01serverStatus\0\0\0\0\0\0\0\xf0\x3f\0"
        };
        private static List<string> SignsList = new List<string> {
            "http|^HTTP.*",
                   "http|^HTTP/0.",
                   "http|^HTTP/1.",
                   "http|<HEAD>.*<BODY>",
                   "http|<HTML>.*",
                   "http|<html>.*",
                   "http|<!DOCTYPE.*",
                   "http|^Invalid requested URL",
                   "http|.*<?xml",
                   "http|^HTTP/.*\nServer: Apache/1",
                   "http|^HTTP/.*\nServer: Apache/2",
                   "http|.*Microsoft-IIS.*",
                   "http|.*<title>.*",
                   "http|^HTTP/.*\nServer: Microsoft-IIS",
                   "http|^HTTP/.*Cookie.*ASPSESSIONID",
                   "http|^<h1>Bad Request .Invalid URL.</h1>",
                   "http-jserv|^HTTP/.*Cookie.*JServSessionId",
                   "http-weblogic|^HTTP/.*Cookie.*WebLogicSession",
                   "http-vnc|^HTTP/.*VNC desktop",
                   "http-vnc|^HTTP/.*RealVNC/",
                   "redis|^-ERR",
                   @"mongodb|^.*version.....([\.\d]+)",
                   "pop3|.*POP3.*",
                   "pop3|.*pop3.*",
                   "ssh|SSH-2.0-OpenSSH.*",
                   "ssh|SSH-1.0-OpenSSH.*",
                   "ssh|.*ssh.*",
                   "netbios|^\\x79\\x08.*BROWSE",
                   "netbios|^\\x79\\x08.\\x00\\x00\\x00\\x00",
                   "netbios|^\\x05\\x00\\x0d\\x03",
                   "netbios|^\\x83\\x00",
                   "netbios|^\\x82\\x00\\x00\\x00",
                   "netbios|\\x83\\x00\\x00\\x01\\x8f",
                   "backdoor-fxsvc|^500 Not Loged in",
                   "backdoor-shell|GET: command",
                   "backdoor-shell|sh: GET:",
                   "bachdoor-shell|[a-z]*sh: .* command not found",
                   "backdoor-shell|^bash[$#]",
                   "backdoor-shell|^sh[$#]",
                   "backdoor-cmdshell|^Microsoft Windows .* Copyright .*>",
                   "dell-openmanage|^\\x4e\\x00\\x0d",
                   "finger|^\r\n	Line	  User",
                   "finger|Line	 User",
                   "finger|Login name:",
                   "finger|Login.*Name.*TTY.*Idle",
                   "finger|^No one logged on",
                   "finger|^\r\nWelcome",
                   "finger|^finger:",
                   "finger|^must provide username",
                   "finger|finger: GET:",
                   "ftp|^220.*\n331",
                   "ftp|^220.*\n530",
                   "ftp|^220.*FTP",
                   "ftp|^220 .* Microsoft .* FTP",
                   "ftp|^220 Inactivity timer",
                   "ftp|^220 .* UserGate",
                   "ftp|^220(.*?)",
                   "ldap|^\\x30\\x0c\\x02\\x01\\x01\\x61",
                   "ldap|^\\x30\\x32\\x02\\x01",
                   "ldap|^\\x30\\x33\\x02\\x01",
                   "ldap|^\\x30\\x38\\x02\\x01",
                   "ldap|^\\x30\\x84",
                   "ldap|^\\x30\\x45",
                   "ldap|^\\x30.*",
                   "smb|^\\x00\\x00\\x00.*\\xffSMBr",
                   "msrdp|^\\x03\\x00\\x00\\x0b",
                   "msrdp|^\\x03\\x00\\x00\\x11",
                   "msrdp|^\\x030x000x00\\x0b\\x06\\xd00x000x00\\x12.*0x00$",
                   "msrdp|^\\x030x000x00\\x17\\x08\\x020x000x00Z~0x00\\x0b\\x05\\x05@\\x060x00\\x08\\x91J0x00\\x02X$",
                   "msrdp|^\\x030x000x00\\x11\\x08\\x02..*}\\x08\\x030x000x00\\xdf\\x14\\x01\\x01$",
                   "msrdp|^\\x030x000x00\\x0b\\x06\\xd00x000x00\\x03.*0x00$",
                   "msrdp|^\\x030x000x00\\x0b\\x06\\xd00x000x000x000x000x00",
                   "msrdp|^\\x030x000x00\\x0e\t\\xd00x000x000x00[\\x02\\xa1]0x00\\xc0\\x01\n$",
                   "msrdp|^\\x030x000x00\\x0b\\x06\\xd00x00\\x004\\x120x00",
                   "msrdp-proxy|^nmproxy: Procotol byte is not 8\n$",
                   "msrpc|^\\x05\\x00\\x0d\\x03\\x10\\x00\\x00\\x00\\x18\\x00\\x00\\x00\\x00\\x00",
                   "msrpc|\\x050x00\r\\x03\\x100x000x000x00\\x180x000x000x00....\\x040x00\\x01\\x050x000x000x000x00$",
                   "mssql|^\\x04\\x010x00C.*0x000x00\\xaa0x000x000x00/\\x0f\\xa2\\x01\\x0e.*",
                   "mssql|^\\x05\\x6e\\x00",
                   "mssql|^\\x04\\x01\\x00\\x25\\x00\\x00\\x01\\x00\\x00\\x00\\x15.*",
                   "mssql|^\\x04\\x01\\x00.*\\x00\\x00\\x01\\x00\\x00\\x00\\x15.*",
                   "mssql|^\\x04\\x01\\x00\\x25\\x00\\x00\\x01\\x00\\x00\\x00\\x15.*",
                   "mssql|^\\x04\\x01\\x00.*\\x00\\x00\\x01\\x00\\x00\\x00\\x15.*",
                   "mssql|^\\x04\\x010x00\\x250x000x00\\x010x000x000x00\\x150x00\\x06\\x01.*",
                   "mssql|^\\x04\\x01\\x00\\x25\\x00\\x00\\x01.*",
                   "mysql|^\\x19\\x00\\x00\\x00\\x0a",
                   "mysql|^\\x2c\\x00\\x00\\x00\\x0a",
                   "mysql|hhost \"",
                   "mysql|khost \"",
                   "mysql|mysqladmin",
                   "mysql|(.*)5(.*)log",
                   "mysql|(.*)4(.*)log",
                   "mysql|whost \"",
                   @"mysql|^\(\\x00\\x00",
                   "mysql|this MySQL",
                   "mysql|^N\\x00",
                   "mysql|(.*)mysql(.*)",
                   @"nagiosd|Sorry, you \(.*are not among the allowed hosts...",
                   "nessus|< NTP 1.2 >\\x0aUser:",
                   @"oracle|\(ERROR_STACK=\(ERROR=\(CODE=",
                   @"oracle|\(ADDRESS=\(PROTOCOL=",
                   "oracle-dbsnmp|^\\x00\\x0c\\x00\\x00\\x04\\x00\\x00\\x00\\x00",
                   "oracle-https|^220- ora",
                   "oracle-rmi|\\x00\\x00\\x00\\x76\\x49\\x6e\\x76\\x61",
                   "oracle-rmi|^\\x4e\\x00\\x09",
                   "postgres|Invalid packet length",
                   "postgres|^EFATAL",
                   "rlogin|login:",
                   "rlogin|rlogind:",
                   "rlogin|^\\x01\\x50\\x65\\x72\\x6d\\x69\\x73\\x73\\x69\\x6f\\x6e\\x20\\x64\\x65\\x6e\\x69\\x65\\x64\\x2e\\x0a",
                   "rpc-nfs|^\\x02\\x00\\x00\\x00\\x00\\x00\\x00\\x01\\x00\\x00\\x00\\x01\\x00\\x00\\x00\\x00",
                   "rpc|\\x01\\x86\\xa0",
                   "rpc|\\x03\\x9b\\x65\\x42\\x00\\x00\\x00\\x01",
                   "rpc|^\\x80\\x00\\x00",
                   "rsync|^@RSYNCD:.*",
                   "smux|^\\x41\\x01\\x02\\x00",
                   "snmp|\\x70\\x75\\x62\\x6c\\x69\\x63\\xa2",
                   "snmp|\\x41\\x01\\x02",
                   "socks|^\\x05[\\x00-\\x08]\\x00",
                   "ssh|^SSH-",
                   "ssh|^SSH-.*openssh",
                   "sybase|^\\x04\\x01\\x00",
                   "telnet|^\\xff\\xfd",
                   "telnet-disabled|Telnet is disabled now",
                   "telnet|^\\xff\\xfe",
                   "telnet|^xff\\xfb\\x01\\xff\\xfb\\x03\\xff\\xfb0x00\\xff\\xfd.*",
                   "tftp|^\\x00[\\x03\\x05]\\x00",
                   "uucp|^login: password:",
                   "vnc|^RFB.*",
                   "webmin|.*MiniServ",
                   "RMI|^N.*",
                   @"webmin|^0\.0\.0\.0:.*:[0-9]",
                   "websphere-javaw|^\\x15\\x00\\x00\\x00\\x02\\x02\\x0a",
                   "db2|.*SQLDB2RA"
        };

        public PortScan(List<IpPortObj> portParams, int maxThreadCount = 200)
        {
            _portParams = portParams;

            ThreadPool.SetMinThreads(150, 150);
            ThreadPool.SetMaxThreads(maxThreadCount, maxThreadCount);

        }
        public static String TcpSend(String ip, int port, String data,int timout)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Parse(ip), port);
            String Response = "";
            tcpClient.Client.ReceiveTimeout = timout;
            if (tcpClient.GetStream().CanWrite)
            {
                try
                {
                    byte[] request = Encoding.UTF8.GetBytes(data.ToString());
                    tcpClient.Client.Send(request);
                    byte[] responseByte = new byte[1024000];
                    int len = tcpClient.Client.Receive(responseByte);
                    Response = Encoding.UTF8.GetString(responseByte, 0, len);
                }
                catch (System.Net.Sockets.SocketException e)
                {
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            tcpClient.Close();
            return Response;
        }
        private void ThreadPoolPortScan(Object paramsInfo)
        {
            IpPortObj paramObj = (IpPortObj)paramsInfo;
            bool HosTstatus = false;
            string data = "";
            String ServerName = "";
            bool isSend = false;
            if (_isping)
            {
                if (_HostStatusCache.TryGetValue(paramObj.Ip.ToString(), out bool Hs))
                {
                    HosTstatus = Hs;
                }
                else
                {
                    HosTstatus = Utils.onlin_ping(paramObj.Ip.ToString());
                    try
                    {
                        _HostStatusCache.Add(paramObj.Ip.ToString(), HosTstatus);
                    }
                    catch (System.ArgumentException)
                    {

                    }
                }

                if (!HosTstatus)
                {
                    Debug.Write(3, "ping  host  error " + paramObj.Ip);
                }
            }
            if (((_isping == true) && (HosTstatus == true)) || (_isping == false))
            {
                try
                {
                    /*IPEndPoint ipport = new IPEndPoint(paramObj.Ip, (Int32)paramObj.Port);
                    Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    s.Connect(ipport);
*/
                    TcpClient client = new TcpClient();
                    IAsyncResult oAsyncResult = client.BeginConnect(paramObj.Ip, (Int32)paramObj.Port, null, null);
                    oAsyncResult.AsyncWaitHandle.WaitOne(_timeout, true);
                    client.ReceiveTimeout = _timeout;
                    client.SendTimeout = _timeout;

                    if (client.Connected)
                    {
                        Debug.Write(3, "ip : " + paramObj.Ip + "port:" + paramObj.Port + " open");
                        if (_getServer)
                        {
                            
                            if (paramObj.Port == 445)
                            {
                                lock (_lockObj)
                                {
                                    SmbDataObj SmbData = Smb.Run(paramObj.Ip.ToString(), 445, _timeout);
                                    data = SmbData._ComputerName + " " + SmbData._DNSTreeName + " " + SmbData._ComputerOs + " " + String.Join(", ", SmbData._ComputerNets.ToArray());
                                }
                            }
                            else
                            {
                                try
                                {
                                    byte[] responseByte = new byte[1024000];
                                    client.Client.ReceiveTimeout = _timeout;
                                    client.Client.SendTimeout = _timeout;
                                    int len = client.Client.Receive(responseByte);
                                    String Response = Encoding.UTF8.GetString(responseByte, 0, len);   

                                    if (Response.Length > 0)
                                    {
                                        Debug.Write(3, "tcpclient revice data");
                                        ServerName = GetServer(Response);
                                    }
                                }

                                catch (System.IO.IOException)
                                {
                                    isSend = true;
                                    Debug.Write(3, " tcpclient GetStream IOException " + paramObj.Ip + " port: " + paramObj.Port);
                                }
                                catch (System.Net.Sockets.SocketException e)
                                {
                                    isSend = true;
                                    Debug.Write(3, " tcpclient GetStream SocketException " + paramObj.Ip + " port: " + paramObj.Port);

                                }

                                if (isSend && ServerName == "")
                                {
                                    foreach (String sendData in PROBES)
                                    {

                                        String TcpRes = TcpSend(paramObj.Ip.ToString(), paramObj.Port, sendData,_timeout);
                                        ServerName = GetServer(TcpRes);
                                        if (ServerName != "")
                                        {
                                            break;
                                        }
                                    }
                                }
                                if(ServerName == "http")
                                {
                                    ServerName = Web.Web.GetWebData(paramObj.Ip.ToString(), paramObj.Port);
                                }
                             
                                data = ServerName;
                            }
                        }
                        ShowInfo(paramObj.Ip.ToString() + " " + paramObj.Port + " " + data);
                        lock (_lockObj)
                        {
                            OpenPortDataObjList.Add(new IpPortDataObj(paramObj.Ip.ToString(), paramObj.Port, data));
                        }
                        //OpenPortObjList.Add(paramObj);

                    }
                    /*else
                    {
                        //ShowInfo(paramObj.Ip.ToString() + " " + paramObj.Port + " " + data,"[-]",false);
                        //ClosePortObjList.Add(paramObj);
                        //Console.WriteLine(string.Format("ip :{0} port: {1}  close ", paramObj.Ip.ToString(), paramObj.Port.ToString()));
                    }*/
                    oAsyncResult.AsyncWaitHandle.Close();
                    client.Close();
                }

                catch (SocketException e)
                {
                    Debug.Write(3, "portscan e.Message");
                }

            }

            if (Interlocked.Decrement(ref _jobCount) == 0)
            {
                _doneEvent.Set();
            }

            return;
        }
        private static string GetServer(string data)
        {
            if (data.Length == 0) return "";
            string ServerName = "";
            foreach (string s in SignsList)
            {
                string[] pArray = s.Split(new char[1] { '|' });
                string server = pArray[0];
                string re = pArray[1];
                Regex regex = new Regex(@re);
                if (regex.IsMatch(data))
                {
                    ServerName = server;
                    break;
                }
            }
            return ServerName;
        }

        public List<IpPortDataObj> Scan(bool isping = true, bool getserver = true, int timeout = 2000)
        {
            ShowInfo("Start Scanning Port", "[*]");
            _isping = isping;
            _getServer = getserver;
            _jobCount = _portParams.ToArray().Length;
            _timeout = timeout;
            foreach (IpPortObj p in _portParams)
            {
                ThreadPool.QueueUserWorkItem(ThreadPoolPortScan, p);
            }
            Debug.Write(3, "PortScan Main Over");
            _doneEvent.WaitOne();
            ShowInfo("Scan Port End", "[*]");
            return OpenPortDataObjList;
        }

        private void ShowInfo(string message, string flag = "[+]", bool isShow = true)
        {
            string str = Utils.Message(message, flag);

            lock (_lockObj)
            {
                PortScanLog.Add(str);
            }
            if (isShow)
            {
                Console.WriteLine(str);
            }
        }
    }
}
