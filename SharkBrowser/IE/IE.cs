using System;
using Microsoft.Win32;
using Shark.lib;
using System;
using System.Security.Principal;
using System.Collections.Generic;
using System.Collections;

namespace Shark
{

    class IE
    {
        static ArrayList GetSID(RegistryKey rkey)
        {

            ArrayList allSID = new ArrayList();
            String[] names = rkey.GetSubKeyNames();
            foreach (String s in names)
            {
                if (s.ToString().Length == 45 )
                {
                    allSID.Add(s.ToString());
                }
            }
            return allSID;
        }
        /*
         * 高权限允许获取其他用户的注册表
         * 
         */
        public static void GetHistroy(int num)
        {
            RegistryKey rk = Registry.Users;
            ArrayList allSID = GetSID(rk);
            foreach (string  s in allSID)
            {
                SecurityIdentifier sid = new SecurityIdentifier(s);
                string Username = GetNameFromSID(sid);
                Common.showMessage("[*]", string.Format("Try to get {0} IE Histroy num: {1}", Username,num));
                int i = 0;
                try
                {
                    RegistryKey UrlKey = Registry.Users.OpenSubKey(sid + @"\Software\\Microsoft\\Internet Explorer\\TypedURLs");

                    // RegistryKey UrlKey = Registry.CurrentUser.OpenSubKey(@"Software\\Microsoft\\Internet Explorer\\TypedURLs");
                    foreach (string value in UrlKey.GetValueNames())
                    {
                        if (i == num)
                        {
                            break;
                        }
                        i = i + 1;
                        Common.showMessage("[+]", string.Format("Title:{0} url:{1} ", "", UrlKey.GetValue(value).ToString()));
                    }
                }
                catch (System.Security.SecurityException e1)
                {
                    Common.showMessage("[-]", string.Format("Access forbidden"));

                }
                Common.showMessage("[*]", string.Format("Try to get {0} IE Histroy num:{1} End", Username,num));
            }
        }

        static string GetNameFromSID(SecurityIdentifier sid)
        {
            NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
            return ntAccount.ToString();
        }
    }


}
