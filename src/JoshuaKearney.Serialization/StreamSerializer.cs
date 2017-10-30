using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class StreamSerializer : IBinarySerializer, IDisposable {
        private readonly Stream stream;
        private readonly long startPosition;
        private bool isClosed = false;

        public StreamSerializer(Stream stream) {
            this.stream = stream;
            this.startPosition = stream.Position;
        }

        public StreamSerializer() : this(new MemoryStream()) { }

        public void Dispose() { }

        public Stream Close() {
            this.isClosed = true;
            return this.stream;
        }

        public Task WriteAsync(ArraySegment<byte> bytes) {
            if (this.isClosed) {
                throw new InvalidOperationException("Cannot write to stream after finalization");
            }

            return this.stream.WriteAsync(bytes.Array, bytes.Offset, bytes.Count);
        }
    }
}