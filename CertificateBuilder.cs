using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace sslCertificateBuilder
{
    public class CertificateBuilder
    {
        public static X509Certificate2 CreateCertificateAuthority(
            string subject,
            int newRsaKeySize
        )
        {
            X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={subject}");

            using (RSA rsa = RSA.Create(newRsaKeySize))
            {
                var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(true, false, 0, false));
                
                var certificate = request
                    .CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));

                return certificate;
            }
        }
        
        public static X509Certificate2 CreateWebSiteTlsCertificate(
                                            X509Certificate2 signingCert,
                                            int newRsaKeySize,
                                            IEnumerable<string> sanDnsEntries)
        {
            var sanBuilder = new SubjectAlternativeNameBuilder();
            string primaryDnsName = null;


            foreach (string dnsEntry in sanDnsEntries)
            {
                // Let just use the first one as the subject.
                primaryDnsName = primaryDnsName ?? dnsEntry;
                
                sanBuilder.AddDnsName(dnsEntry);
            }
            
            using (RSA rsa = RSA.Create(newRsaKeySize))
            {
                var certRequest = new CertificateRequest(
                    $"CN={primaryDnsName}, O=Et OU=Cetera",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);
                
                // Explicitly not a CA.
                certRequest.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));
                
                certRequest.CertificateExtensions.Add(
                    new X509KeyUsageExtension(
                        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                        true));
                
                // TLS Server EKU
                certRequest.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection
                        {
                            new Oid("1.3.6.1.5.5.7.3.1"),
                        },
                        false));

                // Add the SubjectAlternativeName extension
                certRequest.CertificateExtensions.Add(sanBuilder.Build());
                
                // Serial number.
                // It needs to be unique per issuer.
                // CA/Browser forum rules say 64 or more bits must come from a CSPRNG.
                // RFC 3280 says not to use more than 20 bytes.
                // Let use 16 (two C# 'long's)
                byte[] serialNumber = new byte[16];
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(serialNumber);
                }
                
                // If you care about monotonicity (and believe your clock is monotonic enough):
                {
                    long ticks = DateTime.UtcNow.Ticks;
                    byte[] tickBytes = BitConverter.GetBytes(ticks);
                    
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(tickBytes);
                    }
                    
                    Buffer.BlockCopy(tickBytes, 0, serialNumber, 0, tickBytes.Length);
                }

                DateTimeOffset now = DateTimeOffset.UtcNow;
                var cert = certRequest.Create(
                    signingCert,
                    now,
                    now.AddDays(90),
                    serialNumber);
                
                // Adding private key to certificate
                var exportCert = 
                    new X509Certificate2(cert.Export(X509ContentType.Cert), (string)null, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet)
                        .CopyWithPrivateKey(rsa);

                return exportCert;
            }
        }
    }
}