using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BDOL
{
    internal class DigitalSign
    {
        public static RSACryptoServiceProvider Rsa;

        public static void AssignNewKey(ref string privateKey, ref string publicKey)
        {
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));
            var cspParams = new CspParameters(1) { Flags = CspProviderFlags.UseMachineKeyStore };
            Rsa = new RSACryptoServiceProvider(2048, cspParams);
            var publicPrivateKeyXml = Rsa.ToXmlString(true);
            privateKey = publicPrivateKeyXml;
            var publicOnlyKeyXml = Rsa.ToXmlString(false);
            publicKey = publicOnlyKeyXml;
        }
        public static bool CompareRsaMethod(string textToSign, string gettedSign, string publicRsaKey)
        {
            try
            {
                var buffer = Encoding.ASCII.GetBytes(textToSign);
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.KeySize = 2048;
                    rsa.FromXmlString(publicRsaKey);
                    var signature = Convert.FromBase64String(gettedSign);
                    return rsa.VerifyData(buffer, "SHA1", signature);
                }
            }
            catch
            {
                return false;
            }
        }
        public static string Sign(string ptext, string decryptedXmlPkey)
        {
            var cryptoTransformSha1 = new SHA1CryptoServiceProvider();
            var buffer = Encoding.ASCII.GetBytes(ptext);
            buffer = cryptoTransformSha1.ComputeHash(buffer);
            RSAPKCS1SignatureFormatter rsaFormatter;
            using (var rsaCryptoServiceProvider = new RSACryptoServiceProvider())
            {
                rsaCryptoServiceProvider.FromXmlString(decryptedXmlPkey);
                rsaFormatter = new RSAPKCS1SignatureFormatter(rsaCryptoServiceProvider);
            }
            rsaFormatter.SetHashAlgorithm("SHA1");
            var signedHash = rsaFormatter.CreateSignature(buffer);
            return Convert.ToBase64String(signedHash);
        }

        public static string Xor(string text, int key)
        {
            var sb = new StringBuilder();
            foreach (var charValue in text.Select(Convert.ToInt32).Select(charValue => charValue ^ key))
                sb.Append((char)charValue);
            return sb.ToString();
        }

        private static byte[] EncryptString(byte[] clearText, byte[] key, byte[] iv)
        {
            var ms = new MemoryStream();
            var alg = Rijndael.Create();
            alg.Key = key;
            alg.IV = iv;
            var cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearText, 0, clearText.Length);
            cs.Close();
            var encryptedData = ms.ToArray();
            return encryptedData;
        }
        public static string EncryptString(string clearText, string password)
        {
            var clearBytes = Encoding.Unicode.GetBytes(clearText);
            var pdb = new PasswordDeriveBytes(password, new byte[] { 0 });
            var encryptedData = EncryptString(clearBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }
        private static byte[] DecryptString(byte[] cipherData, byte[] key, byte[] iv)
        {
            var ms = new MemoryStream();
            var alg = Rijndael.Create();
            alg.Key = key;
            alg.IV = iv;
            var cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherData, 0, cipherData.Length);
            cs.Close();
            var decryptedData = ms.ToArray();
            return decryptedData;
        }
        public static string DecryptString(string cipherText, string password)
        {
            var cipherBytes = Convert.FromBase64String(cipherText);
            var pdb = new PasswordDeriveBytes(password, new byte[] { 0 });
            var decryptedData = DecryptString(cipherBytes, pdb.GetBytes(32), pdb.GetBytes(16));
            return Encoding.Unicode.GetString(decryptedData);
        }
    }
}
