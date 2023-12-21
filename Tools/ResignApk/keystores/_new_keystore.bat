
:: Gen PKCS12  KeyStore
keytool -genkey -alias android -keyalg RSA -keysize 2048 -validity 36500 -keystore test_pkcs12.keystore
:: change PKCS12 alias password
keytool -keypasswd -keystore test_pkcs12.keystore -alias android

:: Convert PKCS12.keystore -> jks.keystore
keytool -importkeystore -srckeystore test_pkcs12.keystore -srcstoretype PKCS12 -deststoretype JKS -destkeystore test_jks.keystore
:: change JKS alias password
keytool -keypasswd -keystore test_jks.keystore -alias android