using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using log4net;
using Ionic.Zip;


namespace Solution.Tools.Utilities
{
    /// <summary>
    /// Utility class to compress/decompress data in zip format
    /// REQUIRE: Ionic.Zip
    /// </summary>
    public static class ZipUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Compress a byte[] of data from a single file to a ZIP stored inside a memory stream
        /// </summary>
        /// <param name="plainData"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static MemoryStream CompressInMemory(byte[] plainData, string fileName)
        {
            using (var plainStream = new MemoryStream(plainData))
            {
                return CompressInMemory(plainStream, fileName);
            }
        }

        /// <summary>
        /// Compress a Stream of data from a single file to a ZIP inside a memory stream
        /// </summary>
        /// <param name="plainStream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static MemoryStream CompressInMemory(Stream plainStream, string fileName)
        {
            MemoryStream resultStream = new MemoryStream();

            using (var compressedStream = new MemoryStream())
            using (var zipStream = new ZipOutputStream(compressedStream))
            {
                ZipEntry file = zipStream.PutNextEntry(fileName);
                file.SetEntryTimes(DateTime.Now, DateTime.Now, DateTime.Now);
                plainStream.CopyTo(zipStream);
                zipStream.Close();
                resultStream = new MemoryStream(compressedStream.ToArray());
            }
            return resultStream;
        }

        /// <summary>
        /// Compress a byte[] of data from a single file to a ZIP file on disk
        /// </summary>
        /// <param name="plainData"></param>
        /// <param name="zipFile"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream CompressOnDisk(byte[] plainData, string zipFile, string fileName)
        {
            using (var plainStream = new MemoryStream(plainData))
            {
                return CompressOnDisk(plainStream, zipFile, fileName);
            }
        }

        /// <summary>
        /// Compress a Stream of data from a single file to a ZIP file on disk
        /// </summary>
        /// <param name="plainStream"></param>
        /// <param name="zipFile"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream CompressOnDisk(Stream plainStream, string zipFile, string fileName)
        {
            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry(fileName, plainStream);
                zip.Save(zipFile);
            }
            return new FileStream(zipFile, FileMode.Open);
        }

        /// <summary>
        /// Decompress a single file from a zip archive passed as byte[]
        /// </summary>
        /// <param name="compressedData"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static MemoryStream DecompressInMemory(byte[] compressedData, string fileName)
        {
            using (MemoryStream compressedStream = new MemoryStream(compressedData))
            {
                return DecompressInMemory(compressedStream, fileName);
            }
        }

        /// <summary>
        /// Decompress a single file from a zip archive passed as Stream
        /// </summary>
        /// <param name="compressedStream"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static MemoryStream DecompressInMemory(Stream compressedStream, string fileName)
        {
            MemoryStream resultStream = new MemoryStream();

            using (var zipStream = new ZipInputStream(compressedStream))
            {
                ZipEntry file = zipStream.GetNextEntry();

                while (file != null)
                {
                    if (fileName == file.FileName)
                    {
                        zipStream.CopyTo(resultStream);
                        resultStream.Position = 0;
                        break;
                    }
                }
            }
            return resultStream;
        }

        /// <summary>
        /// Decompress a single file from a zip archive passed as byte[] saving it on disk
        /// </summary>
        /// <param name="compressedData"></param>
        /// <param name="destinationDir"></param>
        /// <param name="zipFile"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream DecompressOnDisk(byte[] compressedData, string destinationDir, string zipFile, string fileName)
        {
            using (MemoryStream compressedStream = new MemoryStream(compressedData))
            {
                return DecompressOnDisk(compressedStream, destinationDir, zipFile, fileName);
            }
        }

        /// <summary>
        /// Decompress a single file from a zip archive passed as Stream saving it on disk
        /// </summary>
        /// <param name="compressedStream"></param>
        /// <param name="destinationDir"></param>
        /// <param name="zipFile"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileStream DecompressOnDisk(Stream compressedStream, string destinationDir, string zipFile, string fileName)
        {
            string filePath = Path.Combine(destinationDir, fileName);

            using (FileStream zipStream = new FileStream(zipFile, FileMode.Create, FileAccess.Write))
            {
                compressedStream.CopyTo(zipStream);
            }
            using (ZipFile zip = ZipFile.Read(zipFile))
            {
                foreach (ZipEntry file in zip)
                {
                    if (fileName == file.FileName)
                    {
                        file.Extract(destinationDir, ExtractExistingFileAction.OverwriteSilently);
                        break;
                    }
                }
            }
            return new FileStream(filePath, FileMode.Open);
        }

    }
}
