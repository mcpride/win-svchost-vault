using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Text;

namespace VaultService.Security.Cryptography
{
    internal static class Cipher
    {
        // ReSharper disable once InconsistentNaming
        private const string OID_NIST_AES256_CBC = "2.16.840.1.101.3.4.1.42";
        public static string EncryptText(
            string plainText,
            string thumbprint,
            StoreName storeName = StoreName.My,
            StoreLocation storeLocation = StoreLocation.CurrentUser)
        {
            using (var store = new X509Store(storeName, storeLocation))
            {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);
                if (certificates.Count == 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(thumbprint),
                        $"Unable to locate a certificate with the specified thumbprint: {thumbprint}");
                }

                using (var certificate = certificates[0])
                {
                    return EncryptText(plainText, certificate);
                }
            }
        }

        public static string EncryptText(string plainText, X509Certificate2 certificate)
        {
            var content = new ContentInfo(Encoding.Unicode.GetBytes(plainText));
            var envelope = new EnvelopedCms(content, new AlgorithmIdentifier(new Oid(OID_NIST_AES256_CBC)));
            var recipient = new CmsRecipient(certificate);
            envelope.Encrypt(recipient);
            return Convert.ToBase64String(envelope.Encode());
        }

        public static string DecryptText(string cipherText)
        {

            var content = Convert.FromBase64String(cipherText);
            var envelope = new EnvelopedCms();
            envelope.Decode(content);
            envelope.Decrypt();
            return Encoding.Unicode.GetString(envelope.ContentInfo.Content);
        }
    }
}
