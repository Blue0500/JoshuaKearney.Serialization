using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArrayDeserializer : BinaryDeserializer, IBinarySerializable {
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

        public override Task ResetAsync() {
            this.pos = 0;
            return Task.CompletedTask;
        }

        public override Task<ArraySegment<byte>> ReadToEndAsync() {
            if (this.pos >= this.array.Count) {
                return Task.FromResult(new ArraySegment<byte>(new byte[0]));
            }
            else {
                var result = Task.FromResult(
                    new ArraySegment<byte>(
                        this.array.Array,
                        this.array.Offset + this.pos,
                        this.array.Count - this.pos
                    )
                );

                this.pos = this.array.Offset + this.array.Count;
                return result;
            }
        }

        public override Task<ArraySegment<byte>> TryReadBytesAsync(int count) {
            if (this.pos + count > this.array.Count) {
                return this.ReadToEndAsync();
            }
            else {
                var buffer = new ArraySegment<byte>(this.array.Array, this.array.Offset + this.pos, count);
                this.pos += count;
                return Task.FromResult(buffer);
            }
        }

        public async Task WriteToAsync(BinarySerializer writer) {
            await writer.WriteAsync(await this.ReadToEndAsync());
        }
    }
}