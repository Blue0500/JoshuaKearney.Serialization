using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public abstract class BinarySerializer {
        public abstract Task WriteAsync(ArraySegment<byte> bytes);

        public virtual async Task WriteAsync(Stream stream) {
            byte[] buffer = new byte[1024];
            int read = 0;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await this.WriteAsync(buffer);
            }
        }

        public virtual Task<Stream> GetStreamAsync() {
            return Task.FromResult<Stream>(new SerializationStream(this));
        }

        public static implicit operator BinarySerializer(BinaryWriter writer) {
            return new StreamSerializer(writer.BaseStream);
        }

        public static implicit operator BinarySerializer(StreamWriter writer) {
            return new StreamSerializer(writer.BaseStream);
        }
    }

    public static partial class SerializationExtensions {
        public static async Task WriteSectorsAsync(this BinarySerializer writer, IEnumerable<BuilderPotential<BinarySerializer>> writers) {
            int count;

            if (writers is ICollection<BinarySerializer> collection) {
                count = collection.Count;
            }
            else if (writers is IReadOnlyCollection<BinarySerializer> readCollection) {
                count = readCollection.Count;
            }
            else {
                count = writers.Count();
            }

            await writer.WriteAsync(count);

            foreach (var potential in writers) {
                ArraySerializer serializer = new ArraySerializer();
                await potential(serializer);

                var final = serializer.Close();

                await writer.WriteAsync(final.Count);
                await writer.WriteAsync(final);
            }
        }

        public static Task WriteSectorsAsync(this BinarySerializer writer, params BuilderPotential<BinarySerializer>[] writers) {
            return writer.WriteSectorsAsync((IEnumerable<BuilderPotential<BinarySerializer>>)writers);
        }

        public static Task WriteAsync(this BinarySerializer writer, IBinarySerializable writable) {
            return writable.WriteToAsync(writer);
        }

        public static Task WriteAsync(this BinarySerializer writer, BuilderPotential<BinarySerializer> potential) {
            // TODO - Fix this
            return potential(writer);
        }

        public static Task WriteAsync(this BinarySerializer writer, byte[] bytes) {
            return writer.WriteAsync(new ArraySegment<byte>(bytes));
        }

        public static Task WriteAsync(this BinarySerializer writer, short item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, int item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, long item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }   

        public static Task WriteAsync(this BinarySerializer writer, float item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, double item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, sbyte item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, ushort item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, uint item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, ulong item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this BinarySerializer writer, bool item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static async Task WriteSequenceAsync(this BinarySerializer writer, ArraySegment<byte> items) {
            await writer.WriteAsync(items.Count);
            await writer.WriteAsync(items);
        }

        public static Task WriteSequenceAsync(this BinarySerializer writer, byte[] items) {
            return writer.WriteSequenceAsync(new ArraySegment<byte>(items));
        }

        public static async Task WriteSequenceAsync<T>(this BinarySerializer writer, IEnumerable<T> items, Func<T, Task> eachFunc) {
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

            await writer.WriteAsync(count);

            foreach (T item in items) {
                await eachFunc(item);
            }
        }

        public static Task WriteSequenceAsync<T>(this BinarySerializer writer, IEnumerable<T> items) where T : IBinarySerializable {
            return writer.WriteSequenceAsync(items, x => x.WriteToAsync(writer));
        }

        public static Task WriteSequenceAsync(this BinarySerializer writer, IEnumerable<byte> items) {
            return writer.WriteSequenceAsync(items, x => writer.WriteAsync(x));
        }

        public static Task WriteSequenceAsync(this BinarySerializer writer, IEnumerable<short> items) {
            return writer.WriteSequenceAsync(items, x => writer.WriteAsync(x));
        }

        public static Task WriteSequenceAsync(this BinarySerializer writer, IEnumerable<int> items) {
            return writer.WriteSequenceAsync(items, x => writer.WriteAsync(x));
        }

        public static Task WriteSequenceAsync(this BinarySerializer writer, IEnumerable<long> items) {
            return writer.WriteSequenceAsync(items, x => writer.WriteAsync(x));
        }

        public static Task WriteAsync(this BinarySerializer writer, string str, Encoding encoding) {
            return writer.WriteSequenceAsync(encoding.GetBytes(str));
        }

        public static Task WriteAsync(this BinarySerializer writer, string str) {
            return writer.WriteSequenceAsync(Encoding.ASCII.GetBytes(str));
        }
    }
}