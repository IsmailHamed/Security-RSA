using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Server
{
    public class MyRSA
    {
        RSACryptoServiceProvider csp;
        public MyRSA()
        {
            csp = new RSACryptoServiceProvider(2048);
        }

        public MyRSA(RSAParameters publicKey)
        {
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);
        }

        public RSAParameters GetPrivateKey()
        {
            return csp.ExportParameters(true);
        }

        public RSAParameters GetPublicKey()
        {
            return csp.ExportParameters(false);
        }

        public string GetStringRepresentation(RSAParameters key)
        {
            //converting the public key into a string representation
            string pubKeyString;
            {
                //we need some buffer
                var sw = new System.IO.StringWriter();
                //we need a serializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //serialize the key into the stream
                xs.Serialize(sw, key);
                //get the string from the stream
                pubKeyString = sw.ToString();
            }

            return pubKeyString;
        }

        public RSAParameters GetRSAParametersRepresentation(string key)
        {
            RSAParameters pubKey = new RSAParameters();
            {
                //get a stream from the string
                var sr = new System.IO.StringReader(key);
                //we need a deserializer
                var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //get the object back from the stream
                pubKey = (RSAParameters)xs.Deserialize(sr);
            }
            return pubKey;
        }

        public byte[] Encrypt(byte[] content)
        {
            return csp.Encrypt(content, false);
        }

        public byte[] Decrypt(byte[] content)
        {
            return csp.Decrypt(content, false);
        }
    }
}
