using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;


namespace GetCredentials.lib
{
    class Dpapi
    {

    
        private static ArrayList GetCredFilePath()
        {
            ArrayList userPaths = new ArrayList();
            ArrayList allCredentialsPaths = new ArrayList();
            if (Common.IsHighIntegrity())
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
            ArrayList systemPaths = new ArrayList()
                {
                    String.Format("{0}ServiceProfiles\\LocalService\\AppData\\Local\\Microsoft\\CredentialsServiceProfiles\\LocalService\\AppData\\Local\\Microsoft\\Credentials", Environment.GetEnvironmentVariable("SystemRoot")),
                    String.Format("{0}\\System32\\config\\systemprofile\\AppData\\Roaming\\Microsoft\\Credentials", Environment.GetEnvironmentVariable("SystemRoot")),
                    String.Format("{0}\\ServiceProfiles\\LocalService\\AppData\\Local\\Microsoft\\Credentials", Environment.GetEnvironmentVariable("SystemRoot")),
                    String.Format("{0}\\ServiceProfiles\\LocalService\\AppData\\Roaming\\Microsoft\\Credentials", Environment.GetEnvironmentVariable("SystemRoot")),
                    String.Format("{0}\\ServiceProfiles\\NetworkService\\AppData\\Local\\Microsoft\\Credentials", Environment.GetEnvironmentVariable("SystemRoot")),
                    String.Format("{0}\\ServiceProfiles\\NetworkService\\AppData\\Roaming\\Microsoft\\Credentials", Environment.GetEnvironmentVariable("SystemRoot"))
                };


            foreach (string path in systemPaths)
            {
                userPaths.Add(path);
            }


            foreach (string path in userPaths)
            {
                //Console.WriteLine(path);
                //string userCredentialsPath = String.Format("{0}\\AppData\\Local\\Microsoft\\Credentials\\", path);
                ArrayList CredentialsPathModel = new ArrayList()
                {
                     String.Format("{0}\\AppData\\Local\\Microsoft\\Credentials\\", path),
                     String.Format("{0}\\AppData\\Roaming\\Microsoft\\Credentials\\", path),

                };
                foreach (string userCredentialsPath in CredentialsPathModel)
                {
                    if (Directory.Exists(userCredentialsPath))
                    {
                        string[] filePaths = Directory.GetFiles(userCredentialsPath);
                        foreach (string fp in filePaths)
                        {
                            Dictionary<string, string> CredentialsPath = new Dictionary<string, string>()
                        {
                             {"CredentialsPath",fp},
                             {"userDir", path}
                        };
                            allCredentialsPaths.Add(CredentialsPath);
                        }
                    }
                }
            }
            return allCredentialsPaths;
        }

        public static ArrayList GetCredFileMasterKeyGuid()
        {
            ArrayList CredFilePaths = GetCredFilePath();
            var userMasterKeyGuids = new ArrayList();
            foreach (Dictionary<string, string> CredFilePath in CredFilePaths)
            {
                CredFilePath.TryGetValue("CredentialsPath", out string userCredFilePath);
                CredFilePath.TryGetValue("userDir", out string userDir);
                string userName = GetUserName(userDir);
                Dictionary<string, string> userMasterKeyGuid = new Dictionary<string, string>() 
                {
                    {"userName" ,userName},
                    {"CredentialFilePath", userCredFilePath},
                    {"MasterKeyGuid",ParseCredFiles(userCredFilePath) },
                    {"Credentials",System.IO.Path.GetFileName(userCredFilePath) }
                };
                userMasterKeyGuids.Add(userMasterKeyGuid);
            }

            return userMasterKeyGuids;
        }
        private static string GetUserName(string userDir)
        {
            string userName = "";
            if (Common.IsHighIntegrity())
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

        public static string ParseCredFiles(string credFilePath)
        {
            
            // jankily parse the bytes to extract the credential type and master key GUID
            // reference- https://github.com/gentilkiwi/mimikatz/blob/3d8be22fff9f7222f9590aa007629e18300cf643/modules/kull_m_dpapi.h#L24-L54
            byte[] credentialArray = File.ReadAllBytes(credFilePath);
            byte[] guidMasterKeyArray = new byte[16];
            Array.Copy(credentialArray, 36, guidMasterKeyArray, 0, 16);
            Guid guidMasterKey = new Guid(guidMasterKeyArray);
            byte[] stringLenArray = new byte[16];
            Array.Copy(credentialArray, 56, stringLenArray, 0, 4);
            int descLen = BitConverter.ToInt32(stringLenArray, 0);
            byte[] descBytes = new byte[descLen];
            Array.Copy(credentialArray, 60, descBytes, 0, descLen - 4);
            // Console.WriteLine("found " + credFilePath + " --> "+ guidMasterKey.ToString());
            return guidMasterKey.ToString();
       }
        
        
    }
}
