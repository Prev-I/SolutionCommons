using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using log4net;


namespace Solution.Tools.Utilities
{
    public static class AesCryptUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


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
                throw new Exception("Escalated exception", e);
            }
            finally
            {
                result?.Dispose();
            }
        }

        public static void DecryptFile(string password, string inputFile, string outputFile)
        {
            try
            {
                SharpAESCrypt.SharpAESCrypt.Decrypt(password, inputFile, outputFile);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

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
                throw new Exception("Escalated exception", e);
            }
        }


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
                throw new Exception("Escalated exception", e);
            }
            finally
            {
                result?.Dispose();
            }
        }

        public static void EncryptFile(string password, string inputFile, string outputFile)
        {
            try
            {
                SharpAESCrypt.SharpAESCrypt.Encrypt(password, inputFile, outputFile);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }

        }

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
                throw new Exception("Escalated exception", e);
            }
        }

    }
}
