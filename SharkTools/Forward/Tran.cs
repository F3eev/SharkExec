
using System;
using System.Net;
using System.Net.Sockets;


namespace Forward
{
    public class Tran
    {

        /// <summary>
        /// 
        /// </summary>
        public int inputConn_Port;

        /// <summary>
        /// dns  or ip
        /// </summary>
        public string outputConn_Host = "127.0.0.1";
        public int outputConn_Port;

        public static void start(int lport, string rehost, int report)
        {
            var tran = new Tran()
            {
                inputConn_Port = lport,
                outputConn_Host = rehost,
                outputConn_Port = report
            };


            //定义显示输出

            Console.WriteLine("[*]: " + DateTime.Now.ToString("[HH:mm:ss.fff]") + "开始监听转发:" + lport + "-->" + rehost + ":" + report);

            tran.StartLinstening();
        }

        IPAddress outputConn_IPAddress
        {
            get
            {
                return TcpHelp.ParseToIPAddress(outputConn_Host);
            }
        }


        public void StartLinstening()
        {       
            //input
            TcpHelp.Listening(inputConn_Port, OnInputConnected);         
        }


        private void OnInputConnected(TcpClient inputClient)
        {
            //Bridge
            string RemoteEndPoint=null;
            try
            {
                RemoteEndPoint = inputClient.Client.RemoteEndPoint.ToString();
                var outputClient = new TcpClient();
                outputClient.Connect(outputConn_IPAddress, outputConn_Port);

                if (TcpHelp.Bridge(inputClient, outputClient))
                {           
                    Console.WriteLine("[+]: "+DateTime.Now.ToString("[HH:mm:ss.fff]") + "转发成功:"+ inputConn_Port+"-->" + outputConn_IPAddress + ":"+ outputConn_Port);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-]: " + DateTime.Now.ToString("[HH:mm:ss.fff]") + "转发成功:" + inputConn_Port + "-->" + outputConn_IPAddress + ":"  + ex.GetBaseException().Message);
            }

        }


    }
}
