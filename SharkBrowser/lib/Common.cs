using System;
using System.IO;
using System.Reflection;

namespace Shark.lib
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

        


    }

}
