using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public interface IBinarySerializer : IDisposable {
        Task WriteAsync(ArraySegment<byte> bytes);

        Task WriteAsync(Stream stream);

        Task<Stream> GetStreamAsync(bool disposeOriginal = false);
    }

    public abstract class BinarySerializer : IBinarySerializer {
        protected bool IsDisposed { get; private set; } = false;

        public abstract Task WriteAsync(ArraySegment<byte> bytes);

        public virtual async Task WriteAsync(Stream stream) {
            byte[] buffer = new byte[1024];
            int read = 0;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await this.WriteAsync(buffer);
            }
        }

        public virtual Task<Stream> GetStreamAsync(bool disposeOriginal = false) {
            Stream ret = new SerializationStream(this);

            if (!disposeOriginal) {
                ret = new IndisposableStream(ret);
            }

            return Task.FromResult(ret);
        }

        public virtual void Dispose() {
            this.IsDisposed = true;
        }
    }

    public static partial class SerializationExtensions {
        public static async Task WriteSectorsAsync(this IBinarySerializer writer, IEnumerable<BuilderPotential<IBinarySerializer>> writers) {
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

            await writer.WriteAsync(count);

            foreach (var potential in writers) {
                using (ArraySerializer serializer = new ArraySerializer()) {
                    await potential(serializer);

                    var final = serializer.Array;

                    await writer.WriteAsync(final.Count);
                    await writer.WriteAsync(final);
                }
            }
        }

        public static Task WriteSectorsAsync(this IBinarySerializer writer, params BuilderPotential<IBinarySerializer>[] writers) {
            return writer.WriteSectorsAsync((IEnumerable<BuilderPotential<IBinarySerializer>>)writers);
        }

        public static Task WriteAsync(this IBinarySerializer writer, IBinarySerializable writable) {
            return writable.WriteToAsync(writer);
        }

        public static Task WriteAsync(this IBinarySerializer writer, BuilderPotential<IBinarySerializer> potential) {
            // TODO - Fix this
            return potential(writer);
        }

        public static Task WriteAsync(this IBinarySerializer writer, byte[] bytes) {
            return writer.WriteAsync(new ArraySegment<byte>(bytes));
        }

        public static Task WriteAsync(this IBinarySerializer writer, byte item) {
            return writer.WriteAsync(new[] { item });
        }

        public static Task WriteAsync(this IBinarySerializer writer, short item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, int item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, long item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }   

        public static Task WriteAsync(this IBinarySerializer writer, float item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, double item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, sbyte item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, ushort item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, uint item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, ulong item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static Task WriteAsync(this IBinarySerializer writer, bool item) {
            return writer.WriteAsync(BitConverter.GetBytes(item));
        }

        public static async Task WriteSequenceAsync(this IBinarySerializer writer, ArraySegment<byte> items) {
            await writer.WriteAsync(items.Count);
            await writer.WriteAsync(items);
        }

        public static Task WriteSequenceAsync(this IBinarySerializer writer, byte[] items) {
            return writer.WriteSequenceAsync(new ArraySegment<byte>(items));
        }

        public static async Task WriteSequenceAsync<T>(this IBinarySerializer writer, IEnumerable<T> items, Func<IBinarySerializer, T, Task> eachFunc) {
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
                await eachFunc(writer, item);
            }
        }

        public static Task WriteSequenceAsync<T>(this IBinarySerializer writer, IEnumerable<T> items) where T : IBinarySerializable {
            return writer.WriteSequenceAsync(items, (wr, x) => x.WriteToAsync(wr));
        }

        public static Task WriteSequenceAsync(this IBinarySerializer writer, IEnumerable<byte> items) {
            return writer.WriteSequenceAsync(items.ToArray());
        }

        public static Task WriteSequenceAsync(this IBinarySerializer writer, IEnumerable<short> items) {
            return writer.WriteSequenceAsync(items, (wr, x) => wr.WriteAsync(x));
        }

        public static Task WriteSequenceAsync(this IBinarySerializer writer, IEnumerable<int> items) {
            return writer.WriteSequenceAsync(items, (wr, x) => wr.WriteAsync(x));
        }

        public static Task WriteSequenceAsync(this IBinarySerializer writer, IEnumerable<long> items) {
            return writer.WriteSequenceAsync(items, (wr, x) => wr.WriteAsync(x));
        }

        public static Task WriteAsync(this IBinarySerializer writer, string str, Encoding encoding) {
            return writer.WriteSequenceAsync(encoding.GetBytes(str));
        }

        public static Task WriteAsync(this IBinarySerializer writer, string str) {
            return writer.WriteSequenceAsync(Encoding.ASCII.GetBytes(str));
        }

        public static Task WriteAsync(this IBinarySerializer writer, DateTime time) {
            return writer.WriteAsync(time.Ticks);
        }
    }
}