using JoshuaKearney.Serialization.Linq;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization.Testing {
    class Program {
        static Aes aes = Aes.Create();

        static void Main(string[] args) {
            var node = new BinaryNode("s");

            //aes.KeySize = 256;
            //aes.GenerateKey();
            //aes.GenerateIV();
            //aes.Padding = PaddingMode.PKCS7;

            //Run().GetAwaiter().GetResult();

            Console.Read();
        }

        static async Task Run2() {               
            ArraySegment<byte> encrypted;
            using (var ms = new MemoryStream()) {
                using (var crypto = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write)) {
                    using (StreamWriter writer = new StreamWriter(crypto)) {
                        await writer.WriteLineAsync("Hello, World!");
                    }
                }

                encrypted = ms.ToArray();
            }

            using (var ms = new MemoryStream(encrypted.Array, encrypted.Offset, encrypted.Count)) 
            using (var crypto = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read)) 
            using (StreamReader reader = new StreamReader(crypto)) {
                Console.WriteLine(await reader.ReadLineAsync());
            }
        }

        static async Task Run() {
            //var serial = new ArraySerializer();

            //using (var crypto = serial.Encrypt(aes.CreateEncryptor()))
            //using (var comp = crypto.CompressDeflate()) {  
            //    await comp.WriteAsync("Hello, World!");
            //    await comp.WriteAsync(0);
            //}

            //using (var decomp = new ArrayDeserializer(serial.Array).Decrypt(aes.CreateDecryptor()))
            //using (var crypto = decomp.DecompressDeflate()) { 
            //    Console.WriteLine(await crypto.ReadStringAsync());
            //    Console.WriteLine(await crypto.ReadBooleanAsync());
            //}
        }
    }
}