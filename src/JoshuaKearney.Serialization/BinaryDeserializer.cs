using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization { 
    public delegate Task<(bool success, T result)> DeserializeAsyncTryAction<T>();

    public interface IBinaryDeserializer : IDisposable, IBinarySerializable {
        Task<ArraySegment<byte>> ReadBytesAsync(int count);
        Task<int> ReadBytesAsync(ArraySegment<byte> result);

        Task<ArraySegment<byte>> ReadToEndAsync();
        Task<Stream> GetStreamAsync(bool disposeOriginal = false);
    }

    public abstract class BinaryDeserializer : IBinaryDeserializer {
        public abstract Task<ArraySegment<byte>> ReadBytesAsync(int count);
        public virtual async Task<int> ReadBytesAsync(ArraySegment<byte> result) {
            var bytes = await this.ReadBytesAsync(result.Count);

            if (bytes.Count <= 0) {
                return 0;
            }
            else {
                Buffer.BlockCopy(bytes.Array, bytes.Offset, result.Array, result.Offset, bytes.Count);
                return bytes.Count;
            }
        }

        public abstract Task<ArraySegment<byte>> ReadToEndAsync();
        public virtual Task<Stream> GetStreamAsync(bool disposeOriginal = false) {
            Stream ret = new SerializationStream(this);

            if (!disposeOriginal) {
                ret = new IndisposableStream(ret);
            }

            return Task.FromResult<Stream>(ret);
        }

        public abstract void Dispose();

        public virtual async Task WriteToAsync(IBinarySerializer writer) {
            await writer.WriteAsync(await this.ReadToEndAsync());
        }
    }

    public static partial class SerializationExtensions {
        public static Task<int> ReadBytesAsync(this IBinaryDeserializer deserial, byte[] buffer) {
            return deserial.ReadBytesAsync(new ArraySegment<byte>(buffer));
        }

        public static async Task<byte> ReadByteAsync(this IBinaryDeserializer reader) {
            return (await reader.ReadBytesAsync(1)).First();
        }

        public static async Task<short> ReadInt16Async(this IBinaryDeserializer reader) {
            var read = await reader.ReadBytesAsync(2);
            return BitConverter.ToInt16(read.Array, read.Offset);
        }

        public static async Task<int> ReadInt32Async(this IBinaryDeserializer reader) {
            var bytes = await reader.ReadBytesAsync(4);

            if (bytes.Count < 4) {
                throw new SerializationException("Unable to decode Int32, not enough bytes");
            }

            return BitConverter.ToInt32(bytes.Array, bytes.Offset);
        }

        public static async Task<long> ReadInt64Async(this IBinaryDeserializer reader) {
            var bytes = await reader.ReadBytesAsync(8);
            return BitConverter.ToInt64(bytes.Array, bytes.Offset);
        }

        public static async Task<float> ReadSingleAsync(this IBinaryDeserializer reader) {
            var read = await reader.ReadBytesAsync(4);          
            return BitConverter.ToSingle(read.Array, read.Offset);
        }

        public static async Task<double> ReadDoubleAsync(this IBinaryDeserializer reader) {
            var read = await reader.ReadBytesAsync(8);
            return BitConverter.ToDouble(read.Array, read.Offset);
        }

        public static async Task<ushort> ReadUInt16Async(this IBinaryDeserializer reader) {
            var read = await reader.ReadBytesAsync(2);
            return BitConverter.ToUInt16(read.Array, read.Offset);
        }

        public static async Task<uint> ReadUInt32Async(this IBinaryDeserializer reader) {
            var read = await reader.ReadBytesAsync(4);
            return BitConverter.ToUInt32(read.Array, read.Offset);
        }

        public static async Task<ulong> ReadUInt64Async(this IBinaryDeserializer reader) {
            var read = await reader.ReadBytesAsync(8);
            return BitConverter.ToUInt64(read.Array, read.Offset);
        }

        public static async Task<bool> ReadBooleanAsync(this IBinaryDeserializer reader) {
            var read = await reader.ReadBytesAsync(1);
            return BitConverter.ToBoolean(read.Array, read.Offset);
        }

        public static async Task<ArraySegment<byte>> ReadByteSequenceAsync(this IBinaryDeserializer reader) {
            int count = await reader.ReadInt32Async();

            if (count < 0) {
                throw new InvalidOperationException();
            }

            return await reader.ReadBytesAsync(count);
        }

        public static async Task<IReadOnlyList<T>> ReadSequenceAsync<T>(this IBinaryDeserializer reader, Func<IBinaryDeserializer, Task<T>> itemReader) {
            List<T> list = new List<T>();
            int count = await reader.ReadInt32Async();

            if (count < 0) {
                throw new InvalidOperationException("Sequence count cannot be less than 0");
            }

            for (int i = 0; i < count; i++) {
                list.Add(await itemReader(reader));
            }

            return list;
        }

        public static Task<IReadOnlyList<short>> ReadInt16SequenceAsync(this IBinaryDeserializer reader) {
            return reader.ReadSequenceAsync(rd => rd.ReadInt16Async());
        }

        public static Task<IReadOnlyList<int>> ReadInt32SequenceAsync(this IBinaryDeserializer reader) {
            return reader.ReadSequenceAsync(rd => rd.ReadInt32Async());
        }

        public static Task<IReadOnlyList<long>> ReadInt64SequenceAsync(this IBinaryDeserializer reader) {
            return reader.ReadSequenceAsync(rd => rd.ReadInt64Async());
        }

        public static async Task<string> ReadStringAsync(this IBinaryDeserializer reader, Encoding encoding) {
            var read = await reader.ReadByteSequenceAsync();
            return encoding.GetString(read.Array, read.Offset, read.Count);
        }

        public static Task<string> ReadStringAsync(this IBinaryDeserializer reader) {
            return reader.ReadStringAsync(Encoding.ASCII);
        }

        public static async Task<DateTime> ReadDateTimeAsync(this IBinaryDeserializer reader) {
            return new DateTime(await reader.ReadInt64Async());
        }       
    }
}