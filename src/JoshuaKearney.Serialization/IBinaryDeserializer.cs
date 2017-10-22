using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace JoshuaKearney.Serialization { 
    public delegate T DeserializeAction<T>();

    public delegate bool DeserializeTryAction<T>(out T result);

    public interface IBinaryDeserializer : IDisposable {
        bool TryReadBytes(int count, out ArraySegment<byte> buffer);
        void Reset();
    }

    public static partial class SerializationExtensions {
        public static ArraySegment<byte> ReadBytes(this IBinaryDeserializer reader, int count) {
            if (!reader.TryReadBytes(count, out var bytes)) {
                throw new InvalidOperationException();
            }

            return bytes;
        }

        public static IBinaryDeserializer Read(this IBinaryDeserializer reader, BuilderPotential<IBinaryDeserializer> potential) {
            potential(reader);
            return reader;
        }

        public static IReadOnlyList<IBinaryDeserializer> ReadSectors(this IBinaryDeserializer reader) {
            int count = reader.ReadInt32();
            List<IBinaryDeserializer> deserializers = new List<IBinaryDeserializer>();

            for (int i = 0; i < count; i++) {
                int length = reader.ReadInt32();
                var bytes = reader.ReadBytes(length);
                deserializers.Add(new ArrayDeserializer(bytes));
            }

            return deserializers;
        }

        public static bool TryReadSectors(this IBinaryDeserializer reader, out IReadOnlyList<IBinaryDeserializer> result) {
            if (!reader.TryReadInt32(out int count)) {
                result = default;
                return false;
            }

            List<IBinaryDeserializer> deserializers = new List<IBinaryDeserializer>();
            result = deserializers;

            for (int i = 0; i < count; i++) {
                if (!reader.TryReadInt32(out int length)) {
                    return false;
                }

                if (!reader.TryReadBytes(length, out var bytes)) {
                    return false;
                }

                deserializers.Add(new ArrayDeserializer(bytes));
            }

            return true;
        }

        public static byte ReadByte(this IBinaryDeserializer reader) {
            return reader.ReadBytes(1).First();
        }

        public static bool TryReadByte(this IBinaryDeserializer reader, out byte result) {
            if (!reader.TryReadBytes(1, out var buffer)) {
                result = 0;
                return false;
            }

            result = buffer.Array[buffer.Offset];
            return true;
        }

        public static short ReadInt16(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(2);
            return BitConverter.ToInt16(bytes.Array, bytes.Offset);
        }

        public static bool TryReadInt16(this IBinaryDeserializer reader, out short result) {
            if (!reader.TryReadBytes(2, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToInt16(bytes.Array, bytes.Offset);
            return true;
        }

        public static int ReadInt32(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(4);
            return BitConverter.ToInt32(bytes.Array, bytes.Offset);
        }

        public static bool TryReadInt32(this IBinaryDeserializer reader, out int result) {
            if (!reader.TryReadBytes(4, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToInt32(bytes.Array, bytes.Offset);
            return true;
        }

        public static long ReadInt64(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(8);
            return BitConverter.ToInt64(bytes.Array, bytes.Offset);
        }

        public static bool TryReadInt64(this IBinaryDeserializer reader, out long result) {
            if (!reader.TryReadBytes(8, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToInt64(bytes.Array, bytes.Offset);
            return true;
        }

        public static float ReadSingle(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(4);
            return BitConverter.ToSingle(bytes.Array, bytes.Offset);
        }

        public static bool TryReadSingle(this IBinaryDeserializer reader, out float result) {
            if (!reader.TryReadBytes(4, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToSingle(bytes.Array, bytes.Offset);
            return true;
        }

        public static double ReadDouble(this IBinaryDeserializer reader) {
            var bytes = reader.ReadBytes(8);
            return BitConverter.ToDouble(bytes.Array, bytes.Offset);
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

        public static bool TryReadInt64(this IBinaryDeserializer reader, out ulong result) {
            if (!reader.TryReadBytes(8, out var bytes)) {
                result = 0;
                return false;
            }

            result = BitConverter.ToUInt64(bytes.Array, bytes.Offset);
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