using System;
using System.Net.Sockets;
using System.Net;

namespace SharkScan.SMB
{
    internal class UDPCli : UdpClient
    {

        protected byte[] m_Buffer;

        protected int m_Timeout = 0;

        /// Connection timeout in milliseconds
        public int Timeout
        {
            get
            {
                if (m_Timeout <= 0)
                    return 1000;    // default to 1000 ms
                else
                    return m_Timeout;
            }
            set
            {
                m_Timeout = Math.Abs(value);    // we don't want negative seconds!
            }
        }

        /// <summary>
        /// This will attempt to receive a UDP packet for n milliseconds
        /// based on the Timeout property
        /// </summary>
        /// <param name="EP">Endpoint to receive from</param>
        /// <param name="buffer">Buffer to hold received data</param>
        /// <returns>True if data received, else, false</returns>
        public bool DoReceive(ref IPEndPoint EP, ref byte[] buffer)
        {

            //Client.Blocking = false;

            //Client.SetSockOpt(SocketOption.SolSocket, SocketOption.SoRcvTimeo,  Timeout);
            Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Timeout);
            m_Buffer = new Byte[1024];

            int bytes = 0;

            try
            {
                bytes = Client.Receive(m_Buffer, m_Buffer.Length, 0);
                
            }
            catch (Exception ex) //ignore it for now
            {
                Client.Close();
                
                //Console.Write(ex);
            }
            finally
            {
                Client.Close();
            }


            buffer = m_Buffer;
            m_Buffer = null;

            return !(bytes <= 0);

        }
    } // class
}
