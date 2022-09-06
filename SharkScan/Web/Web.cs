using System;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;


namespace SharkScan.Web
{
    class Web
    {
        static private string GetHtml(string ip, int port, string _type="GET",int timeout=1000)
        {
            string result = "";
            if (!ip.ToLower().Contains("https://") && !ip.ToLower().Contains("http://"))
                ip = "http://" + ip;
            string url = ip + ":" + port.ToString();
            try
            {
                TcpClient clientSocket = new TcpClient();
                Uri URI = new Uri(url);
                clientSocket.Connect(URI.Host, URI.Port);
                StringBuilder RequestHeaders = new StringBuilder();
                clientSocket.ReceiveTimeout = timeout;
                clientSocket.SendTimeout = timeout;

                RequestHeaders.Append(_type + " " + URI.PathAndQuery + " HTTP/1.1\r\n");
                RequestHeaders.Append("Content-Type:application/x-www-form-urlencoded\r\n");
                RequestHeaders.Append("User-Agent:Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.11 (KHTML, like Gecko) Chrome/23.0.1271.64 Safari/537.11\r\n");
                RequestHeaders.Append("Cookie:"  + "\r\n");
                RequestHeaders.Append("Accept:*/*\r\n");
                RequestHeaders.Append("Host:" + URI.Host + "\r\n");
                RequestHeaders.Append("Content-Length:" + 0 + "\r\n");
                RequestHeaders.Append("Connection:close\r\n\r\n");

                byte[] request = Encoding.UTF8.GetBytes(RequestHeaders.ToString());
                clientSocket.Client.Send(request);
                byte[] responseByte = new byte[1024000];
                int len = clientSocket.Client.Receive(responseByte);
                result = Encoding.UTF8.GetString(responseByte, 0, len);
                clientSocket.Close();
                int headerIndex = result.IndexOf("\r\n\r\n");
                string[] headerStr = result.Substring(0, headerIndex).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
              
                return result;
            }
            catch
            {
                Debug.Write(3, "GetHtml ERROR");
            }
            return result;
        }
        static public string GetWebData(string ip, int port)
        {

            string html= GetHtml(ip, port);
            string titleName = "";
            string serverName = "";

            String serverRegex = @"Server:\s*(.*?)\n";
            String titleRegex = @"<title(.*?)title>";
            foreach(Match match in Regex.Matches(html, serverRegex, RegexOptions.IgnoreCase))
            {
                serverName = match.Groups[1].Value;
                serverName = serverName.Trim();


            }
            foreach (Match match in Regex.Matches(html, titleRegex, RegexOptions.IgnoreCase))
            {
                titleName = match.Groups[1].Value;
                
                titleName = titleName.Replace("<", "");
                titleName = titleName.Replace(">", "");
                titleName = titleName.Replace("/", "");
                titleName = titleName.Trim();
            }
            return serverName.ToString() + "\t" + titleName.ToString();
        }
    }
}
