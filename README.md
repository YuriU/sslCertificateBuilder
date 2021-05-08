# sslCertificateBuilder
Util for building ssl SelfSigned CA and Web Certificate for using on local machine

Generates folder cert with two certificates:

CA - Certificate Authority
Web - TLS certificate, issued by CA to be used by IIS

To setup this certificates following steps must be taken

1. After certificates are generated they need to be installed to windows certificate storage
To do that, open VisualStudio Command prompt for Administrator and run following commands

<code>
certmgr -add CA.cer -s -r localmachine root
</code>


2. Open Web.pfx in explorer, and import it to localmachine my in wizarg

3. Open IIS bindings tab, and add HTTPS binding using just added certificate
