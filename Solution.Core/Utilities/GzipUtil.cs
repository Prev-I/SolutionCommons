using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class GZipUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Compress a specified byte array using GZIP and return it
        /// </summary>
        /// <param name="plainData">Byte array to be compressed</param>
        /// <returns>Byte array compressed in GZIP format</returns>
        public static byte[] Compress(byte[] plainData)
        {
            using (var compressedStream = new MemoryStream())
            using (var compressor = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                //Writes compressed byte to the underlying
                //stream from the specified byte array.
                compressor.Write(plainData, 0, plainData.Length);
                compressor.Close();
                return compressedStream.ToArray();
            }
        }

        /// <summary>
        /// Compress a specified stream using GZIP and return it
        /// </summary>
        /// <param name="plainStream">Stream to be compressed</param>
        /// <returns>Compressed Stream in GZIP format</returns>
        public static Stream Compress(Stream plainStream)
        {
            MemoryStream resultStream = null;

            using (var compressedStream = new MemoryStream())
            using (var compressor = new GZipStream(compressedStream, CompressionMode.Compress))
            {
                plainStream.CopyTo(compressor);
                compressor.Close();
                resultStream = new MemoryStream(compressedStream.ToArray());
            }
            return resultStream;
        }

        /// <summary>
        /// Decompress a specified GZIP byte array of data
        /// </summary>
        /// <param name="compressedData">Byte array to be decompressed</param>
        /// <returns>Byte array of resulting plain data</returns>
        public static byte[] Decompress(byte[] compressedData)
        {
            using (MemoryStream compressedStream = new MemoryStream())
            using (MemoryStream resultStream = new MemoryStream())
            using (var decompressor = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                // Write data and Reset the memory stream position to begin decompression.
                compressedStream.Write(compressedData, 0, compressedData.Length);
                compressedStream.Position = 0;
                // Decompress to resultStream
                decompressor.CopyTo(resultStream);
                decompressor.Close();
                return resultStream.ToArray();
            }
        }

        /// <summary>
        /// Decompress a specified GZIP stream of data
        /// </summary>
        /// <param name="compressedStream">Stream to be decompressed</param>
        /// <returns>Stream of resulting plain data</returns>
        public static Stream Decompress(Stream compressedStream)
        {
            MemoryStream resultStream = new MemoryStream();

            using (var decompressor = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                decompressor.CopyTo(resultStream);
                decompressor.Close();
                resultStream.Position = 0;
            }
            return resultStream;
        }

    }
}
