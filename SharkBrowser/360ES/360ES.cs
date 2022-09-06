using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using CO_SQLite3;
using Shark.lib;
using CS_SQLite3;
namespace Shark
{
    class _360ES
    {
        public static void GetHistroy(int num)
        {
            Process[] _360ESProcesses = Process.GetProcessesByName("360se");
            ArrayList allPath = GetPath();
            Common.showMessage("[*]", string.Format("Try to get 360ES Histroy count:{0}", num.ToString()));
            foreach (Dictionary<string, string> _360ESPaths in allPath)
            {
                _360ESPaths.TryGetValue("360ESHistoryPath", out string _360ESHistoryPath);
                _360ESPaths.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                if (File.Exists(_360ESHistoryPath))
                {
                    Common.showMessage("[*]", string.Format("Try to get {0} 360ES Histroy count:{1}", userName, num.ToString()));
                    if (_360ESProcesses.Length > 0)
                    {
                        _360ESHistoryPath = CreateTempFile(_360ESHistoryPath);
                        ParseHistory(_360ESHistoryPath, num, userName);
                        File.Delete(_360ESHistoryPath);
                    }
                    else
                    {
                        ParseHistory(_360ESHistoryPath, num, userName);
                    }
                    Common.showMessage("[*]", string.Format("Get {0} 360ES Histroy end", userName));
                }
            }
            Common.showMessage("[*]", string.Format("Try to get 360ES Histroy count:{0} end", num.ToString()));
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
        private static void ParseHistory(string historyFilePath, int num, string user)
        {
            SQLiteDatabase database = new SQLiteDatabase(historyFilePath);
            string query = "SELECT title, url FROM urls order by  visit_count limit " + num.ToString() + "; ";
            System.Data.DataTable resultantQuery = database.ExecuteQuery(query);
            database.CloseDatabase();
            foreach (System.Data.DataRow row in resultantQuery.Rows)
            {
                Common.showMessage("[+]", string.Format("Title:{0} url:{1} ", row["title"], row["url"]));
            }
            database.CloseDatabase();
         
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
                string user360ESBasePath = String.Format(@"{0}\AppData\Roaming\360se6\User Data\Default", path);
                if (Directory.Exists(user360ESBasePath))
                {
                    Dictionary<string, string> _360ESPath = new Dictionary<string, string>()
                    {
                         {"360ESHistoryPath",String.Format("{0}\\{1}", user360ESBasePath,"History")},
                        // {"360ESBookmarkPath",String.Format("{0}\\{1}", user360ESBasePath, "Bookmarks") },
                        // {"360ESLoginDataPath",String.Format("{0}\\{1}", user360ESBasePath, "Login Data") },
                        // {"360ESCookiesPath",String.Format("{0}\\{1}", user360ESBasePath, "Cookies") },
                         {"userDir", path}
                    };
                    allPath.Add(_360ESPath);
                }
            }
            return allPath;
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
