using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JoshuaKearney.Serialization {
    public class StreamSerializer : IBinarySerializer, IDisposable {
        private Stream stream;

        public StreamSerializer(Stream stream) {
            this.stream = stream;
        }

        public StreamSerializer() : this(new MemoryStream()) { }

        ~StreamSerializer() {
            GC.SuppressFinalize(this);
            this.Dispose();
        }

        public void Dispose() {
            this.stream.Dispose();
        }

        public IBinarySerializer Write(ArraySegment<byte> bytes) {
            throw new NotImplementedException();
        }
    }
}