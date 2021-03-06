# sslCertificateBuilder
Util for building ssl SelfSigned CA and Web Certificate for using on local machine

Generates folder cert with two certificates:

CA - Certificate Authority
Web - TLS certificate, issued by CA to be used by IIS

To setup this certificates following steps must be taken

1. After certificates are generated they need to be installed to windows certificate storage
To do that, open VisualStudio Command prompt for Administrator and run following commands

```
certmgr -add CA.cer -s -r localmachine root
```


2. Open Web.pfx in explorer, and import it to localmachine my in wizarg

3. Open IIS bindings tab, and add HTTPS binding using just added certificate


Sometimes there is a need to call host computer https website from docker
To do that, following steps must be taken:

1. Generate public key in crt format for docker
 ```
  openssl pkcs12 -in CA.pfx -clcerts -nokeys -out MYCA.crt
 ```
 
2. Add crt file during docker build phase
```
COPY <somepath>/MYCA.crt /usr/local/share/ca-certificates/MYCA.crt
```
  
```
RUN chmod 644 /usr/local/share/ca-certificates/MYCA.crt && update-ca-certificates
```

3. Run bin/sh from docker file adding dns name of host machine to host
```docker run --rm --it --entrypoint=/bin/sh ... --add-host host-machine-name:192.168.your.ip image```

4. Call your host machine by name
 ```curl https://host-machine-name```
 
 If everything is configured properly you must be able to see the web server response
 
 
 The list of articles compiled for this project is following:
 
 https://social.technet.microsoft.com/wiki/contents/articles/11510.wcf-iis-and-ssl.aspx?Sort=MostRecent&PageIndex=1
 
 https://www.codeproject.com/Articles/18601/An-easy-way-to-use-certificates-for-WCF-security
 
 https://habr.com/ru/post/497160/
 
 https://support.dnsimple.com/articles/what-is-ssl-san/
 
 https://www.sslshopper.com/article-most-common-openssl-commands.html

 https://stackoverflow.com/questions/55712535/how-to-add-a-ca-root-certificate-in-docker-image
 
 https://stackoverflow.com/questions/51323637/adding-ssl-certificates-to-docker-linux-container
 
