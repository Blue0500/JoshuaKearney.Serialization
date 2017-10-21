using JoshuaKearney.Core;
using System;
using System.Text;

namespace JoshuaKearney.Serialization.Testing {
    class IntAndString : IBinarySerializable {
        public int Int { get; set; }

        public string Str { get; set; }

        public void WriteTo(IBinarySerializer writer) {
            writer.Write(this.Int).Write(this.Str);
        }
    }

    class Program {
        static void Main(string[] args) {
            var poten = BuilderPotential.Empty<StringBuilder>();

            poten = poten.Append(sb => sb.Append("something"));
            poten = poten.Prepend(sb => sb.Append("first "));

            StringBuilder sbb = new StringBuilder();
            poten.Invoke(sbb);

            Console.WriteLine(sbb.ToString());
            Console.Read();

            //var writer = new ArraySerializer();
            //var some = new IntAndString() {
            //    Int = 9,
            //    Str = "other"
            //};

            //var poten = SerializationPotential.Empty;
            //poten = poten.Append(wr => wr.Write(some));
            //poten = poten.Prepend(wr => wr.Write("string"));

            //poten.Serialize(writer);

            //Console.WriteLine("Hello World!");
        }

        static void Some(IBinarySerializer wr) {

        }
    }
}