using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class StreamSerializer : BinarySerializer, IDisposable {
        private readonly Stream stream;
        private bool isClosed = false;

        public StreamSerializer(Stream stream) {
            this.stream = stream;
        }

        public StreamSerializer() : this(new MemoryStream()) { }

        public void Dispose() { }

        public Stream Close() {
            this.isClosed = true;
            return this.stream;
        }

        public override Task WriteAsync(ArraySegment<byte> bytes) {
            if (this.isClosed) {
                throw new InvalidOperationException("Cannot write to stream after closing");
            }

            return this.stream.WriteAsync(bytes.Array, bytes.Offset, bytes.Count);
        }

        public override Task WriteAsync(Stream stream) {
            if (this.isClosed) {
                throw new InvalidOperationException("Cannot write to stream after closing");
            }

            return stream.CopyToAsync(this.stream);
        }

        public override Task<Stream> GetStreamAsync() {
            return Task.FromResult(this.stream);
        }
    }
}