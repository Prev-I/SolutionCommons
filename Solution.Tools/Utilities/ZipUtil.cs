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
            MemoryStream resultStream = new MemoryStream();

            using (var plainStream = new MemoryStream(plainData))
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

        public static FileStream CompressOnDisk(byte[] plainData, string workFolderPath, string zipFileName, string fileName)
        {
            string zipFilePath = Path.Combine(workFolderPath, zipFileName);
            string filePath = Path.Combine(workFolderPath, fileName);

            using (FileStream plainStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                plainStream.Write(plainData, 0, plainData.Length);
                plainStream.Close();
            }
            using (ZipFile zip = new ZipFile())
            {
                zip.AddFile(filePath, "");
                zip.Save(zipFilePath);
            }
            return new FileStream(zipFilePath, FileMode.Open);
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

        public static FileStream CompressOnDisk(FileStream plainStream, string workFolderPath, string zipFileName, string fileName)
        {
            string zipFilePath = Path.Combine(workFolderPath, zipFileName);

            using (ZipFile zip = new ZipFile())
            {
                zip.AddEntry(fileName, plainStream);
                zip.Save(zipFilePath);
            }
            return new FileStream(zipFilePath, FileMode.Open);
        }


        public static MemoryStream DecompressInMemory(byte[] compressedData, string fileName)
        {
            MemoryStream resultStream = new MemoryStream();

            using (MemoryStream compressedStream = new MemoryStream(compressedData))
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

        public static FileStream DecompressOnDisk(byte[] compressedData, string workFolderPath, string zipFileName, string fileName)
        {
            string zipFilePath = Path.Combine(workFolderPath, zipFileName);
            string filePath = Path.Combine(workFolderPath, fileName);

            using (FileStream zipStream = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write))
            {
                zipStream.Write(compressedData, 0, compressedData.Length);
                zipStream.Close();
            }
            using (ZipFile zip = ZipFile.Read(zipFilePath))
            {
                foreach (ZipEntry file in zip)
                {
                    if (fileName == file.FileName)
                    {
                        file.Extract(workFolderPath, ExtractExistingFileAction.OverwriteSilently);
                        break;
                    }
                }
            }
            return new FileStream(filePath, FileMode.Open);
        }

    }
}
