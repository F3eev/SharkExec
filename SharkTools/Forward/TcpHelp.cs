using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Forward
{

    /// <summary>
    /// 外部负责关闭传入的TcpClient
    /// </summary>
    /// <param name="client"></param>
    public delegate void OnConnected(TcpClient client);

    public delegate void OnException(Exception e);



    public class TcpHelp
    {
        #region TcpClientIsConnected
        public static bool TcpClientIsConnected(TcpClient c,int microSeconds=500)
        {
            return null != c && c.Client.Connected  &&  !(c.Client.Poll(microSeconds, SelectMode.SelectRead) && (c.Client.Available == 0) );
        }
        #endregion

        #region ParseToIPAddress
        public static IPAddress ParseToIPAddress(string host)
        {
            IPAddress ipAddress;
            #region 获取ip地址
            if (!IPAddress.TryParse(host, out ipAddress))
            {
                IPHostEntry hostInfo = Dns.GetHostEntry(host);
                ipAddress = hostInfo.AddressList[0];
            }
            #endregion
            return ipAddress;
        }
        #endregion

               
        #region Bridge

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientA"></param>
        /// <param name="clientB"></param>
        /// <returns></returns>
        public static bool Bridge(TcpClient clientA, TcpClient clientB)
        {
            if (!clientA.Connected || !clientB.Connected)
            {                
                clientA.Close();
                clientB.Close();
                return false;
            }

            //桥接
            #region get data from clientA  to clientB
            new Task(() =>
            {
                try
                {
                    using (NetworkStream reader = clientA.GetStream())
                    using (NetworkStream writer = clientB.GetStream())
                    {
                        byte[] buffer = new byte[1024];
                        int size;   
                       
                        while (true)
                        {
                            size = reader.Read(buffer, 0, buffer.Length);
                            if (size > 0) writer.Write(buffer, 0, size);
                            else
                                if (!TcpClientIsConnected(clientA))
                                    break;
                        }
                    }
                }
                catch { }

                try
                {
                    clientA.Close();
                }
                catch { }

                try
                {
                    clientB.Close();
                }
                catch { }

            }).Start();
            #endregion


            #region get data from clientB  to clientA
            new Task(() =>
            {
                try
                {
                    using (NetworkStream reader = clientB.GetStream())
                    using (NetworkStream writer = clientA.GetStream())
                    {                       
                        byte[] buffer = new byte[1024];
                        int size;                        
                        while (true)
                        {
                            size = reader.Read(buffer, 0, buffer.Length);
                            if (size > 0) writer.Write(buffer, 0, size);
                            else
                                if (!TcpClientIsConnected(clientB))
                                break;
                        }
                    }
                }
                catch { }

                try
                {
                    clientA.Close();
                }
                catch { }

                try
                {
                    clientB.Close();
                }
                catch { }

            }).Start();
            #endregion


            return true;
        }
        #endregion


        #region Listening
        /// <summary>
        /// 新开线程，监听。
        /// 外部负责关闭返回的TcpListener
        /// </summary>
        /// <param name="port"></param>
        /// <param name="onConnected"></param>
        /// <returns></returns>
        public static TcpListener Listening(int port, OnConnected onConnected, OnException onException= null       )
        {
            if (null == onException)
            {
                onException = (e) => { };
            }
            var listener = new TcpListener(new System.Net.IPEndPoint(0, port));
            try
            {
                listener.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine("[-]: 端口被占用");
                throw e;
            }
            try
            {
                new Task(() =>
                {
                    try
                    {
                        while (true)
                        {
                            var client = listener.AcceptTcpClient();
                            new Task(() =>
                            {
                                try
                                {
                                    onConnected(client);
                                }
                                catch (Exception e)
                                {
                                    try
                                    {
                                        onException(e);
                                    }
                                    catch { }
                                }
                            }).Start();
                        }
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            onException(e);
                        }
                        catch { }
                    }
                }).Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                try
                {
                    listener.Stop();
                }
                catch { }

                try
                {
                    onException(e);
                }
                catch { }
            }
            return listener;
        }

        #endregion

        #region Connect
        /// <summary>
        /// 新开线程，连接
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="onConnected"></param>
        /// <param name="onException"></param>
        public static void Connect(string host, int port, OnConnected onConnected, OnException onException = null)
        {
            Connect(ParseToIPAddress(host),port, onConnected, onException);
        }


        public static void Connect(IPAddress ipAddress, int port, OnConnected onConnected, OnException onException = null)
        {
            if (null == onException)
            {
                onException = (e) => { };
            }
            new Task(() =>
            {
                try
                {
                    var client = new TcpClient();

                    client.Connect(ipAddress, port);
                    //var stream = client.GetStream();

                    onConnected(client);
                }
                catch(Exception e)
                {
                    try
                    {
                        onException(e);
                    }
                    catch { }                    
                }
            }).Start();
        }
        #endregion

    }

    class MyException : Exception
    {
        public MyException(string message) : base(message)
        {
        }
    }
}
