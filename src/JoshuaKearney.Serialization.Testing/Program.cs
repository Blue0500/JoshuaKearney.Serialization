using JoshuaKearney.Serialization.Linq;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization.Testing {
    class Program {
        static void Main(string[] args) {           

            Run().GetAwaiter().GetResult();
        }

        static async Task Run() {
            

            Console.WriteLine(await deserial.ReadInt32Async());
            Console.Read();

            //BuilderPotential<BinarySerializer> data = wr => {
            //    return wr.WriteAsync("This is some sentense whereupon I am testing the length issues");
            //};

            //ArraySerializer serial = new ArraySerializer();
            //StreamSerializer serial2 = new StreamSerializer();

            //await serial.WriteAsync(data);
            //await serial2.WriteAsync(data);

            //BinaryDeserializer deserial = new ArrayDeserializer(serial.Close());

            //Stream s = serial2.Close();
            //s.Position = 0;

            //BinaryDeserializer deserial2 = new StreamDeserializer(s);

            //deserial = new StreamDeserializer(await deserial.GetStreamAsync());
            //deserial2 = new StreamDeserializer(await deserial2.GetStreamAsync());

            //Console.WriteLine(string.Join(" ", await deserial.ReadBytesAsync(25)));
            //Console.WriteLine(string.Join(" ", await deserial2.ReadBytesAsync(25)));
            //Console.WriteLine();

            //Console.WriteLine(string.Join(" ", await deserial.ReadBytesAsync(25)));
            //Console.WriteLine(string.Join(" ", await deserial2.ReadBytesAsync(25)));
            //Console.WriteLine();

            //Console.WriteLine(string.Join(" ", await deserial.ReadBytesAsync(25)));
            //Console.WriteLine(string.Join(" ", await deserial2.ReadBytesAsync(25)));
            //Console.WriteLine();

            //Console.WriteLine(string.Join(" ", await deserial.ReadBytesAsync(25)));
            //Console.WriteLine(string.Join(" ", await deserial2.ReadBytesAsync(25)));
            //Console.WriteLine();

            //Console.Read();
        }
    }
}