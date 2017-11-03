using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    internal class SerializationStream : Stream {
        private BinarySerializer serializer = null;
        private BinaryDeserializer deserializer = null;

        public SerializationStream(BinarySerializer serializer) {
            this.serializer = serializer;
        }

        public SerializationStream(BinaryDeserializer deserializer) {
            this.deserializer = deserializer;
        }

        public override bool CanRead => this.deserializer != null;

        public override bool CanSeek => false;

        public override bool CanWrite => this.serializer != null;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) {
            if (this.deserializer == null) {
                throw new InvalidOperationException();
            }

            return this.ReadAsync(buffer, offset, count, default).GetAwaiter().GetResult();
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            if (this.deserializer == null) {
                throw new InvalidOperationException();
            }

            return this.deserializer.TryReadBytesAsync(new ArraySegment<byte>(buffer, offset, count));
        }

        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotSupportedException();
        }

        public override void SetLength(long value) {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            if (this.serializer == null) {
                throw new InvalidOperationException();
            }

            this.WriteAsync(buffer, offset, count).GetAwaiter().GetResult();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            if (this.serializer == null) {
                throw new InvalidOperationException();
            }

            return this.serializer.WriteAsync(new ArraySegment<byte>(buffer, offset, count));
        }
    }
}