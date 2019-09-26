using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NewVPlusSales.Business.Core;

namespace NewVPlusSale.Business.Core
{
    internal class EncryptionHelper
    {
        private const string SaltChars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ-";
        private const string SpecialChars = @"()*&^%$#@!`~/?;:.>,<\|=+-_";
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once InconsistentNaming
        private static int SaltCharLength = SaltChars.Length;
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once InconsistentNaming
        private static int SpecialCharLength = SpecialChars.Length;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        // ReSharper disable once InconsistentNaming
        private static RNGCryptoServiceProvider crypt = new RNGCryptoServiceProvider();

        public static string GenerateSalt(int minChars, int maxChars)
        {
            if (minChars <= 0) throw new ArgumentOutOfRangeException("minChars");
            if (maxChars < minChars) throw new ArgumentOutOfRangeException("maxChars");

            var saltLength = GenerateTrueRandomNumber(minChars, maxChars);
            var salt = string.Empty;
            for (int i = 0; i < saltLength; i++)
            {
                salt += SaltChars.Substring(GenerateTrueRandomNumber(0, SaltCharLength), 1);
            }
            return salt;
        }

        public static int GenerateTrueRandomNumber(int min, int max)
        {
            if (min == max) return min;
            int range = max - min;
            byte[] buffer = BitConverter.GetBytes(range);
            crypt.GetBytes(buffer);
            var rndValue = (decimal)Math.Abs(BitConverter.ToInt32(buffer, 0));
            var integral = int.MaxValue / (decimal)range;
            var value = ((int)(rndValue / integral) + min);
            return value;
        }

        public static byte[] GenerateHash(string toHash, string salt)
        {
            if (string.IsNullOrEmpty(toHash)) throw new ArgumentException("toHash");
            if (string.IsNullOrEmpty(salt)) throw new ArgumentException("salt");

            var bytes = Encoding.UTF8.GetBytes(toHash + salt);
            using (var algorithm = new SHA384Managed())
            {
                return algorithm.ComputeHash(bytes);
            }
        }

        public static byte[] EncryptString(byte[] toEncrypt, byte[] encryptionKey)
        {
            if (encryptionKey == null || encryptionKey.Length == 0) throw new ArgumentException("encryptionKey");
            using (var provider = new AesCryptoServiceProvider())
            {
                provider.Key = encryptionKey;
                provider.Mode = CipherMode.CBC;
                provider.Padding = PaddingMode.PKCS7;
                using (var encryptor = provider.CreateEncryptor(provider.Key, provider.IV))
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(toEncrypt, 0, toEncrypt.Length);
                            cs.FlushFinalBlock();
                            var retVal = new byte[16 + ms.Length];
                            provider.IV.CopyTo(retVal, 0);
                            ms.ToArray().CopyTo(retVal, 16);
                            return retVal;
                        }
                    }
                }
            }
        }

        public static byte[] EncryptString(string toEncrypt, byte[] encryptionKey)
        {
            if (string.IsNullOrEmpty(toEncrypt)) throw new ArgumentException("toEncrypt");
            return EncryptString(Encoding.UTF8.GetBytes(toEncrypt), encryptionKey);
        }

        public static byte[] DecryptString(byte[] encryptedString, byte[] encryptionKey)
        {
            if (encryptedString == null || encryptedString.Length == 0) throw new ArgumentException("encryptedString");
            if (encryptionKey == null || encryptionKey.Length == 0) throw new ArgumentException("encryptionKey");
            using (var provider = new AesCryptoServiceProvider())
            {
                provider.Key = encryptionKey;
                provider.Mode = CipherMode.CBC;
                provider.Padding = PaddingMode.PKCS7;
                provider.IV = encryptedString.Take(16).ToArray();
                using (var ms = new MemoryStream(encryptedString, 16, encryptedString.Length - 16))
                {
                    using (var decryptor = provider.CreateDecryptor(provider.Key, provider.IV))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            var decrypted = new byte[encryptedString.Length];
                            // ReSharper disable once UnusedVariable
                            var byteCount = cs.Read(decrypted, 0, encryptedString.Length);
                            return decrypted;
                        }
                    }
                }
            }
        }

        public static byte[] GenerateAesKey()
        {
            using (var provider = new AesManaged())
            {
                provider.KeySize = 256;
                provider.GenerateKey();
                return provider.Key;
            }
        }

        public static string GenerateNumbers(int length)
        {
            try
            {
                var newPassword = IdGenerator.RandCodeGenMixed(length);
                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length == length)
                {
                    return newPassword;
                }

                const string numbers = "0123456789";
                var random = new Random();
                var builder = new StringBuilder(length);
                string numberAsString = "";
                for (var i = 0; i < length; i++)
                {
                    var value = numbers[random.Next(0, numbers.Length)];
                    builder.Append(value);
                }

                numberAsString = builder.ToString();
                return numberAsString;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
    }
}
