using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class StreamDeserializer : BinaryDeserializer, IBinarySerializable {
        private Stream stream;

        public StreamDeserializer(Stream stream) {
            this.stream = stream;
        }

        public override async Task<ArraySegment<byte>> ReadToEndAsync() {
            ResizableArray<byte> data = new ResizableArray<byte>();
            byte[] buffer = new byte[1024];
            int read = 0;

            while ((read = await this.stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                data.AddRange(buffer);
            }

            return data.ToArraySegment();
        }

        public override Task<Stream> GetStreamAsync() {
            return Task.FromResult(this.stream);
        }

        public override Task ResetAsync() {
            this.stream.Position = 0;
            return Task.CompletedTask;
        }

        public override async Task<int> TryReadBytesAsync(ArraySegment<byte> buffer) {
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

        public override Task<ArraySegment<byte>> TryReadBytesAsync(int count) {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[count]);

            return this.TryReadBytesAsync(buffer).ContinueWith(task => {
                return new ArraySegment<byte>(buffer.Array, buffer.Offset, task.Result);
            });
        }

        public async Task WriteToAsync(BinarySerializer writer) {
            await writer.WriteAsync(this.stream);
        }
    }
}