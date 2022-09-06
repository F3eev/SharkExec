using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SharkScan.Exp.Ms17010
{
    public class TestResult
    {
        public bool IsVulnerable;
        public bool VulnerabilityOK;
        public bool hadError;
        public string error;
        public string OSName;
        public string OSBuild;
        public string Workgroup;
        public bool Active;
    }
    class Tester
    {
        const int bufferSize = 1024;
        private static int _timeout;
       
        public static TestResult TestIP(string ip, int timeout)
        {
            _timeout = timeout;
            TestResult testresult = new TestResult();
            try
            {
                byte[] buffer = new byte[bufferSize];
                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                client.ReceiveTimeout = _timeout;
                client.SendTimeout = _timeout;
                
                //IPAddress ipAddress = IPAddress.Parse(ip);
                IAsyncResult result = client.BeginConnect(ip, 445, null, null);

                bool success = result.AsyncWaitHandle.WaitOne(1000, true);
                if (!success)
                {
                  
                    testresult.Active = false;
                    testresult.IsVulnerable = false;
                    testresult.VulnerabilityOK = false;
                    testresult.hadError = true;
                    testresult.error = "";
                    return testresult;
                }
                var request = Constants.negotiateProtoRequest();
                client.Send(request);
                var receivedBytes = client.Receive(buffer, SocketFlags.Partial);
                Debug.Write(3, $"Received bytes: {receivedBytes}");
                var oBuff = buffer;

                /* Do nothing? */

                request = Constants.sessionSetupAndxRequest();
                client.Send(request);
                receivedBytes = client.Receive(buffer, SocketFlags.Partial);
                oBuff = buffer;               
                var netbiosB = Utils.ByteTake(oBuff, 0, 4);
                var smbHeaderB = Utils.ByteTake(oBuff, 4, 32);
                var smb = Constants.ByteArrayToSmbHeader(smbHeaderB);
                var sessionSetupAndxResponse = Utils.ByteTake(oBuff, 0, 36);
                var nativeOsB = Utils.ByteTake(sessionSetupAndxResponse, 9, sessionSetupAndxResponse.Length - 9);
                string[] osData = Encoding.ASCII.GetString(nativeOsB).Split('\x00');

                testresult.OSName = osData[0];
                if (osData.Length >= 3)
                {
                    testresult.OSBuild = osData[1];
                    testresult.Workgroup = osData[2];
                }
                Debug.Write(3, $"OS: {osData[0]} - UserId: {smb.user_id}");
                request = Constants.treeConnectAndxRequest(ip, smb.user_id);
                client.Send(request);
                receivedBytes = client.Receive(buffer, SocketFlags.Partial);
                /* oBuff = buffer.Take(receivedBytes).ToArray(); */
                oBuff = buffer;
                /* netbiosB = oBuff.Take(4).ToArray(); */
                netbiosB = Utils.ByteTake(oBuff, 0, 4);
                /* smbHeaderB = oBuff.Skip(4).Take(32).ToArray(); */
                smbHeaderB = Utils.ByteTake(oBuff, 4, 32);
                smb = Constants.ByteArrayToSmbHeader(smbHeaderB);

                request = Constants.peeknamedpipeRequest(smb.tree_id, smb.process_id, smb.user_id, smb.multiplex_id);
                client.Send(request);
                receivedBytes = client.Receive(buffer, SocketFlags.Partial);
                /* oBuff = buffer.Take(receivedBytes).ToArray(); */
                oBuff = buffer;
                /* netbiosB = oBuff.Take(4).ToArray(); */
                netbiosB = Utils.ByteTake(oBuff, 0, 4);

                /* smbHeaderB = oBuff.Skip(4).Take(32).ToArray(); */
                smbHeaderB = Utils.ByteTake(oBuff, 4, 32);
                smb = Constants.ByteArrayToSmbHeader(smbHeaderB);

                /*
				 * 0xC000 02 05 - STATUS_INSUFF_SERVER_RESOURCES - vulnerable
				 * 0xC000 00 08 - STATUS_INVALID_HANDLE
				 * 0xC000 00 22 - STATUS_ACCESS_DENIED
				 */
                testresult.IsVulnerable = (smb.error_class == 0x05 && smb.reserved1 == 0x02 && smb.error_code == 0xC000);
                testresult.VulnerabilityOK = (smb.error_class == 0x08 && smb.reserved1 == 0x00 && smb.error_code == 0xC000) ||
                              (smb.error_class == 0x22 && smb.reserved1 == 0x00 && smb.error_code == 0xC000);
                testresult.error = $"{smb.error_class:X2} {smb.reserved1:X2} {smb.error_code:X4}";
                testresult.Active = true;
                client.Close();
                return (testresult);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    testresult.Active = true;
                    testresult.IsVulnerable = false;
                    testresult.VulnerabilityOK = false;
                    testresult.hadError = true;
                    testresult.error = e.Message;
                    return testresult;
                }
            /*    testresult.IsVulnerable = false;
                testresult.VulnerabilityOK = false;
                testresult.hadError = true;
                testresult.error = e.Message;*/
                
            }
           
            return testresult;
        }
    }
}

