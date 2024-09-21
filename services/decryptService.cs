using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Text.Json;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

public class decryptService
{

    IConfiguration appsettings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
    
    //private readonly string SERVER_PRIVATE_KEY_UTI;



    
    public string decryptWithPrivateKey(string cypherText, string privateKey)
    {

       try{ RSACryptoServiceProvider RSAprivateKey = ImportPrivateKey(privateKey);
        //first, get our bytes back from the base64 string ...
        var bytesCypherText = Convert.FromBase64String(cypherText);

        //we want to decrypt, therefore we need a csp and load our private key 

        //decrypt and strip pkcs#1.5 padding
        var bytesPlainTextData = RSAprivateKey.Decrypt(bytesCypherText, false);

        //get our original plainText back...
        var plainTextData = Encoding.UTF8.GetString(bytesPlainTextData);
        // var plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);
        return plainTextData;
        }
        catch(Exception ex)
        {

        }
        return null;

    }
    public static RSACryptoServiceProvider ImportPrivateKey(string pem)
    {
        PemReader pr = new PemReader(new StringReader(pem));
        AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
        RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

        RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
        csp.ImportParameters(rsaParams);
        return csp;
    }

    public string getHash(string stringToCompute)
    {
        SHA256 mySHA256 = SHA256.Create();
        // convert string to byte array
        byte[] strToComputeBytes=Encoding.UTF8.GetBytes(stringToCompute);
        byte[] byteHash = mySHA256.ComputeHash(strToComputeBytes);
        return BitConverter.ToString(byteHash).Replace("-","");
    }

    // public string AESDecrypt(string base64Key, string base64Ciphertext)
    // {
    //     // convert from base64 to raw bytes spans
    //     var encryptedData = Convert.FromBase64String(base64Ciphertext).AsSpan();
    //     var key = Convert.FromBase64String(base64Key).AsSpan();

    //     var tagSizeBytes = 16; // 128 bit encryption / 8 bit = 16 bytes
    //     var ivSizeBytes = 12; // 12 bytes iv
    
    //     // ciphertext size is whole data - iv - tag
    //     var cipherSize = encryptedData.Length - tagSizeBytes - ivSizeBytes;

    //     // extract iv (nonce) 12 bytes prefix
    //     var iv = encryptedData.Slice(0, ivSizeBytes);
    
    //     // followed by the real ciphertext
    //     var cipherBytes = encryptedData.Slice(ivSizeBytes, cipherSize);

    //     // followed by the tag (trailer)
    //     var tagStart = ivSizeBytes + cipherSize;
    //     var tag = encryptedData.Slice(tagStart);

    //     // now that we have all the parts, the decryption
    //     Span<byte> plainBytes = cipherSize < 1024
    //         ? stackalloc byte[cipherSize]
    //         : new byte[cipherSize];

    //     //var ae= new AesCng();    
    //     var aes = new AesGcm(key);
        

    //     aes.Decrypt(iv, cipherBytes, tag, plainBytes);
    //     return Encoding.UTF8.GetString(plainBytes);
    // }
        public string AESDecrypt(string base64Key, string base64Ciphertext)
{
    var fullCipher = Convert.FromBase64String(base64Ciphertext);

    var iv = new byte[16];
    var cipher = new byte[fullCipher.Length - iv.Length];

    Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
    Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

    var key = Encoding.UTF8.GetBytes(base64Key);

    using (var aesAlg = Aes.Create())
    {
        aesAlg.Key = key;
        aesAlg.IV = iv;
        aesAlg.Mode = CipherMode.CBC;
        aesAlg.Padding = PaddingMode.PKCS7;

        using (var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV))
        {
            using (var msDecrypt = new MemoryStream(cipher))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}


    // this function is called only once copy the publoc and private key from the console to be shared
    public  void GeneratePrivatePublicKeyPair() 
    {
        var name = "test";
        //var privateKeyXmlFile = name + "_priv.xml";
        //var publicKeyXmlFile = name + "_pub.xml";
        //var publicKeyFile = name + ".pub";

        using var provider = new RSACryptoServiceProvider(1024);
        Console.Write(provider.ToXmlString(true)); // private key
        Console.Write(provider.ToXmlString(false)); // public key
        //var x = provider.ImportRSAPrivateKey();
        //provider.ToString(true)
        //File.WriteAllText(privateKeyXmlFile, provider.ToXmlString(true));
        //File.WriteAllText(publicKeyXmlFile, provider.ToXmlString(false));
        //using var publicKeyWriter = File.CreateText(publicKeyFile);
        //ExportPublicKey(provider, publicKeyWriter);
        var x=1;
    }

    public void testCrypto()
    {
      //lets take a new CSP with a new 2048 bit rsa key pair
      var csp = new RSACryptoServiceProvider(2048);

      //how to get the private key
      var privKey = csp.ExportParameters(true);

      //and the public key ...
      var pubKey = csp.ExportParameters(false);

      //converting the public key into a string representation
      string pubKeyString;
      {
        //we need some buffer
        var sw = new System.IO.StringWriter();
        //we need a serializer
        var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        //serialize the key into the stream
        xs.Serialize(sw, pubKey);
        //get the string from the stream
        pubKeyString = sw.ToString();
      }
      //csp.ExportRSAPublicKey()
   
      //converting it back
      {
        //get a stream from the string
        var sr = new System.IO.StringReader(pubKeyString);
        //we need a deserializer
        var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
        //get the object back from the stream
        pubKey = (RSAParameters)xs.Deserialize(sr);
      }

      //conversion for the private key is no black magic either ... omitted

      //we have a public key ... let's get a new csp and load that key
      csp = new RSACryptoServiceProvider();
      csp.ImportParameters(pubKey);

      //we need some data to encrypt
      var plainTextData = "foobar";

      //for encryption, always handle bytes...
      var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

      //apply pkcs#1.5 padding and encrypt our data 
      var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

      //we might want a string representation of our cypher text... base64 will do
      var cypherText = Convert.ToBase64String(bytesCypherText);


      /*
       * some transmission / storage / retrieval
       * 
       * and we want to decrypt our cypherText
       */

      //first, get our bytes back from the base64 string ...
      bytesCypherText = Convert.FromBase64String(cypherText);

      //we want to decrypt, therefore we need a csp and load our private key
      csp = new RSACryptoServiceProvider();
      csp.ImportParameters(privKey);

      //decrypt and strip pkcs#1.5 padding
      bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

      //get our original plainText back...
      plainTextData = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);
    }


