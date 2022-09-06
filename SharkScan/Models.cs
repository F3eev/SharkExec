using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace SharkScan
{
    public class IpObj
    {
        public IPAddress Ip;
        public IpObj(string ip)
        {
            Ip = IPAddress.Parse(ip);
        }
    }
    class IpPortObj : IpObj
    {
        public int Port;
        public IpPortObj(string ip, int port) :
            base(ip)
        {
            Port = port;
        }
    }

    class IpPortExpObj : IpPortObj
    {
        public string Exp;
        public IpPortExpObj(string ip, int port, string exp) : base(ip, port)
        {
            Exp = exp;
        }
    }

    class IpPortExpResObj : IpPortObj
    {
        public string Exp;
        public string Res;
        public string Message;
        public IpPortExpResObj(string ip, int port, string exp, string res,string message="") : base(ip, port)
        {
            Exp = exp;
            Res = res;
            Message = message;
        }
       
    }

    //端口扫描结果
    class IpPortDataObj : IpPortObj
    {
        public string Data;

        public IpPortDataObj(string ip, int port, string data ) : base(ip, port)
        {
            Data = data;
        }
    }
}
