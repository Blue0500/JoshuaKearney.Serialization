using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JoshuaKearney.Serialization {
    public interface IBinarySerializer : IDisposable {
        IBinarySerializer Write(ArraySegment<byte> bytes);
    }

    public static partial class SerializationExtensions {
        public static IBinarySerializer WriteBlocks(this IBinarySerializer writer, IEnumerable<IBinarySerializer> writers) {
            int count;

            if (writers is ICollection<IBinarySerializer> collection) {
                count = collection.Count;
            }
            else if (writers is IReadOnlyCollection<IBinarySerializer> readCollection) {
                count = readCollection.Count;
            }
            else {
                count = writers.Count();
            }

            writer.Write(count);

            foreach ()
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, IBinarySerializable writable) {
            writable.WriteTo(writer);
            return writer;
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, BuilderPotential<IBinarySerializer> potential) {
            potential(writer);
            return writer;
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, byte[] bytes) {
            return writer.Write(new ArraySegment<byte>(bytes));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, short item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, int item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, long item) {
            return writer.Write(BitConverter.GetBytes(item));
        }   

        public static IBinarySerializer Write(this IBinarySerializer writer, float item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, double item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, sbyte item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, ushort item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, uint item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, ulong item) {
            return writer.Write(BitConverter.GetBytes(item));
        }

        public static IBinarySerializer WriteSequence(this IBinarySerializer writer, ArraySegment<byte> items) {
            writer.Write(items.Count);
            return writer.Write(items);
        }

        public static IBinarySerializer WriteSequence(this IBinarySerializer writer, byte[] items) {
            return writer.WriteSequence(new ArraySegment<byte>(items));
        }

        public static IBinarySerializer WriteSequence<T>(this IBinarySerializer writer, IEnumerable<T> items, Action<T> eachFunc) {
            int count;

            if (items is ICollection<T> collection) {
                count = collection.Count;
            }
            else if (items is IReadOnlyCollection<T> readcollection) {
                count = readcollection.Count;
            }
            else {
                count = items.Count();
            }

            writer.Write(count);

            foreach (T item in items) {
                eachFunc(item);
            }

            return writer;
        }

        public static IBinarySerializer WriteSequence(this IBinarySerializer writer, IEnumerable<IBinarySerializable> items) {
            return writer.WriteSequence(items, x => x.WriteTo(writer));
        }

        public static IBinarySerializer WriteSequence(this IBinarySerializer writer, IEnumerable<byte> items) {
            return writer.WriteSequence(items, x => writer.Write(x));
        }

        public static IBinarySerializer WriteSequence(this IBinarySerializer writer, IEnumerable<short> items) {
            return writer.WriteSequence(items, x => writer.Write(x));
        }

        public static IBinarySerializer WriteSequence(this IBinarySerializer writer, IEnumerable<int> items) {
            return writer.WriteSequence(items, x => writer.Write(x));
        }

        public static IBinarySerializer WriteSequence(this IBinarySerializer writer, IEnumerable<long> items) {
            return writer.WriteSequence(items, x => writer.Write(x));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, string str, Encoding encoding) {
            return writer.WriteSequence(encoding.GetBytes(str));
        }

        public static IBinarySerializer Write(this IBinarySerializer writer, string str) {
            return writer.WriteSequence(Encoding.ASCII.GetBytes(str));
        }
    }
}