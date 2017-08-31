using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityLauncher.Core.Attributes;

namespace UnityLauncher.Core
{
    public class EncryptedStringPropertyResolver : DefaultContractResolver
    {
        private readonly byte[] encryptionKeyBytes;

        public EncryptedStringPropertyResolver(string encryptionKey)
        {
            if (encryptionKey == null)
                throw new ArgumentNullException(nameof(encryptionKey));
            
            using (SHA256Managed sha = new SHA256Managed())
            {
                encryptionKeyBytes =
                    sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));
            }
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

            foreach (JsonProperty prop in props.Where(p => p.PropertyType == typeof(string)))
            {
                PropertyInfo pi = type.GetProperty(prop.UnderlyingName);
                if (pi != null && pi.GetCustomAttribute(typeof(JsonEncryptAttribute), true) != null)
                {
                    prop.ValueProvider =
                        new EncryptedStringValueProvider(pi, encryptionKeyBytes);
                }
            }

            return props;
        }

        class EncryptedStringValueProvider : IValueProvider
        {
            private readonly PropertyInfo targetProperty;
            private readonly byte[] encryptionKey;

            public EncryptedStringValueProvider(PropertyInfo targetProperty, byte[] encryptionKey)
            {
                this.targetProperty = targetProperty;
                this.encryptionKey = encryptionKey;
            }
            
            public object GetValue(object target)
            {
                try
                {
                    string value = (string)targetProperty.GetValue(target);
                    if (string.IsNullOrEmpty(value)) return "";
                    byte[] buffer = Encoding.UTF8.GetBytes(value);

                    using (MemoryStream inputStream = new MemoryStream(buffer, false))
                    using (MemoryStream outputStream = new MemoryStream())
                    using (AesManaged aes = new AesManaged { Key = encryptionKey })
                    {
                        byte[] iv = aes.IV;
                        outputStream.Write(iv, 0, iv.Length);
                        outputStream.Flush();

                        ICryptoTransform encryptor = aes.CreateEncryptor(encryptionKey, iv);
                        using (CryptoStream cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                        {
                            inputStream.CopyTo(cryptoStream);
                        }

                        return Convert.ToBase64String(outputStream.ToArray());
                    }
                }
                catch
                {
                    //
                }
                return null;
            }

            public void SetValue(object target, object value)
            {
                try
                {
                    if (string.IsNullOrEmpty((string) value))
                    {
                        targetProperty.SetValue(target, "");
                        return;
                    }

                    byte[] buffer = Convert.FromBase64String((string)value);

                    using (MemoryStream inputStream = new MemoryStream(buffer, false))
                    using (MemoryStream outputStream = new MemoryStream())
                    using (AesManaged aes = new AesManaged { Key = encryptionKey })
                    {
                        byte[] iv = new byte[16];
                        int bytesRead = inputStream.Read(iv, 0, 16);
                        if (bytesRead < 16)
                        {
                            throw new CryptographicException("IV is missing or invalid.");
                        }

                        ICryptoTransform decryptor = aes.CreateDecryptor(encryptionKey, iv);
                        using (CryptoStream cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                        {
                            cryptoStream.CopyTo(outputStream);
                        }

                        string decryptedValue = Encoding.UTF8.GetString(outputStream.ToArray());
                        targetProperty.SetValue(target, decryptedValue);
                    }
                }
                catch
                {
                    //
                }
                
            }

        }
    }
}