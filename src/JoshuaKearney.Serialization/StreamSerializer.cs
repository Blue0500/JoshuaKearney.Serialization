using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class StreamSerializer : BinarySerializer {
        private readonly Stream stream;
        private bool isDisposed = false;

        public StreamSerializer(Stream stream) {
            this.stream = stream;
        }

        public StreamSerializer() : this(new MemoryStream()) { }

        public override Task WriteAsync(ArraySegment<byte> bytes) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamSerializer));
            }

            return this.stream.WriteAsync(bytes.Array, bytes.Offset, bytes.Count);
        }

        public override Task WriteAsync(Stream stream) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamSerializer));
            }

            return stream.CopyToAsync(this.stream);
        }

        public override Task<Stream> GetStreamAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(StreamSerializer));
            }

            return Task.FromResult<Stream>(new IndisposableStream(this.stream));
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