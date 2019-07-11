using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using log4net;


namespace Solution.Tools.Utilities
{
    /// <summary>
    /// Class to handle the load and save from AES Crypt file format
    /// https://www.aescrypt.com/
    /// REQUIRE: SharpAESCrypt
    /// </summary>
    public static class AesCryptUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Encrypt input data to AES Crypt format using the provided password
        /// </summary>
        /// <param name="password">String used to encrypt data</param>
        /// <param name="inputData">Byte array of the plain data</param>
        /// <returns>Encrypted byte array</returns>
        public static byte[] EncryptFile(string password, byte[] inputData)
        {
            MemoryStream result = null;

            try
            {
                using (MemoryStream streamInput = new MemoryStream(inputData))
                {
                    result = new MemoryStream();
                    SharpAESCrypt.SharpAESCrypt.Encrypt(password, streamInput, result);
                }
                return result.ToArray();
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
            finally
            {
                result?.Dispose();
            }
        }

        /// <summary>
        /// Encrypt input data to AES Crypt format using the provided password and save it to disk
        /// </summary>
        /// <param name="password">String used to encrypt data</param>
        /// <param name="inputFile">Full path of plain file</param>
        /// <param name="outputFile">Full path where to create encrypted file</param>
        public static void EncryptFile(string password, string inputFile, string outputFile)
        {
            try
            {
                SharpAESCrypt.SharpAESCrypt.Encrypt(password, inputFile, outputFile);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }

        }

        /// <summary>
        /// Encrypt input data to AES Crypt format using the provided password and return it as stream
        /// </summary>
        /// <param name="password">String used to encrypt data</param>
        /// <param name="inputFileStream">Stream of plain content</param>
        /// <returns>Stream of encrypted data</returns>
        public static Stream EncryptFile(string password, Stream inputFileStream)
        {
            MemoryStream resultStream = new MemoryStream();

            try
            {
                SharpAESCrypt.SharpAESCrypt.Encrypt(password, inputFileStream, resultStream);
                return resultStream;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Decrypt input data in AES Crypt format using the provided password
        /// </summary>
        /// <param name="password">String used to decrypt data</param>
        /// <param name="inputData">Byte array of the encrypted file</param>
        /// <returns>Decrypted byte array</returns>
        public static byte[] DecryptFile(string password, byte[] inputData)
        {
            MemoryStream result = null;

            try
            {
                using (MemoryStream streamInput = new MemoryStream(inputData))
                {
                    result = new MemoryStream();
                    SharpAESCrypt.SharpAESCrypt.Decrypt(password, streamInput, result);
                }
                return result.ToArray();
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
            finally
            {
                result?.Dispose();
            }
        }

        /// <summary>
        /// Decrypt input data in AES Crypt format using the provided password and save it on disk
        /// </summary>
        /// <param name="password">String used to decrypt data</param>
        /// <param name="inputFile">Full path of encrypted file</param>
        /// <param name="outputFile">Full path where to create decrypted file</param>
        public static void DecryptFile(string password, string inputFile, string outputFile)
        {
            try
            {
                SharpAESCrypt.SharpAESCrypt.Decrypt(password, inputFile, outputFile);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Decrypt input data in AES Crypt format using the provided password and return as stream
        /// </summary>
        /// <param name="password">String used to decrypt data</param>
        /// <param name="inputFileStream">Stream of the encrypted content</param>
        /// <returns>Stream of decrypted data</returns>
        public static Stream DecryptFile(string password, Stream inputFileStream)
        {
            MemoryStream resultStream = new MemoryStream();

            try
            {
                SharpAESCrypt.SharpAESCrypt.Decrypt(password, inputFileStream, resultStream);
                return resultStream;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

    }
}
