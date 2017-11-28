using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public static partial class SerializationExtensions {
        public static BinarySerializer SelectStream(this BinarySerializer serial, Func<Stream, Stream> selector) {
            return new WrappedSerializer(async () => {
                var stream = await serial.GetStreamAsync();
                return selector(stream);
            });
        }

        public static BinaryDeserializer SelectStream(this BinaryDeserializer deserial, Func<Stream, Stream> selector) {
            return new WrappedDeserializer(async () => {
                var stream = await deserial.GetStreamAsync();
                return selector(stream);
            });
        }

        public static BinarySerializer CompressGzip(this BinarySerializer serial) {
            return serial.SelectStream(x => new GZipStream(x, CompressionMode.Compress));
        }

        public static BinarySerializer CompressDeflate(this BinarySerializer serial) {
            return serial.SelectStream(x => new DeflateStream(x, CompressionMode.Compress));
        }

        public static BinaryDeserializer DecompressGzip(this BinaryDeserializer deserial) {
            return deserial.SelectStream(x => new GZipStream(x, CompressionMode.Decompress));
        }

        public static BinaryDeserializer DecompressDeflate(this BinaryDeserializer deserial) {
            return deserial.SelectStream(x => new DeflateStream(x, CompressionMode.Decompress));
        }

        public static BinarySerializer Encrypt(this BinarySerializer serial, ICryptoTransform algo) {
            return serial.SelectStream(x => new CryptoStream(x, algo, CryptoStreamMode.Write));
        }

        public static BinaryDeserializer Decrypt(this BinaryDeserializer deserial, ICryptoTransform algo) {
            return deserial.SelectStream(x => new CryptoStream(x, algo, CryptoStreamMode.Read));
        }

        private class WrappedDeserializer : BinaryDeserializer {
            private Func<Task<Stream>> initializer;
            private StreamDeserializer source;
            private bool isDisposed = false;

            public WrappedDeserializer(Func<Task<Stream>> initializer) {
                this.initializer = initializer;
            }

            public override void Dispose() {
                if (this.isDisposed) {
                    return;
                }

                this.isDisposed = true;
                this.source?.Dispose();

                GC.SuppressFinalize(this);
            }

            public override async Task<ArraySegment<byte>> ReadToEndAsync() {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedDeserializer));
                }

                if (this.source == null) {
                    var stream = await this.initializer();
                    this.source = new StreamDeserializer(stream);
                }

                return await this.source.ReadToEndAsync();
            }

            public override async Task ResetAsync() {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedDeserializer));
                }

                if (this.source == null) {
                    var stream = await this.initializer();
                    this.source = new StreamDeserializer(stream);
                }

                await this.source.ResetAsync();
            }

            public override async Task<ArraySegment<byte>> TryReadBytesAsync(int count) {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedDeserializer));
                }

                if (this.source == null) {
                    var stream = await this.initializer();
                    this.source = new StreamDeserializer(stream);
                }

                return await this.source.TryReadBytesAsync(count);
            }

            public override async Task<Stream> GetStreamAsync() {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedDeserializer));
                }

                if (this.source == null) {
                    var stream = await this.initializer();
                    this.source = new StreamDeserializer(stream);
                }

                return await this.source.GetStreamAsync();
            }

            public override async Task<int> TryReadBytesAsync(ArraySegment<byte> result) {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedDeserializer));
                }

                if (this.source == null) {
                    var stream = await this.initializer();
                    this.source = new StreamDeserializer(stream);
                }

                return await this.source.TryReadBytesAsync(result);
            }
        }

        private class WrappedSerializer : BinarySerializer {
            private Func<Task<Stream>> initializer;
            private Stream gzip;
            private bool isDisposed = false;

            public WrappedSerializer(Func<Task<Stream>> initializer) {
                this.initializer = initializer;
            }

            public override void Dispose() {
                if (this.isDisposed) {
                    return;
                }

                this.isDisposed = true;
                this.gzip?.Dispose();

                GC.SuppressFinalize(this);
            }

            public override async Task WriteAsync(ArraySegment<byte> bytes) {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedSerializer));
                }

                if (this.gzip == null) {
                    this.gzip = await this.initializer();
                }

                await this.gzip.WriteAsync(bytes.Array, bytes.Offset, bytes.Count);
            }

            public override async Task<Stream> GetStreamAsync() {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedSerializer));
                }

                if (this.gzip == null) {
                    this.gzip = await this.initializer();
                }

                return new IndisposableStream(this.gzip);
            }

            public override async Task WriteAsync(Stream stream) {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(WrappedSerializer));
                }

                if (this.gzip == null) {
                    this.gzip = await this.initializer();
                }

                await stream.CopyToAsync(this.gzip);
            }
        }
    }
}