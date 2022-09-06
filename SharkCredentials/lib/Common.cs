using System;
using System.Security.Principal;

namespace GetCredentials.lib
{
    class Common
    {

        public static void showMessage(string s, string mess)
        {
            
            Console.WriteLine("{0}: {1}",s,mess);

        }
        public static void showBrowserPassword(string url,string username,string password)
        {
            Common.showMessage("[+]", String.Format("URL:{0} USERNAME:{1} PASSWORD:{2}", url, username, password));

        }
        public static void showBrowserHistroy(string title,string url)
        {
            Console.WriteLine("Title:{0} url:{1} ", title, url);

        }

        public static bool IsHighIntegrity()
        {
            // returns true if the current process is running with adminstrative privs in a high integrity context
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

}
