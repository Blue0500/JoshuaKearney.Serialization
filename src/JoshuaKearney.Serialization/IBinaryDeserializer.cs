using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization { 
    public delegate T DeserializeAction<T>();

    public delegate bool DeserializeTryAction<T>(out T result);

    public interface IBinaryDeserializer : IDisposable {
        Task<bool> TryReadBytesAsync(ArraySegment<byte> result);
        Task<(bool success, ArraySegment<byte> result)> TryReadBytesAsync(int count);

        Task<ArraySegment<byte>> ReadToEndAsync();
        void Reset();
    }

    public static partial class SerializationExtensions {
        public static async Task<ArraySegment<byte>> ReadBytesAsync(this IBinaryDeserializer reader, int count) {
            var result = await reader.TryReadBytesAsync(count);

            if (!result.success) {
                throw new InvalidOperationException();
            }

            return result.result;
        }

        public static Task ReadAsync(this IBinaryDeserializer reader, BuilderPotential<IBinaryDeserializer> potential) {
            // TODO - Fix this
            potential(reader);
            return Task.CompletedTask;
        }

        public static async Task<IReadOnlyList<IBinaryDeserializer>> ReadSectorsAsync(this IBinaryDeserializer reader) {
            int count = reader.ReadInt32();
            List<IBinaryDeserializer> deserializers = new List<IBinaryDeserializer>();

            for (int i = 0; i < count; i++) {
                int length = await reader.ReadInt32Async();
                var bytes = await reader.ReadBytesAsync(length);
                deserializers.Add(new ArrayDeserializer(bytes));
            }

            return deserializers;
        }

        public static async Task<(bool success, IReadOnlyList<IBinaryDeserializer> sectors)> TryReadSectors(this IBinaryDeserializer reader) {
            if (!reader.TryReadInt32(out int count)) {
                return (false, default);
            }

            List<IBinaryDeserializer> deserializers = new List<IBinaryDeserializer>();

            for (int i = 0; i < count; i++) {
                if (!reader.TryReadInt32(out int length)) {
                    return (false, default);
                }

                var maybe = await reader.TryReadBytesAsync(length);
                if (!maybe.success) {
                    return (false, default);
                }

                deserializers.Add(new ArrayDeserializer(maybe.result));
            }

            return (true, deserializers);
        }

        public static Task<byte> ReadByteAsync(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(1).ContinueWith(task => task.Result.First());
        }

        public static Task<(bool success, byte result)> TryReadByteAsync(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(1).ContinueWith(task => {
                return (task.Result.success, task.Result.result.First());
            });
        }

        public static Task<short> ReadInt16Async(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(2).ContinueWith(task => {
                return BitConverter.ToInt16(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, short result)> TryReadInt16Async(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(2).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToInt16(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static Task<int> ReadInt32Async(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(4).ContinueWith(task => {
                return BitConverter.ToInt32(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, int result)> TryReadInt32Async(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(4).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToInt32(task.Result.result.Array, task.Result.result.Offset)); 
                }
            });
        }

        public static Task<long> ReadInt64Async(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(8).ContinueWith(task => {
                return BitConverter.ToInt64(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, long result)> TryReadInt64Async(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(8).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToInt64(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static Task<float> ReadSingleAsync(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(4).ContinueWith(task => {                
                return BitConverter.ToSingle(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, float result)> TryReadSingleAsync(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(4).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToSingle(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static Task<double> ReadDoubleAsync(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(8).ContinueWith(task => {
                return BitConverter.ToDouble(task.Result.Array, task.Result.Offset);
            });
        }

        public static bool TryReadDouble(this IBinaryDeserializer reader, out double result) {
            if (!reader.TryReadBytes(8, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToDouble(bytes.Array, bytes.Offset);
            return true;
        }

        public static ushort ReadUInt16(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(2);
            return BitConverter.ToUInt16(bytes.Array, bytes.Offset);
        }

        public static bool TryReadUInt16(this IBinaryDeserializer reader, out ushort result) {
            if (!reader.TryReadBytes(2, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToUInt16(bytes.Array, bytes.Offset);
            return true;
        }

        public static uint ReadUInt32(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(4);
            return BitConverter.ToUInt32(bytes.Array, bytes.Offset);
        }

        public static bool TryReadUInt32(this IBinaryDeserializer reader, out uint result) {
            if (!reader.TryReadBytes(4, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToUInt32(bytes.Array, bytes.Offset);
            return true;
        }

        public static ulong ReadUInt64(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(8);
            return BitConverter.ToUInt64(bytes.Array, bytes.Offset);
        }

        public static bool TryReadUInt64(this IBinaryDeserializer reader, out ulong result) {
            if (!reader.TryReadBytes(8, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToUInt64(bytes.Array, bytes.Offset);
            return true;
        }

        public static bool ReadBoolean(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(1);
            return BitConverter.ToBoolean(bytes.Array, bytes.Offset);
        }

        public static bool TryReadBoolean(this IBinaryDeserializer reader, out bool result) {
            if (!reader.TryReadBytes(1, out var bytes)) {
                result = default;
                return false;
            }

            result = BitConverter.ToBoolean(bytes.Array, bytes.Offset);
            return true;
        }

        public static ArraySegment<byte> ReadByteSequence(this IBinaryDeserializer reader) {
            int count = reader.ReadInt32();

            if (count < 0) {
                throw new InvalidOperationException();
            }

            return reader.ReadBytes(count);
        }

        public static bool TryReadByteSequence(this IBinaryDeserializer reader, out ArraySegment<byte> result) {
            if (!reader.TryReadInt32(out int count)) {
                result = default;
                return false;
            }

            if (count < 0) {
                result = default;
                return false;
            }

            return reader.TryReadBytes(count, out result);
        }

        public static IReadOnlyList<T> ReadSequence<T>(this IBinaryDeserializer reader, DeserializeAction<T> itemReader) {
            List<T> list = new List<T>();
            int count = reader.ReadInt32();

            if (count < 0) {
                throw new InvalidOperationException("Sequence count cannot be less than 0");
            }

            for (int i = 0; i < count; i++) {
                list.Add(itemReader());
            }

            return list;
        }

        public static bool TryReadSequence<T>(this IBinaryDeserializer reader, DeserializeTryAction<T> itemReader, out IReadOnlyList<T> result) {
            List<T> list = new List<T>();
            
            if (!reader.TryReadInt32(out int count)) {
                result = default;
                return false;
            }

            if (count < 0) {
                result = default;
                return false;
            }

            for (int i = 0; i < count; i++) {
                if (!itemReader(out T tResult)) {
                    result = default;
                    return false;
                }

                list.Add(tResult);
            }

            result = list;
            return true;
        }

        public static IReadOnlyList<short> ReadInt16Sequence(this IBinaryDeserializer reader) {
            return reader.ReadSequence(() => reader.ReadInt16());
        }

        public static bool TryReadInt16Sequence(this IBinaryDeserializer reader, out IReadOnlyList<short> result) {
            return reader.TryReadSequence(reader.TryReadInt16, out result);
        }

        public static IReadOnlyList<int> ReadInt32Sequence(this IBinaryDeserializer reader) {
            return reader.ReadSequence(() => reader.ReadInt32());
        }

        public static bool TryReadInt32Sequence(this IBinaryDeserializer reader, out IReadOnlyList<int> result) {
            return reader.TryReadSequence(reader.TryReadInt32, out result);
        }

        public static IReadOnlyList<long> ReadInt64Sequence(this IBinaryDeserializer reader) {
            return reader.ReadSequence(() => reader.ReadInt64());
        }

        public static bool TryReadInt64Sequence(this IBinaryDeserializer reader, out IReadOnlyList<long> result) {
            return reader.TryReadSequence(reader.TryReadInt64, out result);
        }

        public static string ReadString(this IBinaryDeserializer reader, Encoding encoding) {
            var buffer = reader.ReadByteSequence();
            return encoding.GetString(buffer.Array, buffer.Offset, buffer.Count);
        }

        public static string ReadString(this IBinaryDeserializer reader) {
            return reader.ReadString(Encoding.ASCII);
        }

        public static bool TryReadString(this IBinaryDeserializer reader, Encoding encoding, out string result) {
            if (!reader.TryReadByteSequence(out var bytes)) {
                result = default;
                return false;
            }

            result = encoding.GetString(bytes.Array, bytes.Offset, bytes.Count);
            return true;
        }

        public static bool TryReadString(this IBinaryDeserializer reader, out string result) {
            return reader.TryReadString(Encoding.ASCII, out result);
        }
    }
}