    private static void DecryptPrivate(string dataToDecrypt)
    {
        var name = "test";
        var encryptedBase64 = @"Rzabx5380rkx2+KKB+HaJP2dOXDcOC7SkYOy4HN8+Nb9HmjqeZfGQlf+ZUa6uAfAJ3oAB2iIlHlnx+iXK3XDIX3izjoW1eeiNmdOWieNCu6YXqW4denUVEv0Z4EpAmEYgVImnEzoMdmPDEcl9UHgdWUmS4Bnq6T8Yqh3UZ/4NOc=";
        var encrypted = Convert.FromBase64String(encryptedBase64);
        using var privateKey = new RSACryptoServiceProvider();
        privateKey.FromXmlString(File.ReadAllText(name + "_priv.xml"));
        var decryptedBytes = privateKey.Decrypt(encrypted, false);
        var dectryptedText = Encoding.UTF8.GetString(decryptedBytes);
    }
public static string EncryptAES(string aesKey, string data)
    {
        byte[] key = Encoding.UTF8.GetBytes(aesKey);
        byte[] iv = new byte[16];
        using (var random = new RNGCryptoServiceProvider())
        {
            random.GetBytes(iv);
        }

        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] encryptedData = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(data), 0, data.Length);

            byte[] result = new byte[iv.Length + encryptedData.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encryptedData, 0, result, iv.Length, encryptedData.Length);

            return Convert.ToBase64String(result);
        }
    }

        // public static string EncryptWithPrivate(string privateKeyB64, string data)
        // {
        //     // byte[] privateKey = Convert.FromBase64String(privateKeyB64);
        //     // using (var rsa = RSA.Create())
        //     // {
        //     //     rsa.ImportRSAPrivateKey(privateKey, out _);

        //     //     byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(data), RSAEncryptionPadding.Pkcs1);
        //     //     return Convert.ToBase64String(encryptedData);
        //     // }

        //     //string a = "LS0tLS1CRUdJTiBQUklWQVRFIEtFWS0tLS0tCk1JSUV2Z0lCQURBTkJna3Foa2lHOXcwQkFRRUZBQVNDQktnd2dnU2tBZ0VBQW9JQkFRREpPTWRTYzVKT3hiMGMKTGViWWphT1VqLyt1UzJXZT0tLS0tRU5EIFBSSVZBVEUgS0VZLS0tLS0K";

        //     //string base64Encoded = Base64Encode(privateKeyB64);
        // // Console.WriteLine(base64Encoded);
        //    // return Convert.ToBase64String(base64Encoded.ToString());
        // }
        public static string Base64Encode(string plainText, string data)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            string base64Encoded = Convert.ToBase64String(plainTextBytes);
            return base64Encoded;
        }
    private static void EncryptPrivate(string dataToDecrypt)
    {
        var name = "test";
        var encryptedBase64 = @"Rzabx5380rkx2+KKB+HaJP2dOXDcOC7SkYOy4HN8+Nb9HmjqeZfGQlf+ZUa6uAfAJ3oAB2iIlHlnx+iXK3XDIX3izjoW1eeiNmdOWieNCu6YXqW4denUVEv0Z4EpAmEYgVImnEzoMdmPDEcl9UHgdWUmS4Bnq6T8Yqh3UZ/4NOc=";
        var encrypted = Convert.FromBase64String(encryptedBase64);
        using var privateKey = new RSACryptoServiceProvider();
        privateKey.FromXmlString(File.ReadAllText(name + "_priv.xml"));
        var decryptedBytes = privateKey.Decrypt(encrypted, false);
        var dectryptedText = Encoding.UTF8.GetString(decryptedBytes);

        
    }
}