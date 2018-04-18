using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class StreamDeserializer : BinaryDeserializer, IBinarySerializable {
        private bool isDisposed = false;
        private Stream stream;

        public StreamDeserializer(Stream stream) {
            this.stream = stream;
        }

        public override async Task<ArraySegment<byte>> ReadToEndAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamDeserializer));
            }

            ResizableArray<byte> data = new ResizableArray<byte>();
            byte[] buffer = new byte[1024];
            int read = 0;

            while ((read = await this.stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                data.AddRange(buffer);
            }

            return data.ToArraySegment();
        }

        public override Task<Stream> GetStreamAsync(bool disposeOriginal) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamDeserializer));
            }

            Stream ret = this.stream;

            if (!disposeOriginal) {
                ret = new IndisposableStream(ret);
            }

            return Task.FromResult(ret);
        }

        public override async Task<int> ReadBytesAsync(ArraySegment<byte> buffer) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamDeserializer));
            }

            int pos = 0;

            try {
                int read = 0;

                while ((read = await this.stream.ReadAsync(buffer.Array, buffer.Offset + pos, buffer.Count - pos)) > 0) {
                    pos += read;

                    if (pos + 1 >= buffer.Count) {
                        break;
                    }
                }

                return pos;
            }
            catch (IOException) {
                return pos;
            }
            catch (NotSupportedException) {
                return pos;
            }
            catch (ObjectDisposedException) {
                return pos;
            }
        }

        public async override Task<ArraySegment<byte>> ReadBytesAsync(int count) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamDeserializer));
            }

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[count]);
            int read = await this.ReadBytesAsync(buffer);

            return new ArraySegment<byte>(buffer.Array, buffer.Offset, read);
        }

        public override async Task WriteToAsync(IBinarySerializer writer) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamDeserializer));
            }

            await writer.WriteAsync(this.stream);
        }

        public override void Dispose() {
            if (this.isDisposed) {
                return;
            }

            this.isDisposed = true;
            this.stream.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}