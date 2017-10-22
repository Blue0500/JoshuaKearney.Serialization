using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public Stream Close(bool resetPosition = true) {
            this.isClosed = true;

            if (resetPosition) {
                this.stream.Position = this.startPosition;
            }

            return this.stream;
        }

        public IBinarySerializer Write(ArraySegment<byte> bytes) {
            if (this.isClosed) {
                throw new InvalidOperationException("Cannot write to array after finalization");
            }

            this.stream.Write(bytes.Array, bytes.Offset, bytes.Count);
            return this;
        }
    }
}