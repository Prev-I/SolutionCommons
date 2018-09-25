using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using log4net;


namespace Solution.Core.Utilities
{
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
        public static DirectoryInfo InitDirectory(DirectoryInfo target, bool overwrite = false)
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
                return target;
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

    }
}
