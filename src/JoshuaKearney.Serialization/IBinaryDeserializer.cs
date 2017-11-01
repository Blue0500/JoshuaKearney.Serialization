using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization { 
    public delegate Task<T> DeserializeAsyncAction<T>();

    public delegate Task<(bool success, T result)> DeserializeAsyncTryAction<T>();

    public interface IBinaryDeserializer {
        Task<bool> TryReadBytesAsync(ArraySegment<byte> result);
        Task<(bool success, ArraySegment<byte> result)> TryReadBytesAsync(int count);

        Task<ArraySegment<byte>> ReadToEndAsync();
        Task ResetAsync();
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
            return potential(reader);
        }

        public static async Task<IReadOnlyList<IBinaryDeserializer>> ReadSectorsAsync(this IBinaryDeserializer reader) {
            int count = await reader.ReadInt32Async();
            List<IBinaryDeserializer> deserializers = new List<IBinaryDeserializer>();

            for (int i = 0; i < count; i++) {
                int length = await reader.ReadInt32Async();
                var bytes = await reader.ReadBytesAsync(length);
                deserializers.Add(new ArrayDeserializer(bytes));
            }

            return deserializers;
        }

        public static async Task<(bool success, IReadOnlyList<IBinaryDeserializer> sectors)> TryReadSectors(this IBinaryDeserializer reader) {
            var (success, count) = await reader.TryReadInt32Async();
            if (!success || count < 0) {
                return (false, default);
            }

            List<IBinaryDeserializer> deserializers = new List<IBinaryDeserializer>();

            for (int i = 0; i < count; i++) {
                (success, count) = await reader.TryReadInt32Async();
                if (!success || count < 0) {
                    return (false, default);
                }

                var maybe = await reader.TryReadBytesAsync(count);
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

        public static Task<(bool success, double result)> TryReadDoubleAsync(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(8).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToDouble(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static Task<ushort> ReadUInt16Async(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(2).ContinueWith(task => {
                return BitConverter.ToUInt16(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, ushort result)> TryReadUInt16Async(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(2).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToUInt16(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static Task<uint> ReadUInt32Async(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(4).ContinueWith(task => {
                return BitConverter.ToUInt32(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, uint result)> TryReadUInt32Async(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(4).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToUInt32(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static Task<ulong> ReadUInt64Async(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(8).ContinueWith(task => {
                return BitConverter.ToUInt64(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, ulong result)> TryReadUInt64Async(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(8).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToUInt64(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static Task<bool> ReadBooleanAsync(this IBinaryDeserializer reader) {
            return reader.ReadBytesAsync(1).ContinueWith(task => {
                return BitConverter.ToBoolean(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, bool result)> TryReadBooleanAsync(this IBinaryDeserializer reader) {
            return reader.TryReadBytesAsync(1).ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToBoolean(task.Result.result.Array, task.Result.result.Offset));
                }
            });
        }

        public static async Task<ArraySegment<byte>> ReadByteSequenceAsync(this IBinaryDeserializer reader) {
            int count = await reader.ReadInt32Async();

            if (count < 0) {
                throw new InvalidOperationException();
            }

            return await reader.ReadBytesAsync(count);
        }

        public static async Task<(bool success, ArraySegment<byte> result)> TryReadByteSequenceAsync(this IBinaryDeserializer reader) {
            var (success, count) = await reader.TryReadInt32Async();

            if (!success || count < 0) {
                return (false, default);
            }

            return await reader.TryReadBytesAsync(count);
        }

        public static async Task<IReadOnlyList<T>> ReadSequenceAsync<T>(this IBinaryDeserializer reader, DeserializeAsyncAction<T> itemReader) {
            List<T> list = new List<T>();
            int count = await reader.ReadInt32Async();

            if (count < 0) {
                throw new InvalidOperationException("Sequence count cannot be less than 0");
            }

            for (int i = 0; i < count; i++) {
                list.Add(await itemReader());
            }

            return list;
        }

        public static async Task<(bool success, IReadOnlyList<T> result)> TryReadSequenceAsync<T>(this IBinaryDeserializer reader, DeserializeAsyncTryAction<T> itemReader) {
            List<T> list = new List<T>();
            var (success, count) = await reader.TryReadInt32Async();

            if (!success || count < 0) {
                return (false, default);
            }

            for (int i = 0; i < count; i++) {
                var (itemSuccess, tResult) = await itemReader();

                if (!itemSuccess) {
                    return (false, default);
                }

                list.Add(tResult);
            }

            return (true, list);
        }

        public static Task<IReadOnlyList<short>> ReadInt16SequenceAsync(this IBinaryDeserializer reader) {
            return reader.ReadSequenceAsync(reader.ReadInt16Async);
        }

        public static Task<(bool success, IReadOnlyList<short> result)> TryReadInt16Sequence(this IBinaryDeserializer reader) {
            return reader.TryReadSequenceAsync(reader.TryReadInt16Async);
        }

        public static Task<IReadOnlyList<int>> ReadInt32SequenceAsync(this IBinaryDeserializer reader) {
            return reader.ReadSequenceAsync(reader.ReadInt32Async);
        }

        public static Task<(bool success, IReadOnlyList<int> result)> TryReadInt32SequenceAsync(this IBinaryDeserializer reader) {
            return reader.TryReadSequenceAsync(reader.TryReadInt32Async);
        }

        public static Task<IReadOnlyList<long>> ReadInt64SequenceAsync(this IBinaryDeserializer reader) {
            return reader.ReadSequenceAsync(reader.ReadInt64Async);
        }

        public static Task<(bool success, IReadOnlyList<long> result)> TryReadInt64Sequence(this IBinaryDeserializer reader) {
            return reader.TryReadSequenceAsync(reader.TryReadInt64Async);
        }

        public static Task<string> ReadStringAsync(this IBinaryDeserializer reader, Encoding encoding) {
            return reader.ReadByteSequenceAsync().ContinueWith(task => {
                return encoding.GetString(task.Result.Array, task.Result.Offset, task.Result.Count);
            });
        }

        public static Task<string> ReadStringAsync(this IBinaryDeserializer reader) {
            return reader.ReadStringAsync(Encoding.ASCII);
        }

        public static Task<(bool success, string result)> TryReadStringAsync(this IBinaryDeserializer reader, Encoding encoding) {
            return reader.TryReadByteSequenceAsync().ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }

                var bytes = task.Result.result;
                return (true, encoding.GetString(bytes.Array, bytes.Offset, bytes.Count));
            });
        }

        public static Task<(bool success, string result)> TryReadString(this IBinaryDeserializer reader) {
            return reader.TryReadStringAsync(Encoding.ASCII);
        }
    }
}