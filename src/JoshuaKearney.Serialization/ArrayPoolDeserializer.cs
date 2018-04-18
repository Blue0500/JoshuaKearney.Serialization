using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArrayPoolDeserializer : BinaryDeserializer, IBinarySerializable {
        private byte[] array;
        private ArrayPool<byte> pool;
        private int pos;

        protected bool IsDisposed { get; private set; } = false;

        public ArrayPoolDeserializer(byte[] array, ArrayPool<byte> pool) {
            this.array = array;
            this.pool = pool;
        }

        ~ArrayPoolDeserializer() {
            GC.SuppressFinalize(this);
            this.Dispose();
        }

        public override Task<ArraySegment<byte>> ReadToEndAsync() {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(nameof(ArrayPoolDeserializer));
            }

            if (this.pos >= this.array.Length) {
                return Task.FromResult(new ArraySegment<byte>(new byte[0]));
            }
            else {
                byte[] result = new byte[this.array.Length - this.pos];
                Buffer.BlockCopy(this.array, this.pos, result, 0, this.array.Length - this.pos);

                this.pos = this.array.Length;
                return Task.FromResult(new ArraySegment<byte>(result));
            }
        }

        public override async Task<ArraySegment<byte>> ReadBytesAsync(int count) {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(nameof(ArrayPoolDeserializer));
            }

            if (this.pos + count > this.array.Length) {
                return await this.ReadToEndAsync();
            }
            else {
                byte[] result = new byte[count];
                int read = await this.ReadBytesAsync(result);

                return new ArraySegment<byte>(result, 0, read);
            }
        }

        public override Task<int> ReadBytesAsync(ArraySegment<byte> buffer) {
            ArraySegment<byte> source;

            if (this.pos + buffer.Count > this.array.Length) {
                source = new ArraySegment<byte>(this.array, this.pos, this.pos + this.array.Length);
            }
            else {
                source = new ArraySegment<byte>(this.array, this.pos, buffer.Count);
            }

            Buffer.BlockCopy(source.Array, source.Offset, buffer.Array, buffer.Offset, source.Count);
            this.pos += source.Count;

            return Task.FromResult(source.Count);
        }

        public override async Task WriteToAsync(IBinarySerializer writer) {
            if (this.IsDisposed) {
                throw new ObjectDisposedException(nameof(ArrayPoolDeserializer));
            }

            if (this.pos < this.array.Length) {
                var result = new ArraySegment<byte>(
                    this.array,
                    this.pos,
                    this.array.Length - this.pos
                );                

                this.pos = this.array.Length;
                await writer.WriteAsync(result);
            }
        }

        public override void Dispose() {
            if (!this.IsDisposed) {
                this.IsDisposed = true;
                this.pool.Return(this.array);
            }
        }
    }
}