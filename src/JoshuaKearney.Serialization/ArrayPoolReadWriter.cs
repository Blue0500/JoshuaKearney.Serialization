using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArrayPoolReadWriter : IBinarySerializer, IBinaryDeserializer {
        private ArraySegment<byte> array;
        private ArrayPool<byte> pool;
        private int readPos;
        private int writePos;
        private int length;
        private bool isDisposed = false;
        private SemaphoreSlim writeSemaphore = new SemaphoreSlim(1, 1);

        public ArrayPoolReadWriter() : this(ArrayPool<byte>.Shared) { }

        public ArrayPoolReadWriter(ArrayPool<byte> pool) {
            this.array = new ArraySegment<byte>(pool.Rent(10));
            this.pool = pool;

            this.length = this.readPos = this.writePos = 0;
        }

        public ArrayPoolReadWriter(byte[] array, ArrayPool<byte> pool) : this(new ArraySegment<byte>(array), pool) { }

        public ArrayPoolReadWriter(ArraySegment<byte> array, ArrayPool<byte> pool) {
            this.array = array;
            this.pool = pool;

            this.length = array.Count;
            this.readPos = array.Offset;
            this.writePos = array.Offset + array.Count;
        }

        ~ArrayPoolReadWriter() {
            GC.SuppressFinalize(this);
            this.Dispose();
        }

        public void Dispose() {
            if (!this.isDisposed) {
                this.isDisposed = true;
                this.pool.Return(this.array.Array);
            }
        }

        public Task<Stream> GetStreamAsync(bool disposeOriginal = false) {
            Stream ret = new SerializationStream(this, this);

            if (!disposeOriginal) {
                ret = new IndisposableStream(ret);
            }

            return Task.FromResult(ret);
        }

        public Task<ArraySegment<byte>> ReadBytesAsync(int count) {
            if (this.readPos + count > this.array.Offset + this.length) {
                return this.ReadToEndAsync();
            }
            else {
                byte[] bytes = new byte[count];

                Buffer.BlockCopy(this.array.Array, this.readPos, bytes, 0, count);
                this.readPos += count;

                return Task.FromResult(new ArraySegment<byte>(bytes));
            }
        }

        public Task<int> ReadBytesAsync(ArraySegment<byte> buffer) {
            ArraySegment<byte> source;

            if (this.readPos + buffer.Count > this.array.Offset + this.length) {
                source = new ArraySegment<byte>(this.array.Array, this.readPos, this.length - this.readPos);
            }
            else {
                source = new ArraySegment<byte>(this.array.Array, this.readPos, buffer.Count);
            }

            Buffer.BlockCopy(source.Array, source.Offset, buffer.Array, buffer.Offset, source.Count);
            this.readPos += source.Count;

            return Task.FromResult(source.Count);
        }

        public Task<ArraySegment<byte>> ReadToEndAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(ArrayPoolDeserializer));
            }

            if (this.readPos > this.array.Offset + this.length) {
                return Task.FromResult(new ArraySegment<byte>(new byte[0]));
            }
            else {
                byte[] result = new byte[this.length - this.readPos];
                Buffer.BlockCopy(this.array.Array, this.readPos, result, 0, this.length - this.readPos);

                this.readPos = this.length;
                return Task.FromResult(new ArraySegment<byte>(result));
            }
        }

        public Task WriteAsync(ArraySegment<byte> bytes) {
            if (this.writePos + bytes.Count > this.array.Offset + this.array.Count) {
                byte[] newBuffer = this.pool.Rent(this.array.Count * 2 + bytes.Count);
                Buffer.BlockCopy(this.array.Array, this.array.Offset, newBuffer, 0, this.length);

                this.pool.Return(this.array.Array);
                this.array = new ArraySegment<byte>(newBuffer);
            }

            Array.ConstrainedCopy(bytes.Array, bytes.Offset, this.array.Array, this.writePos, bytes.Count);

            this.writePos += bytes.Count;
            this.length += bytes.Count;

            return Task.CompletedTask;
        }

        public async Task WriteToAsync(IBinarySerializer writer) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(ArrayPoolDeserializer));
            }

            if (this.readPos <= this.array.Offset + this.length) {
                await writer.WriteAsync(new ArraySegment<byte>(this.array.Array, this.readPos, this.length - this.readPos));
                this.readPos = this.length;
            }
        }

        public async Task WriteAsync(Stream stream) {
            byte[] buffer = new byte[1024];
            int read = 0;

            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await this.WriteAsync(buffer);
            }
        }
    }
}