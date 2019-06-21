using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public static partial class SerializationExtensions {
        public static async Task<IBinarySerializer> CompressGzipAsync(this IBinarySerializer serial) {
            var stream = new GZipStream(await serial.GetStreamAsync(), CompressionMode.Compress);
            return new StreamSerializer(stream);
        }

        public static async Task<IBinaryDeserializer> DecompressGzipAsync(this IBinaryDeserializer deserial) {
            var stream = new GZipStream(await deserial.GetStreamAsync(), CompressionMode.Decompress);
            return new StreamDeserializer(stream);
        }

        public static async Task<IBinarySerializer> CompressDeflateAsync(this IBinarySerializer serial) {
            var stream = new DeflateStream(await serial.GetStreamAsync(), CompressionMode.Compress);
            return new StreamSerializer(stream);
        }

        public static async Task<IBinaryDeserializer> DecompressDeflateAsync(this IBinaryDeserializer deserial) {
            var stream = new DeflateStream(await deserial.GetStreamAsync(), CompressionMode.Decompress);
            return new StreamDeserializer(stream);
        }

        public static async Task<IBinarySerializer> EncryptAesAsync(this IBinarySerializer serial, byte[] key) {
            using (var aes = Aes.Create()) {
                aes.KeySize = key.Length * 8;
                aes.Key = key;

                await serial.WriteSequenceAsync(aes.IV);

                var stream = new CryptoStream(await serial.GetStreamAsync(), aes.CreateEncryptor(), CryptoStreamMode.Write);
                return new StreamSerializer(stream);
            }
        }

        public static async Task<IBinaryDeserializer> DecryptAesAsync(this IBinaryDeserializer deserial, byte[] key) {
            using (var aes = Aes.Create()) {
                aes.KeySize = key.Length * 8;
                aes.Key = key;
                aes.IV = (await deserial.ReadByteSequenceAsync()).ToArray();

                var stream = new CryptoStream(await deserial.GetStreamAsync(), aes.CreateDecryptor(), CryptoStreamMode.Read);
                return new StreamDeserializer(stream);
            }
        }
    }
}