using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

using log4net;
using SevenZip;

using Solution.Core.Utilities;


namespace Solution.Tools.Utilities
{
    /// <summary>
    /// Class to handle compression and decompression of archives supported by 7zip 
    /// REQUIRE: SevenZip
    /// REQUIRE: /Libraries/SevenZip/ in the root directory to load 7z.dll for x86 or x64 arch
    /// </summary>
    public static class SevenZipUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Compress al inputFiles to destination sevenZip archive
        /// </summary>
        /// <param name="inputFiles">Array of full path of input files</param>
        /// <param name="destinationFile">Path of where to create 7z archive</param>
        public static void Compress(string[] inputFiles, string destinationFile)
        {
            SevenZipWorking wrk = new SevenZipWorking();

            try
            {
                wrk.CompressFiles(inputFiles, destinationFile);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
            finally
            {
                wrk = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// Decompress file contained in inputFile inside a destinationDir
        /// </summary>
        /// <param name="inputFile">Path to 7z archive</param>
        /// <param name="destinationDir">Directory where to put the archive content</param>
        /// <returns>Return true if extraction is succeded, false otherwise</returns>
        public static bool Decompress(string inputFile, string destinationDir)
        {
            SevenZipWorking wrk = new SevenZipWorking();

            try
            {
                return wrk.Extract(inputFile, destinationDir);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
            finally
            {
                wrk = null;
                GC.Collect();
                //wait max 1 sec for file to be released
                int i = 0;
                while (i < 10)
                {
                    FileStream fs = null;
                    try
                    {
                        fs = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.None);
                        fs.Close();
                        i = 10;
                    }
                    catch (Exception)
                    {
                        fs?.Close();
                        Thread.Sleep(100);
                        i++;
                    }
                }
            }
        }

    }

    internal class SevenZipWorking
    {

        public SevenZipWorking()
        {
            // Toggle between the x86 and x64 bit dll
            string dllPath = Path.Combine(FileUtil.GetLocalExecutionFullPah(), "Libraries\\SevenZip", Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
            SevenZipBase.SetLibraryPath(dllPath);
        }

        public bool Extract(string inputFile, string destinationDir)
        {
            SevenZipExtractor extractor = new SevenZipExtractor(inputFile);
            extractor.FileExists += new EventHandler<FileOverwriteEventArgs>(ZipExtractor_FileExists);
            if (extractor.Check())
            {
                extractor.ExtractArchive(destinationDir);
                return true;
            }
            else return false;
        }

        public void ZipExtractor_FileExists(object sender, FileOverwriteEventArgs args)
        {
            FileInfo fi = new FileInfo(args.FileName);
            while (File.Exists(args.FileName))
            {
                args.FileName = Path.Combine(fi.Directory.FullName, Path.GetFileNameWithoutExtension(fi.Name) + "_" + DateTime.Now.Ticks + fi.Extension);
            }
        }

        public void CompressFiles(string[] inputFiles, string outputfile)
        {
            if (File.Exists(outputfile))
                File.Delete(outputfile);

            SevenZipCompressor szc = new SevenZipCompressor
            {
                CompressionLevel = CompressionLevel.Normal,
                ArchiveFormat = OutArchiveFormat.SevenZip,
                CompressionMethod = CompressionMethod.Lzma2,
                CompressionMode = CompressionMode.Create,
                DirectoryStructure = true,
                EncryptHeaders = true,
                DefaultItemName = outputfile
            };
            using (FileStream archive = new FileStream(outputfile, FileMode.Create, FileAccess.ReadWrite))
            {
                szc.CompressFiles(archive, inputFiles);
            }
        }

    }

}
