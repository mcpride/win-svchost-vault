using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace VaultService.Security.Cryptography
{

    internal static class Pki
    {
        public static (byte[]?, string?) CreateEnciphermentCertificate(string subject, string? password)
        {
            ArgumentNullException.ThrowIfNull(subject);
            var req = CreateEnciphermentCertificateRequest(subject);
            var utcNowOffset = DateTimeOffset.UtcNow;
            var notBefore = utcNowOffset.AddMinutes(-1);
            var notAfter = utcNowOffset.AddYears(10).AddDays(15);

            var cert = req.CreateSelfSigned(notBefore, notAfter);
            var thumb = cert.Thumbprint;

            var col = new X509Certificate2Collection { cert };

            return (col.Export(X509ContentType.Pfx, password), thumb);

        }

        private static CertificateRequest CreateEnciphermentCertificateRequest(string subject)
        {
            var rsa = RSA.Create(4096);
            var req = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            req.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, true, 0, false));
            req.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment, false));
            req.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(req.PublicKey, false));
            req.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection
            {
                new Oid("1.3.6.1.4.1.311.80.1")
            }, false));

            return req;
        }
    }
}
