using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArrayDeserializer : BinaryDeserializer {
        private ArraySegment<byte> array;
        private int pos;

        protected bool IsDisposed { get; private set; } = false;

        public ArrayDeserializer(byte[] array) : this(new ArraySegment<byte>(array)) { }

        public ArrayDeserializer(ArraySegment<byte> array) {
            this.array = array;
        }

        ~ArrayDeserializer() {
            GC.SuppressFinalize(this);
            this.Dispose();
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

        public override Task<ArraySegment<byte>> ReadBytesAsync(int count) {
            if (this.pos + count > this.array.Count) {
                return this.ReadToEndAsync();
            }
            else {
                var buffer = new ArraySegment<byte>(this.array.Array, this.array.Offset + this.pos, count);
                this.pos += count;
                return Task.FromResult(buffer);
            }
        }

        public override async Task WriteToAsync(IBinarySerializer writer) {
            await writer.WriteAsync(await this.ReadToEndAsync());
        }

        public override void Dispose() {
            if (!this.IsDisposed) {
                this.IsDisposed = true;
            }
        }
    }
}