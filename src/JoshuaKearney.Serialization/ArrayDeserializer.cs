using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArrayDeserializer : IBinaryDeserializer {
        private ArraySegment<byte> array;
        private int pos;

        public ArrayDeserializer(byte[] array) : this(new ArraySegment<byte>(array)) { }

        public ArrayDeserializer(ArraySegment<byte> array) {
            this.array = array;
        }

        public void Dispose() { }

        public bool TryReadBytes(int count, out ArraySegment<byte> buffer) {
            if (this.pos + count > this.array.Count) {
                buffer = new ArraySegment<byte>(new byte[0]);
                return false;
            }
            else {
                buffer = new ArraySegment<byte>(this.array.Array, this.array.Offset + this.pos, count);
                this.pos += count;
                return true;
            }
        }

        public void Reset() {
            this.pos = 0;
        }

        public Task<ArraySegment<byte>> ReadToEndAsync() {
            return Task.FromResult(
                new ArraySegment<byte>(
                    this.array.Array, 
                    this.array.Offset + this.pos,
                    this.array.Count - this.pos
                )
            );
        }

        public bool TryReadBytes(ArraySegment<byte> buffer) {
            if (!this.TryReadBytes(buffer.Count, out var read)) {
                return false;
            }

            Buffer.BlockCopy(read.Array, read.Offset, buffer.Array, buffer.Offset, buffer.Count);
            return true;
        }

        public Task<bool> TryReadBytesAsync(ArraySegment<byte> buffer) {
            return Task.FromResult(this.TryReadBytes(buffer));
        }
    }
}