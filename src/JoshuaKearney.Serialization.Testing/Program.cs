using JoshuaKearney.Serialization.Linq;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization.Testing {
    class Program {
        static void Main(string[] args) {
            Run().GetAwaiter().GetResult();
        }

        static async Task Run() {
            var writer = new ArraySerializer();

            await writer.WriteAsync(
                new BinaryNode(
                    "some", 
                    async wr => {
                        await wr.WriteAsync("thing");
                        await wr.WriteAsync(45);
                    },
                    new BinaryNode("other", wr => wr.WriteAsync(98)),
                    new BinaryNode("other2", wr => wr.WriteAsync(87))
                )
            );

            var reader = new ArrayDeserializer(writer.Close());
            var node = await reader.ReadBinaryNodeAsync();

            Console.WriteLine(await node.Content.ReadStringAsync());
            Console.WriteLine(await node.Content.ReadInt32Async());
            Console.WriteLine(node.Children.First(x => x.Name == "other").NextSibling.Name);

            Console.Read();
        }
    }
}