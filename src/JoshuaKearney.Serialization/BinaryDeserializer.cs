using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization { 
    public delegate Task<T> DeserializeAsyncAction<T>();

    public delegate Task<(bool success, T result)> DeserializeAsyncTryAction<T>();

    public abstract class BinaryDeserializer : IDisposable {
        public abstract Task<ArraySegment<byte>> TryReadBytesAsync(int count);
        public virtual async Task<int> TryReadBytesAsync(ArraySegment<byte> result) {
            var bytes = await this.TryReadBytesAsync(result.Count);

            if (bytes.Count <= 0) {
                return 0;
            }
            else {
                Buffer.BlockCopy(bytes.Array, bytes.Offset, result.Array, result.Offset, bytes.Count);
                return bytes.Count;
            }
        }

        public abstract Task<ArraySegment<byte>> ReadToEndAsync();
        public virtual Task<Stream> GetStreamAsync() {
            return Task.FromResult<Stream>(new IndisposableStream(new SerializationStream(this)));
        }

        public abstract Task ResetAsync();

        public static implicit operator BinaryDeserializer(BinaryReader writer) {
            return new StreamDeserializer(writer.BaseStream);
        }

        public static implicit operator BinaryDeserializer(StreamReader writer) {
            return new StreamDeserializer(writer.BaseStream);
        }

        public abstract void Dispose();
    }

    public static partial class SerializationExtensions {
        public static async Task<ArraySegment<byte>> ReadBytesAsync(this BinaryDeserializer reader, int count) {
            var result = await reader.TryReadBytesAsync(count);

            return result;
        }

        public static Task ReadAsync(this BinaryDeserializer reader, BuilderPotential<BinaryDeserializer> potential) {
            return potential(reader);
        }

        public static async Task<IReadOnlyList<BinaryDeserializer>> ReadSectorsAsync(this BinaryDeserializer reader) {
            int count = await reader.ReadInt32Async();
            List<BinaryDeserializer> deserializers = new List<BinaryDeserializer>();

            for (int i = 0; i < count; i++) {
                int length = await reader.ReadInt32Async();
                var bytes = await reader.ReadBytesAsync(length);
                deserializers.Add(new ArrayDeserializer(bytes));
            }

            return deserializers;
        }

        public static async Task<IReadOnlyList<BinaryDeserializer>> TryReadSectorsAsync(this BinaryDeserializer reader) {
            var (success, count) = await reader.TryReadInt32Async();
            if (!success || count < 0) {
                return new BinaryDeserializer[0];
            }

            List<BinaryDeserializer> deserializers = new List<BinaryDeserializer>();

            for (int i = 0; i < count; i++) {
                (success, count) = await reader.TryReadInt32Async();
                if (!success || count < 0) {
                    return new BinaryDeserializer[0];
                }

                var maybe = await reader.TryReadBytesAsync(count);
                if (maybe.Count != count) {
                    return new BinaryDeserializer[0];
                }

                deserializers.Add(new ArrayDeserializer(maybe));
            }

            return deserializers;
        }

        public static Task<byte> ReadByteAsync(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(1).ContinueWith(task => task.Result.First());
        }

        public static Task<(bool success, byte result)> TryReadByteAsync(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(1).ContinueWith(task => {
                return (task.Result.Count == 1, task.Result.FirstOrDefault());
            });
        }

        public static Task<short> ReadInt16Async(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(2).ContinueWith(task => {
                return BitConverter.ToInt16(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, short result)> TryReadInt16Async(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(2).ContinueWith(task => {
                if (task.Result.Count != 2) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToInt16(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static Task<int> ReadInt32Async(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(4).ContinueWith(task => {
                return BitConverter.ToInt32(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, int result)> TryReadInt32Async(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(4).ContinueWith(task => {
                if (task.Result.Count != 4) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToInt32(task.Result.Array, task.Result.Offset)); 
                }
            });
        }

        public static Task<long> ReadInt64Async(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(8).ContinueWith(task => {
                return BitConverter.ToInt64(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, long result)> TryReadInt64Async(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(8).ContinueWith(task => {
                if (task.Result.Count != 8) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToInt64(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static Task<float> ReadSingleAsync(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(4).ContinueWith(task => {                
                return BitConverter.ToSingle(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, float result)> TryReadSingleAsync(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(4).ContinueWith(task => {
                if (task.Result.Count != 4) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToSingle(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static Task<double> ReadDoubleAsync(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(8).ContinueWith(task => {
                return BitConverter.ToDouble(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, double result)> TryReadDoubleAsync(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(8).ContinueWith(task => {
                if (task.Result.Count != 8) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToDouble(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static Task<ushort> ReadUInt16Async(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(2).ContinueWith(task => {
                return BitConverter.ToUInt16(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, ushort result)> TryReadUInt16Async(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(2).ContinueWith(task => {
                if (task.Result.Count != 2) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToUInt16(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static Task<uint> ReadUInt32Async(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(4).ContinueWith(task => {
                return BitConverter.ToUInt32(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, uint result)> TryReadUInt32Async(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(4).ContinueWith(task => {
                if (task.Result.Count != 4) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToUInt32(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static Task<ulong> ReadUInt64Async(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(8).ContinueWith(task => {
                return BitConverter.ToUInt64(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, ulong result)> TryReadUInt64Async(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(8).ContinueWith(task => {
                if (task.Result.Count != 8) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToUInt64(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static Task<bool> ReadBooleanAsync(this BinaryDeserializer reader) {
            return reader.ReadBytesAsync(1).ContinueWith(task => {
                return BitConverter.ToBoolean(task.Result.Array, task.Result.Offset);
            });
        }

        public static Task<(bool success, bool result)> TryReadBooleanAsync(this BinaryDeserializer reader) {
            return reader.TryReadBytesAsync(1).ContinueWith(task => {
                if (task.Result.Count != 1) {
                    return (false, default);
                }
                else {
                    return (true, BitConverter.ToBoolean(task.Result.Array, task.Result.Offset));
                }
            });
        }

        public static async Task<ArraySegment<byte>> ReadByteSequenceAsync(this BinaryDeserializer reader) {
            int count = await reader.ReadInt32Async();

            if (count < 0) {
                throw new InvalidOperationException();
            }

            return await reader.ReadBytesAsync(count);
        }

        public static async Task<(bool success, ArraySegment<byte> result)> TryReadByteSequenceAsync(this BinaryDeserializer reader) {
            var (success, count) = await reader.TryReadInt32Async();

            if (!success || count < 0) {
                return (false, default);
            }

            var bytes = await reader.TryReadBytesAsync(count);
            return (bytes.Count == count, bytes);
        }

        public static async Task<IReadOnlyList<T>> ReadSequenceAsync<T>(this BinaryDeserializer reader, DeserializeAsyncAction<T> itemReader) {
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

        public static async Task<(bool success, IReadOnlyList<T> result)> TryReadSequenceAsync<T>(this BinaryDeserializer reader, DeserializeAsyncTryAction<T> itemReader) {
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

        public static Task<IReadOnlyList<short>> ReadInt16SequenceAsync(this BinaryDeserializer reader) {
            return reader.ReadSequenceAsync(reader.ReadInt16Async);
        }

        public static Task<(bool success, IReadOnlyList<short> result)> TryReadInt16SequenceAsync(this BinaryDeserializer reader) {
            return reader.TryReadSequenceAsync(reader.TryReadInt16Async);
        }

        public static Task<IReadOnlyList<int>> ReadInt32SequenceAsync(this BinaryDeserializer reader) {
            return reader.ReadSequenceAsync(reader.ReadInt32Async);
        }

        public static Task<(bool success, IReadOnlyList<int> result)> TryReadInt32SequenceAsync(this BinaryDeserializer reader) {
            return reader.TryReadSequenceAsync(reader.TryReadInt32Async);
        }

        public static Task<IReadOnlyList<long>> ReadInt64SequenceAsync(this BinaryDeserializer reader) {
            return reader.ReadSequenceAsync(reader.ReadInt64Async);
        }

        public static Task<(bool success, IReadOnlyList<long> result)> TryReadInt64Sequence(this BinaryDeserializer reader) {
            return reader.TryReadSequenceAsync(reader.TryReadInt64Async);
        }

        public static Task<string> ReadStringAsync(this BinaryDeserializer reader, Encoding encoding) {
            return reader.ReadByteSequenceAsync().ContinueWith(task => {
                return encoding.GetString(task.Result.Array, task.Result.Offset, task.Result.Count);
            });
        }

        public static Task<string> ReadStringAsync(this BinaryDeserializer reader) {
            return reader.ReadStringAsync(Encoding.ASCII);
        }

        public static Task<(bool success, string result)> TryReadStringAsync(this BinaryDeserializer reader, Encoding encoding) {
            return reader.TryReadByteSequenceAsync().ContinueWith(task => {
                if (!task.Result.success) {
                    return (false, default);
                }

                var bytes = task.Result.result;
                return (true, encoding.GetString(bytes.Array, bytes.Offset, bytes.Count));
            });
        }

        public static Task<(bool success, string result)> TryReadStringAsync(this BinaryDeserializer reader) {
            return reader.TryReadStringAsync(Encoding.ASCII);
        }
    }
}