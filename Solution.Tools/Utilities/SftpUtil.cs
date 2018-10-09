using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using log4net;
using Renci.SshNet;
using Renci.SshNet.Sftp;


namespace Solution.Tools.Utilities
{
    /// <summary>
    /// Utility to initialize and manage operations on SFTP server
    /// REQUIRE: SSH.NET
    /// </summary>
    public static class SftpUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public enum SftpElementListType
        {
            FILES_ONLY = 0,
            DIRECTORIES_ONLY = 1,
            FILES_AND_DIRECTORIES = 2
        }


        #region Connection Creation

        public static SftpClient CreateClient(string hostname, int port, string username, string password, string privateKeyFile, string passPhrase)
        {
            AuthenticationMethod authenticationMethod = null;
            PrivateKeyFile keyFile = null;
            PrivateKeyFile[] keyFiles = null;

            if (string.IsNullOrEmpty(privateKeyFile))
                authenticationMethod = new PasswordAuthenticationMethod(username, password);
            else
            {
                keyFile = new PrivateKeyFile(privateKeyFile, passPhrase);
                keyFiles = new[] { keyFile };
                authenticationMethod = new PrivateKeyAuthenticationMethod(username, keyFiles);
            }
            ConnectionInfo connectionInfo = new ConnectionInfo(hostname, port, username, new AuthenticationMethod[] { authenticationMethod });
            return new SftpClient(connectionInfo);
        }

        #endregion


        #region Remotes methods

        public static List<string> GetRemoteFileList(SftpClient client, string remotePath, string searchFilter = "*")
        {
            return GetRemoteElementList(client, remotePath, searchFilter, SftpElementListType.FILES_ONLY);
        }

        public static List<string> GetRemoteFolderList(SftpClient client, string remotePath, string searchFilter = "*")
        {
            return GetRemoteElementList(client, remotePath, searchFilter, SftpElementListType.DIRECTORIES_ONLY);
        }

        public static List<string> GetRemoteFileAndFolderList(SftpClient client, string remotePath, string searchFilter = "*")
        {
            return GetRemoteElementList(client, remotePath, searchFilter, SftpElementListType.FILES_AND_DIRECTORIES);
        }

        public static void DownloadRemoteFile(SftpClient client, string remotePath, string fileName, string outputDir, bool deleteRemoteFileAfterDownload)
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(outputDir, fileName), FileMode.Create))
                {
                    client.DownloadFile(remotePath + "/" + fileName, fs);
                }
                if (deleteRemoteFileAfterDownload)
                {
                    client.DeleteFile(remotePath + "/" + fileName);
                }
            }
            catch (Exception e)
            {
                File.Delete(Path.Combine(outputDir, fileName));
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static byte[] DownloadRemoteFile(SftpClient client, string remotePath, string fileName, bool deleteRemoteFileAfterDownload)
        {
            byte[] fileData;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    client.DownloadFile(remotePath + "/" + fileName, ms);
                    fileData = ms.ToArray();
                }
                if (deleteRemoteFileAfterDownload)
                {
                    client.DeleteFile(remotePath + "/" + fileName);
                }
                return fileData;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void UploadRemoteFile(SftpClient client, string remotePath, string fileName, string inputDir)
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(inputDir, fileName), FileMode.Open))
                {
                    client.UploadFile(fs, remotePath + "/" + fileName);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void UploadRemoteFile(SftpClient client, string remotePath, string fileName, byte[] fileData)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(fileData))
                {
                    client.UploadFile(ms, remotePath + "/" + fileName);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void RenameRemoteFile(SftpClient client, string remotePath, string oldFileName, string newFileName)
        {
            try
            {
                client.RenameFile(remotePath + "/" + oldFileName, remotePath + "/" + newFileName);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void DeleteRemoteFile(SftpClient client, string remotePath, string fileName)
        {
            try
            {
                client.DeleteFile(remotePath + "/" + fileName);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void DeleteRemoteFolderIfEmpty(SftpClient client, string remotePath, string folderName)
        {
            try
            {
                client.DeleteDirectory(remotePath + "/" + folderName);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        #endregion


        #region Support

        private static List<string> GetRemoteElementList(SftpClient client, string remotePath, string elementFilter, SftpElementListType type)
        {
            List<string> elementList = new List<string>();
            Regex myRegex = (string.IsNullOrEmpty(elementFilter) || elementFilter == "*") ? new Regex("^*$") : new Regex("^" + elementFilter.Replace("*", ".*") + "$");

            try
            {
                IEnumerable<SftpFile> files = client.ListDirectory(remotePath);
                foreach (SftpFile file in files)
                {
                    if (myRegex.IsMatch(file.Name))
                    {
                        switch (type)
                        {
                            case SftpElementListType.FILES_AND_DIRECTORIES:
                                elementList.Add(file.Name);
                                break;
                            case SftpElementListType.FILES_ONLY:
                                if (!file.IsDirectory)
                                    elementList.Add(file.Name);
                                break;
                            case SftpElementListType.DIRECTORIES_ONLY:
                                if (file.IsDirectory)
                                    elementList.Add(file.Name);
                                break;
                        }
                    }
                }
                return elementList;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        #endregion

    }
}
