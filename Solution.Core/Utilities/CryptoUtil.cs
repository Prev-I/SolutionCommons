using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// Utility to encrypt/decrypt using AES and generate hash using SHA256
    /// </summary>
    public static class CryptoUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const int SizeOfBuffer = 8192;

        /// <summary>
        /// Enum to indicate hash function 
        /// </summary>
        public enum HashFunction
        {
            /// <summary>
            /// Use MD5 function
            /// </summary>
            MD5 = 0,
            /// <summary>
            /// Use SHA256 function
            /// </summary>
            SHA256 = 1
        }


        /// <summary>
        /// Function to encrypt a string using rsa crypto
        /// </summary>
        /// <param name="inputString">Input plain text to be encrypted</param>
        /// <param name="dwKeySize">RSA key size in bit, 2048</param>
        /// <param name="xmlString">xml containing the public/private key</param>
        /// <returns>Return the encrypted string encoded in Base64</returns>
        public static string EncryptString(string inputString, int dwKeySize, string xmlString)
        {
            try
            {
                // TODO: Add Proper Exception Handlers
                RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
                rsaCryptoServiceProvider.FromXmlString(xmlString);
                int keySize = dwKeySize / 8;
                byte[] bytes = Encoding.UTF32.GetBytes(inputString);
                // The hash function in use by the .NET RSACryptoServiceProvider here 
                // is SHA1
                // int maxLength = ( keySize ) - 2 - 
                //              ( 2 * SHA1.Create().ComputeHash( rawBytes ).Length );
                int maxLength = keySize - 42;
                int dataLength = bytes.Length;
                int iterations = dataLength / maxLength;
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i <= iterations; i++)
                {
                    byte[] tempBytes = new byte[(dataLength - maxLength * i > maxLength) ? maxLength : dataLength - maxLength * i];
                    Buffer.BlockCopy(bytes, maxLength * i, tempBytes, 0, tempBytes.Length);
                    byte[] encryptedBytes = rsaCryptoServiceProvider.Encrypt(tempBytes, true);
                    // Be aware the RSACryptoServiceProvider reverses the order of 
                    // encrypted bytes. It does this after encryption and before 
                    // decryption. If you do not require compatibility with Microsoft 
                    // Cryptographic API (CAPI) and/or other vendors. Comment out the 
                    // next line and the corresponding one in the DecryptString function.
                    Array.Reverse(encryptedBytes);
                    // Why convert to base 64?
                    // Because it is the largest power-of-two base printable using only 
                    // ASCII characters
                    stringBuilder.Append(Convert.ToBase64String(encryptedBytes));
                }
                return stringBuilder.ToString();
            }
            catch (Exception e)
            {
                 log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Function to decrypt a string using rsa crypto
        /// </summary>
        /// <param name="inputString">Input plain text to be encrypted</param>
        /// <param name="dwKeySize">RSA key size in bit, 2048</param>
        /// <param name="xmlString">xml containing the public/private key</param>
        /// <returns>Return the plain string</returns>
        public static string DecryptString(string inputString, int dwKeySize, string xmlString)
        {
            try
            {
                // TODO: Add Proper Exception Handlers
                RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
                rsaCryptoServiceProvider.FromXmlString(xmlString);
                int base64BlockSize = ((dwKeySize / 8) % 3 != 0) ? (((dwKeySize / 8) / 3) * 4) + 4 : ((dwKeySize / 8) / 3) * 4;
                int iterations = inputString.Length / base64BlockSize;
                ArrayList arrayList = new ArrayList();

                for (int i = 0; i < iterations; i++)
                {
                    byte[] encryptedBytes = Convert.FromBase64String(inputString.Substring(base64BlockSize * i, base64BlockSize));
                    // Be aware the RSACryptoServiceProvider reverses the order of 
                    // encrypted bytes after encryption and before decryption.
                    // If you do not require compatibility with Microsoft Cryptographic 
                    // API (CAPI) and/or other vendors.
                    // Comment out the next line and the corresponding one in the 
                    // EncryptString function.
                    Array.Reverse(encryptedBytes);
                    arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(encryptedBytes, true));
                }
                return Encoding.UTF32.GetString(arrayList.ToArray(Type.GetType("System.Byte")) as byte[]);
            }
            catch (Exception e)
            {
                 log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Function to encrypt a file using aes crypto
        /// </summary>
        /// <param name="inputFile">Input file to be encrypted</param>
        /// <param name="outputFile">Encrypted output file to be created</param>
        /// <param name="password">Password used to derivate symmetric key</param>
        /// <param name="salt">Salt used in combination with password to create the key</param>
        /// <returns></returns>
        public static void EncryptFile(FileInfo inputFile, FileInfo outputFile, string password, string salt)
        {
            try
            {
                // Essentially, if you want to use RijndaelManaged as AES you need to make sure that:
                // 1.The block size is set to 128 bits
                // 2.You are not using CFB mode, or if you are the feedback size is also 128 bits

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentNullException("password");
                if (!inputFile.Exists)
                    throw new FileNotFoundException(inputFile.Name);

                using (var algorithm = new AesManaged { KeySize = 256, BlockSize = 128 })
                using (var key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(salt)))
                {
                    algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
                    algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

                    using (var inputStream = new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read))
                    using (var outputStream = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write))
                    using (var encryptedStream = new CryptoStream(outputStream, algorithm.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(encryptedStream, SizeOfBuffer);
                    }
                }
            }
            catch (Exception e)
            {
                 log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Function to decrypt a file using aes crypto
        /// </summary>
        /// <param name="inputFile">Input file to be decripted</param>
        /// <param name="outputFile">Decripted output file to be created</param>
        /// <param name="password">Password used to derivate symmetric key</param>
        /// <param name="salt">Salt used in combination with password to create the key</param>
        /// <returns></returns>
        public static void DecryptFile(FileInfo inputFile, FileInfo outputFile, string password, string salt)
        {
            try
            {
                // Essentially, if you want to use RijndaelManaged as AES you need to make sure that:
                // 1.The block size is set to 128 bits
                // 2.You are not using CFB mode, or if you are the feedback size is also 128 bits

                if (string.IsNullOrEmpty(password))
                    throw new ArgumentNullException("password");
                if (!inputFile.Exists)
                    throw new FileNotFoundException(inputFile.Name);

                using (var algorithm = new AesManaged { KeySize = 256, BlockSize = 128 })
                using (var key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes(salt)))
                {
                    algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
                    algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

                    using (var inputStream = new FileStream(inputFile.FullName, FileMode.Open, FileAccess.Read))
                    using (var outputStream = new FileStream(outputFile.FullName, FileMode.Create, FileAccess.Write))
                    using (var decryptedStream = new CryptoStream(outputStream, algorithm.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        inputStream.CopyTo(decryptedStream, SizeOfBuffer);
                    }
                }
            }
            catch (Exception e)
            {
                 log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Function to hash a file using SHA256 algoritm
        /// </summary>
        /// <param name="inputFile">Input file to be hashed</param>
        /// <param name="function">Specify with function to use in generating hash</param>
        /// <returns>Return the file hash encoded in Base64</returns>
        public static string HashFile(FileInfo inputFile, HashFunction function = HashFunction.SHA256)
        {
            try
            {
                byte[] hashValue = new byte[0];

                if (!inputFile.Exists)
                    throw new FileNotFoundException(inputFile.Name);

                if(function == HashFunction.SHA256)
                {
                    using (var sha256Hash = SHA256.Create())
                    {
                        hashValue = sha256Hash.ComputeHash(inputFile.OpenRead());
                    }
                }
                else if (function == HashFunction.MD5)
                {
                    using (var md5Hash = MD5.Create())
                    {
                        hashValue = md5Hash.ComputeHash(inputFile.OpenRead());
                    }
                }
                return Convert.ToBase64String(hashValue);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

    }
}
