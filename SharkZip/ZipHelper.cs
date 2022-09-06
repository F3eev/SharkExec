using System.Text;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Security.Cryptography;
using System;

namespace SharkZip
{
    public class ZipHelper
    {
        /// <summary>
        /// 压缩单个文件
        /// </summary>
        /// <param name="fileToZip">需压缩的文件名</param>
        /// <param name="zipFile">压缩后的文件名（文件名都是绝对路径）</param>
        /// <param name="level">压缩等级（0-9）</param>
        /// <param name="password">压缩密码（解压是需要的密码）</param>
        public static void ZipFile(string fileToZip, string zipFile, string password = "",int level = 5 )
        {
            if (!File.Exists(fileToZip))
                throw new FileNotFoundException("压缩文件" + fileToZip + "不存在");
            using (FileStream fs = File.OpenRead(fileToZip))
            {
                fs.Position = 0;//设置流的起始位置
                byte[] buffer = new byte[(int)fs.Length];
                fs.Read(buffer, 0, buffer.Length);//读取的时候设置Position，写入的时候不需要设置
                fs.Close();
                using (FileStream zfstram = File.Create(zipFile))
                {
                    using (ZipOutputStream zipstream = new ZipOutputStream(zfstram))
                    {
                        if (password != "")
                        {
                            zipstream.Password = md5(password);//设置属性的时候在PutNextEntry函数之前
                        }
                        zipstream.SetLevel(level);
                        string fileName = fileToZip.Substring(fileToZip.LastIndexOf('\\') + 1);
                        ZipEntry entry = new ZipEntry(fileName);
                        zipstream.PutNextEntry(entry);
                        zipstream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        /// <summary>
        /// 压缩多个文件目录
        /// </summary>
        /// <param name="dirname">需要压缩的目录</param>
        /// <param name="zipFile">压缩后的文件名</param>
        /// <param name="level">压缩等级</param>
        /// <param name="password">密码</param>
        public static void ZipDir(string dirname, string zipFile, string password = "",int level = 5 )
        {
            ZipOutputStream zos = new ZipOutputStream(File.Create(zipFile));

            if (password != "")
            {
                zos.Password = md5(password);
            }

            zos.SetLevel(level);
            addZipEntry(dirname, zos, dirname);
            zos.Finish();
            zos.Close();
        }
        /// <summary>
        /// 往压缩文件里面添加Entry
        /// </summary>
        /// <param name="PathStr">文件路径</param>
        /// <param name="zos">ZipOutputStream</param>
        /// <param name="BaseDirName">基础目录</param>
        private static void addZipEntry(string PathStr, ZipOutputStream zos, string BaseDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(PathStr);
            foreach (FileSystemInfo item in dir.GetFileSystemInfos())
            {
                if ((item.Attributes & FileAttributes.Directory) == FileAttributes.Directory)//如果是文件夹继续递归
                {
                    addZipEntry(item.FullName, zos, BaseDirName);
                }
                else
                {
                    FileInfo f_item = (FileInfo)item;
                    using (FileStream fs = f_item.OpenRead())
                    {
                        byte[] buffer = new byte[(int)fs.Length];
                        fs.Position = 0;
                        fs.Read(buffer, 0, buffer.Length);
                        fs.Close();
                        ZipEntry z_entry = new ZipEntry(item.FullName.Replace(BaseDirName, ""));
                        zos.PutNextEntry(z_entry);
                        zos.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }
        
        /// <summary>
        /// ZIP:解压一个zip文件
        /// add yuangang by 2016-06-13
        /// </summary>
        /// <param name="ZipFile">需要解压的Zip文件（绝对路径）</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public static void UnZip(string ZipFile, string TargetDirectory,string password="", bool OverWrite = true)
        {
            
            if (!Directory.Exists(TargetDirectory)) Directory.CreateDirectory(TargetDirectory);
            //目录结尾
            if (!TargetDirectory.EndsWith("\\")) { TargetDirectory = TargetDirectory + "\\"; }

            using (ZipInputStream zipfiles = new ZipInputStream(File.OpenRead(ZipFile)))
            {
                if (password != "")
                {
                    zipfiles.Password = md5(password);
                }

                ZipEntry theEntry;

                while ((theEntry = zipfiles.GetNextEntry()) != null)
                {
                    string directoryName = "";
                    string pathToZip = "";
                    pathToZip = theEntry.Name;

                    if (pathToZip != "")
                        directoryName = Path.GetDirectoryName(pathToZip) + "\\";

                    string fileName = Path.GetFileName(pathToZip);

                    Directory.CreateDirectory(TargetDirectory + directoryName);

                    if (fileName != "")
                    {
                        if ((File.Exists(TargetDirectory + directoryName + fileName) && OverWrite) || (!File.Exists(TargetDirectory + directoryName + fileName)))
                        {
                            using (FileStream streamWriter = File.Create(TargetDirectory + directoryName + fileName))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = zipfiles.Read(data, 0, data.Length);

                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                                streamWriter.Close();
                            }
                        }
                    }
                }

                zipfiles.Close();
            }

        }


        private static string md5(string pwd)
        {
            var res = "";
            MD5 md = MD5.Create();
            byte[] s = md.ComputeHash(Encoding.Default.GetBytes(pwd));
            for (int i = 0; i < s.Length; i++)
                res = res + s[i].ToString("X");
            return res;
        }
    }
}