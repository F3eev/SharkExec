using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using Shark.lib;
using CS_SQLite3;
namespace Shark
{
    class _360Chrome
    {
        public static void GetFavicons(int num)
        {
            Process[] _360ChromeProcesses = Process.GetProcessesByName("360chrome");
            ArrayList allPath = GetPath();
            Common.showMessage("[*]", string.Format("Try to get 360Chrome Favicons count:{0}", num.ToString()));
            foreach (Dictionary<string, string> _360ChromePaths in allPath)
            {
                _360ChromePaths.TryGetValue("360ChromeFaviconsPath", out string _360ChromeFaviconsPath);
                _360ChromePaths.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                if (File.Exists(_360ChromeFaviconsPath))
                {

                    Common.showMessage("[*]", string.Format("Try to get {0} 360chrome Favicons count:{1}", userName, num.ToString()));
                    if (_360ChromeProcesses.Length > 0)
                    {
                        _360ChromeFaviconsPath = CreateTempFile(_360ChromeFaviconsPath);
                        ParseFavicons(_360ChromeFaviconsPath, num, userName);
                        File.Delete(_360ChromeFaviconsPath);
                    }
                    else
                    {
                        ParseFavicons(_360ChromeFaviconsPath, num, userName);
                    }
                    Common.showMessage("[*]", string.Format("Get {0} 360chrome Favicons end", userName));
                }
            }

            Common.showMessage("[*]", string.Format("Try to get 360chrome Favicons count:{0} end", num.ToString()));
        }

        public static void GetHistroy(int num)
        {
           
            Process[] _360ChromeProcesses = Process.GetProcessesByName("360chrome");
            ArrayList allPath = GetPath();
            Common.showMessage("[*]", string.Format("Try to get 360Chrome Histroy count:{0}", num.ToString()));
            foreach (Dictionary<string, string> _360ChromePaths in allPath)
            {
                _360ChromePaths.TryGetValue("360ChromeHistoryPath", out string _360ChromeHistoryPath);
                _360ChromePaths.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                if (File.Exists(_360ChromeHistoryPath))
                {
                    Common.showMessage("[*]", string.Format("Try to get {0} 360chrome Histroy count:{1}", userName, num.ToString()));
                    if (_360ChromeProcesses.Length > 0)
                    {
                        _360ChromeHistoryPath = CreateTempFile(_360ChromeHistoryPath);
                        ParseHistory(_360ChromeHistoryPath, num, userName);
                        File.Delete(_360ChromeHistoryPath);
                    }
                    else
                    {
                        ParseHistory(_360ChromeHistoryPath, num, userName);
                    }
                    Common.showMessage("[*]", string.Format("Get {0} 360chrome Histroy end", userName));
                }
            }
            Common.showMessage("[*]", string.Format("Try to get 360chrome Histroy count:{0} end", num.ToString()));

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
        private static void ParseFavicons(string historyFilePath, int num, string user)
        {

            CO_SQLite3.SqliteDatabase db = new CO_SQLite3.SqliteDatabase();
            db.Open(historyFilePath);
            CO_SQLite3.DataTable dt = db.ExecuteQuery("SELECT id, url FROM Favicons limit" + num.ToString() + "; ");
            foreach (CO_SQLite3.DataRow dbgRow in dt.Rows)
            {
                Common.showMessage("[+]", string.Format("Title:{0} url:{1} ", dbgRow["title"], dbgRow["url"]));
            }
            db.Close();
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
                string user360ChromeBasePath = String.Format(@"{0}\AppData\Local\360Chrome\Chrome\User Data\Default", path);


                if (Directory.Exists(user360ChromeBasePath))
                {
                    Dictionary<string, string> _360ChromePath = new Dictionary<string, string>()
                    {
                         {"360ChromeHistoryPath",String.Format("{0}\\{1}", user360ChromeBasePath,"History")},
                         {"360ChromeFaviconsPath",String.Format("{0}\\{1}", user360ChromeBasePath, "Favicons") },
                      
                         {"userDir", path}
                    };
                    allPath.Add(_360ChromePath);
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
