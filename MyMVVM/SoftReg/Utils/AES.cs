using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MyMVVM.SoftReg.Utils
{
    public class AES
    {
        private const string DEFAULT_CODING = "UTF-8";

        /**  
         * 加密  
         * @param content 待加密的字符串  
         * @param key 加密密钥  
         */
        public static string Encrypt(string content, string key)
        {
            byte[] input = Encoding.GetEncoding(DEFAULT_CODING).GetBytes(content);

            using (MD5 md5 = MD5.Create())
            {
                byte[] keyBytes = md5.ComputeHash(Encoding.GetEncoding(DEFAULT_CODING).GetBytes(key));
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = keyBytes;
                    aesAlg.Mode = CipherMode.ECB;
                    aesAlg.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(input, 0, input.Length);
                            csEncrypt.Close();
                        }
                        return ByteArrayToHex(msEncrypt.ToArray());
                    }
                }
            }
        }

        /**  
         * 解密  
         * @param encrypted 加密后的十六进制字符串  
         * @param key 解密密钥  
         */
        public static string Decrypt(string encrypted, string key)
        {
            byte[] keyBytes = MD5.Create().ComputeHash(Encoding.GetEncoding(DEFAULT_CODING).GetBytes(key));
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = keyBytes;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                byte[] encryptedBytes = HexToByteArray(encrypted);
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }

        // 字节数组转十六进制字符串  
        private static string ByteArrayToHex(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        // 十六进制字符串转字节数组  
        private static byte[] HexToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}

