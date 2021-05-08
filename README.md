# sslCertificateBuilder
Util for building ssl SelfSigned CA and Web Certificate for using on local machine

Generates folder cert with two certificates:

CA - Certificate Authority
Web - TLS certificate, issued by CA to be used by IIS

After certificates are generated they need to be installed to windows certificate storage
To do that, open VisualStudio Command prompt for Administrator and run following commands

certmgr -add CA.cer -s -r localmachine root
