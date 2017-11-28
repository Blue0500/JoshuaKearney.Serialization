using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    internal class IndisposableStream : Stream {
        private Stream stream;

        public IndisposableStream(Stream toWrap) {
            this.stream = toWrap;
        }

        public override bool CanRead => this.stream.CanRead;

        public override bool CanSeek => this.stream.CanSeek;

        public override bool CanWrite => this.stream.CanWrite;

        public override long Length => this.stream.Length;

        public override long Position { get => this.stream.Position; set => this.stream.Position = value; }

        public override void Flush() => this.stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) => this.stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => this.stream.Seek(offset, origin);

        public override void SetLength(long value) => this.stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) => this.stream.Write(buffer, offset, count);

        public override bool CanTimeout => this.stream.CanTimeout;

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) {
            return this.stream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken) {
            return this.stream.FlushAsync(cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return this.stream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte() {
            return this.stream.ReadByte();
        }

        public override int ReadTimeout { get => this.stream.ReadTimeout; set => this.stream.ReadTimeout = value; }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            return this.stream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value) {
            this.stream.WriteByte(value);
        }

        public override int WriteTimeout { get => this.stream.WriteTimeout; set => this.stream.WriteTimeout = value; }
    }
}