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
    public static class ZipUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static MemoryStream CompressInMemory(byte[] plainData, string fileName)
        {
            using (var plainStream = new MemoryStream(plainData))
            {
                return CompressInMemory(plainStream, fileName);
            }
        }

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

        public static FileStream CompressOnDisk(byte[] plainData, string workDirPath, string zipFileName, string fileName)
        {
            using (var plainStream = new MemoryStream(plainData))
            {
                return CompressOnDisk(plainStream, workDirPath, zipFileName, fileName);
            }
        }

        public static FileStream CompressOnDisk(Stream plainStream, string workDirPath, string zipFileName, string fileName)
        {
            string zipFilePath = Path.Combine(workDirPath, zipFileName);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry(fileName, plainStream);
                zip.Save(zipFilePath);
            }
            return new FileStream(zipFilePath, FileMode.Open);
        }


        public static MemoryStream DecompressInMemory(byte[] compressedData, string fileName)
        {
            using (MemoryStream compressedStream = new MemoryStream(compressedData))
            {
                return DecompressInMemory(compressedStream, fileName);
            }
        }

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

        public static FileStream DecompressOnDisk(byte[] compressedData, string workDirPath, string zipFileName, string fileName)
        {
            using (MemoryStream compressedStream = new MemoryStream(compressedData))
            {
                return DecompressOnDisk(compressedStream, workDirPath, zipFileName, fileName);
            }
        }

        public static FileStream DecompressOnDisk(Stream compressedStream, string workDirPath, string zipFileName, string fileName)
        {
            string zipFilePath = Path.Combine(workDirPath, zipFileName);
            string filePath = Path.Combine(workDirPath, fileName);

            using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write))
            {
                compressedStream.CopyTo(zipStream);
            }
            using (ZipFile zip = ZipFile.Read(zipFilePath))
            {
                foreach (ZipEntry file in zip)
                {
                    if (fileName == file.FileName)
                    {
                        file.Extract(workDirPath, ExtractExistingFileAction.OverwriteSilently);
                        break;
                    }
                }
            }
            return new FileStream(filePath, FileMode.Open);
        }

    }
}
