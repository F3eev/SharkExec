using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace SharkScan.Web
{
    class webscan
    {
        public static string run(string ip)
        {
            if (string.IsNullOrEmpty(ip))
                return "";
            else
            {
                String aa = ip+ "\t" + GetTitle(getHtml("http://" + ip, 2));
                Console.WriteLine(aa);
                return aa;
            }
        }
        private static string getURLbanner(string url)
        {
            ////HttpWebResponse res;
            if (!url.ToLower().Contains("https://") && !url.ToLower().Contains("http://"))
                url = "http://" + url;
            try
            {
                var req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
                req.Method = "HEAD";
                req.Timeout = 1000;
                var res = (HttpWebResponse)req.GetResponse();

                if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.Forbidden || res.StatusCode == HttpStatusCode.Redirect || res.StatusCode == HttpStatusCode.MovedPermanently)
                {
                    return res.Server;
                }
                Console.WriteLine(res.Server);
                return res.Server;
            }
            catch (WebException ex)
            {
                return "";
            }
        }

        private static string GetTitle(string html)
        {
            String regex = @"<title.*?<";
            String title = Regex.Match(html, regex, RegexOptions.IgnoreCase).ToString();
            title = title.Replace("title","");
            title = title.Replace(" ", "");
            title = title.Replace("<", "");
            title = title.Replace(">", "");
            if (title.Length > 50)
                return title.Substring(0, 50);
            Console.WriteLine(title);
            return title;
        }

        private static string getHtml(string url, int codingType)
        {
            try
            {
                if (!url.ToLower().Contains("https://") && !url.ToLower().Contains("http://"))
                    url = "http://" + url;
                WebClient myWebClient = new WebClient();
                if (url.ToLower().Contains("https://"))
                {
                    ServicePointManager.ServerCertificateValidationCallback +=
                    delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                             System.Security.Cryptography.X509Certificates.X509Chain chain,
                             System.Net.Security.SslPolicyErrors sslPolicyErrors)
                    {
                        return true; // **** Always accept
                    };

                }
                byte[] myDataBuffer = myWebClient.DownloadData(url);
                //return Encoding.Default.GetString(myDataBuffer);
                string strWebData = System.Text.Encoding.Default.GetString(myDataBuffer);

                //自动识别编码  不一定有<meta  比如 百度开放平台 content="text/html; charset=gbk">
                //Match charSetMatch = Regex.Match(strWebData, "<meta([^>]*)charset=(\")?(.*)?\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                Match charSetMatch = Regex.Match(strWebData, "(.*)charset=(\")?(.*)?\"", RegexOptions.IgnoreCase | RegexOptions.Multiline);

                string webCharSet = charSetMatch.Groups[3].Value.Trim().ToLower();

                if (webCharSet != "gb2312" && webCharSet != "gbk")
                {
                    webCharSet = "utf-8";
                }

                if (System.Text.Encoding.GetEncoding(webCharSet) != System.Text.Encoding.Default)
                {
                    strWebData = System.Text.Encoding.GetEncoding(webCharSet).GetString(myDataBuffer);
                }
                return strWebData;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(url + " " + ex.Message);
                return "<hTmlKErRor>" + ex.Message;
            }
            return "";
        }

    }
}
