﻿using System;
using System.Security.Cryptography;


namespace GetCredentials.lib
{
    public class Crypto
    {
        public static byte[] DecryptBlob(byte[] ciphertext, byte[] key, int algCrypt = 26115, PaddingMode padding = PaddingMode.Zeros)
        {
            // decrypts a DPAPI blob using 3DES or AES

            // reference: https://docs.microsoft.com/en-us/windows/desktop/seccrypto/alg-id
            // 26115 == CALG_3DES
            // 26128 == CALG_AES_256

            if (algCrypt == 26115)
            {
                // takes a byte array of ciphertext bytes and a key array, decrypt the blob with 3DES
                TripleDESCryptoServiceProvider desCryptoProvider = new TripleDESCryptoServiceProvider();

                byte[] ivBytes = new byte[8];

                desCryptoProvider.Key = key;
                desCryptoProvider.IV = ivBytes;
                desCryptoProvider.Mode = CipherMode.CBC;
                desCryptoProvider.Padding = padding;

                byte[] plaintextBytes = desCryptoProvider.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);

                return plaintextBytes;
            }
            else if (algCrypt == 26128)
            {
                // takes a byte array of ciphertext bytes and a key array, decrypt the blob with AES256
                AesManaged aesCryptoProvider = new AesManaged();

                byte[] ivBytes = new byte[16];

                aesCryptoProvider.Key = key;
                aesCryptoProvider.IV = ivBytes;
                aesCryptoProvider.Mode = CipherMode.CBC;
                aesCryptoProvider.Padding = padding;

                byte[] plaintextBytes = aesCryptoProvider.CreateDecryptor().TransformFinalBlock(ciphertext, 0, ciphertext.Length);

                return plaintextBytes;
            }
            else
            {
                return new byte[0];
            }
        }

       
        public static byte[] AESDecrypt(byte[] key, byte[] IV, byte[] data)
        {
            // helper to AES decrypt a given blob with optional IV

            AesManaged aesCryptoProvider = new AesManaged();

            aesCryptoProvider.Key = key;
            if (IV.Length != 0)
            {
                aesCryptoProvider.IV = IV;
            }
            aesCryptoProvider.Mode = CipherMode.CBC;

            byte[] plaintextBytes = aesCryptoProvider.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);

            return plaintextBytes;
        }

        public static byte[] LSAAESDecrypt(byte[] key, byte[] data)
        {
            AesManaged aesCryptoProvider = new AesManaged();

            aesCryptoProvider.Key = key;
            aesCryptoProvider.IV = new byte[16];
            aesCryptoProvider.Mode = CipherMode.CBC;
            aesCryptoProvider.BlockSize = 128;
            aesCryptoProvider.Padding = PaddingMode.Zeros;
            ICryptoTransform transform = aesCryptoProvider.CreateDecryptor();

            int chunks = Decimal.ToInt32(Math.Ceiling((decimal)data.Length / (decimal)16));
            byte[] plaintext = new byte[chunks * 16];

            for (int i = 0; i < chunks; ++i)
            {
                int offset = i * 16;
                byte[] chunk = new byte[16];
                Array.Copy(data, offset, chunk, 0, 16);

                byte[] chunkPlaintextBytes = transform.TransformFinalBlock(chunk, 0, chunk.Length);
                Array.Copy(chunkPlaintextBytes, 0, plaintext, i * 16, 16);
            }

            return plaintext;
        }

        public static byte[] RSADecrypt(byte[] privateKey, byte[] dataToDecrypt)
        {
            // helper to RSA decrypt a given blob

            // PROV_RSA_AES == 24
            var cspParameters = new System.Security.Cryptography.CspParameters(24);

            using (var rsaProvider = new System.Security.Cryptography.RSACryptoServiceProvider(cspParameters))
            {
                try
                {
                    rsaProvider.PersistKeyInCsp = false;
                    rsaProvider.ImportCspBlob(privateKey);

                    byte[] dataToDecryptRev = new byte[256];

                    Buffer.BlockCopy(dataToDecrypt, 0, dataToDecryptRev, 0, dataToDecrypt.Length); // ... Array.Copy? naw... :(

                    Array.Reverse(dataToDecryptRev); // ... don't ask me how long it took to realize this :(

                    byte[] dec = rsaProvider.Decrypt(dataToDecryptRev, false); // no padding
                    return dec;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error decryption domain key: {0}", e.Message);
                }
                finally
                {
                    rsaProvider.PersistKeyInCsp = false;
                    rsaProvider.Clear();
                }
            }

            return new byte[0];
        }

        public static byte[] LSASHA256Hash(byte[] key, byte[] rawData)
        {
            // yay
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] buffer = new byte[key.Length + (rawData.Length * 1000)];
                Array.Copy(key, 0, buffer, 0, key.Length);
                for (int i = 0; i < 1000; ++i)
                {
                    Array.Copy(rawData, 0, buffer, key.Length + (i * rawData.Length), rawData.Length);
                }
                return sha256Hash.ComputeHash(buffer);
            }
        }
    }
}