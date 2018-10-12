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

        public static void DownloadRemoteFile(SftpClient client, string remotePath, string remoteFileName, string localDir, string localFileName, bool deleteRemoteFileAfterDownload)
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(localDir, localFileName), FileMode.Create))
                {
                    client.DownloadFile(remotePath + "/" + remoteFileName, fs);
                }
                if (deleteRemoteFileAfterDownload)
                {
                    client.DeleteFile(remotePath + "/" + remoteFileName);
                }
            }
            catch (Exception e)
            {
                File.Delete(Path.Combine(localDir, localFileName));
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static byte[] DownloadRemoteFile(SftpClient client, string remotePath, string remoteFileName, bool deleteRemoteFileAfterDownload)
        {
            byte[] fileData;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    client.DownloadFile(remotePath + "/" + remoteFileName, ms);
                    fileData = ms.ToArray();
                }
                if (deleteRemoteFileAfterDownload)
                {
                    client.DeleteFile(remotePath + "/" + remoteFileName);
                }
                return fileData;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void UploadRemoteFile(SftpClient client, string remotePath, string remoteFileName, string localDir, string localFileName)
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(localDir, localFileName), FileMode.Open))
                {
                    client.UploadFile(fs, remotePath + "/" + remoteFileName);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void UploadRemoteFile(SftpClient client, string remotePath, string remoteFileName, byte[] fileData)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream(fileData))
                {
                    client.UploadFile(ms, remotePath + "/" + remoteFileName);
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

        public static void DeleteRemoteFile(SftpClient client, string remotePath, string remoteFileName)
        {
            try
            {
                client.DeleteFile(remotePath + "/" + remoteFileName);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        public static void DeleteRemoteFolderIfEmpty(SftpClient client, string remotePath, string remoteFolderName)
        {
            try
            {
                client.DeleteDirectory(remotePath + "/" + remoteFolderName);
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
