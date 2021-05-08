using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace sslCertificateBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            // The certificate is generated for current machine, so request https://machinename would be always successfull
            var machineName = Environment.MachineName; 
            
            // Certificate authority which generates following certificates
            var certificateAuthority = CertificateBuilder.CreateCertificateAuthority($"{machineName}-CA-Generated", 2048 * 2);
            certificateAuthority.FriendlyName = $"{machineName} Certificate Authority created by sslCertificateBuilder";
            
            
            // Tls certificate to install to IIS binding
            var webCertificate = CertificateBuilder.CreateWebSiteTlsCertificate(certificateAuthority, 2048, new[]
            {
                machineName // SAN, required for browsers to verify certificate
            });
            
            webCertificate.FriendlyName = $"{machineName} TLS Certificate created by sslCertificateBuilder";

            Export(certificateAuthority, "CA");
            Console.WriteLine("CA certificate exported.");
            Export(webCertificate, "Web");
            Console.WriteLine("Web certificate exported.");
        }
        
        private static void Export(X509Certificate2 cert, string name)
        {
            if (!Directory.Exists("cert"))
            {
                Directory.CreateDirectory("cert");
            }
            
            var publicKey = cert.Export(X509ContentType.Cert);
            File.WriteAllBytes($@"cert\{name}.cer", publicKey);
            var privateKey = cert.Export(X509ContentType.Pkcs12);
            File.WriteAllBytes($@"cert\{name}.pfx", privateKey);
        }
    }
}
