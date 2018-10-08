using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class FileUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Check if the current process alredy exist in the system process list
        /// </summary>
        /// <returns></returns>
        public static bool CheckAlreadyRunning()
        {
            if (System.Diagnostics.Process.GetProcessesByName(
                System.IO.Path.GetFileNameWithoutExtension(
                    System.Reflection.Assembly.GetEntryAssembly().Location
                    )).Count() > 1)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Find the full path for specified file relative to application executable
        /// </summary>
        /// <param name="fileName">File to found relative to executable</param>
        /// <returns></returns>
        public static string GetLocalExecutionFullPah(string fileName = "")
        {
            try
            {
                return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), fileName);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Find out the full path of a specified folder
        /// </summary>
        /// <param name="path">Path (relative or full) that will be located in the system</param>
        /// <returns>Full path of the specified folder</returns>
        public static string GetFullPath(string path)
        {
            if (path.StartsWith("~"))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Replace("~/", "").Replace("~\\", ""));
            }
            else
            {
                return path;
            }
        }

        /// <summary>
        /// Copy all file and subdirectory inside a directory to a target
        /// </summary>
        /// <param name="source">Selected folder to be copied</param>
        /// <param name="target">Destination folder for the copy operation</param>
        /// <param name="overwrite">What to do in case a subfolder/file already exist</param>
        /// <returns></returns>
        public static void CopyAll(DirectoryInfo source, DirectoryInfo target, bool overwrite = false)
        {
            try
            {
                if (Directory.Exists(target.FullName) == false)
                {
                    Directory.CreateDirectory(target.FullName);
                }
                // Copy each file into it’s new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    fi.CopyTo(Path.Combine(target.ToString(), fi.Name), overwrite);
                }
                // Copy each subdirectory using recursion.
                foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
                {
                    DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                    CopyAll(diSourceSubDir, nextTargetSubDir, overwrite);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Create or clean the specified target directory
        /// </summary>
        /// <param name="target">DirectoryInfo to be initialized</param>
        /// <param name="overwrite">Params to tell if overwrite already existing directory</param>
        /// <returns></returns>
        public static void InitDirectory(DirectoryInfo target, bool overwrite = false)
        {
            try
            {
                if (target.Exists)
                {
                    if (overwrite)
                    {
                        CleanDirectory(target);
                    }
                }
                else
                {
                    target.Create();
                }
                target.Refresh();
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Remove all file and directory inside a specified target
        /// </summary>
        /// <param name="target">Directory to be cleaned</param>
        /// <returns></returns>
        public static void CleanDirectory(DirectoryInfo target)
        {
            try
            {
                target.Refresh();
                foreach (FileInfo file in target.GetFiles())
                {
                    DeleteFile(file);
                }
                foreach (DirectoryInfo dir in target.GetDirectories())
                {
                    CleanDirectory(dir);
                    dir.Refresh();
                    dir.Delete();
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Calculates the size of the directory
        /// </summary>
        /// <param DirectoryInfo="target">DirectoryInfo targheting folder whose size must be retrieved</param>
        /// <param name="searchPattern">Filter file to be included in size calculation</param>
        /// <returns></returns>
        /// <exception cref="DirectoryNotFoundException">Thrown when directory path doesn't exist</exception>
        public static long GetDirectorySize(DirectoryInfo target, string searchPattern = "*")
        {
            if (!target.Exists)
                throw new DirectoryNotFoundException();

            return target.GetFiles(searchPattern, SearchOption.AllDirectories).Sum(t => t.Length);
        }

        /// <summary>
        /// Delete file even if it is a readonly one
        /// </summary>
        /// <param name="file">FileInfo of file to be removed</param>
        public static void DeleteFile(FileInfo file)
        {
            try
            {
                file.IsReadOnly = false;
                file.Refresh();
                file.Delete();
            }
            catch (FileNotFoundException)
            {
                log.Warn("File already deleted: " + file.FullName);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Lock a file if its length is not changing during specified interval and the file is not already in use.
        /// </summary>
        /// <param name="file">FileInfo that point to file to be locked</param>
        /// <param name="lockedFileStream">FileStream handle to the locked file</param>
        /// <param name="mSecIntervalChecBeforeLocking">Window interval to check for file length changes</param>     
        /// <param name="access">Specify how to access the locked file</param>
        /// <returns>true if success or false if the file length is changing or the file is already locked.</returns>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        public static bool Lock(FileInfo file, out FileStream lockedFileStream, int mSecIntervalChecBeforeLocking = 1000, FileAccess access = FileAccess.Read)
        {
            lockedFileStream = null;

            if (!file.Exists)
                throw new FileNotFoundException();

            if (IsFileSizeChanging(file, mSecIntervalChecBeforeLocking))
            {
                return false;
            }
            else
            {
                try
                {
                    lockedFileStream = file.Open(FileMode.Open, access, FileShare.None);
                }
                catch (Exception)
                {
                    lockedFileStream?.Close();
                    lockedFileStream?.Dispose();
                    lockedFileStream = null;
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Unlock the fileStream
        /// </summary>
        /// <param name="fileStream">FileStream handle to the locked file</param>
        public static void UnLock(FileStream fileStream)
        {
            fileStream?.Close();
            fileStream?.Dispose();
        }

        /// <summary>
        /// Check if the file size is changing during specified interval 
        /// </summary>
        /// <param name="file">FileInfo that point to file to be checked</param>
        /// <param name="mSecInterval">how many milliseconds wait before picking the file length again</param>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        /// <returns>true if the file size is chaning, false otherwise</returns>
        public static bool IsFileSizeChanging(FileInfo file, int mSecInterval = 1000)
        {
            if (!file.Exists)
                throw new FileNotFoundException();

            long lenght = file.Length;
            Thread.Sleep(mSecInterval);
            file.Refresh();
            return !((file.Length - lenght) == 0);
        }

    }
}
