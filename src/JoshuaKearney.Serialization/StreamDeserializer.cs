using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class StreamDeserializer : IBinaryDeserializer, IBinarySerializable {
        private Stream stream;
        private long initialPosition;

        public StreamDeserializer(Stream stream) {
            this.stream = stream;
            this.initialPosition = stream.Position;
        }

        public async Task<ArraySegment<byte>> ReadToEndAsync() {
            ResizableArray<byte> array = new ResizableArray<byte>();
            int read = 0;
            byte[] buffer = new byte[1024];
            
            while ((read = await this.stream.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                array.AddRange(buffer);
            }

            return array.ToArraySegment();
        }

        public Task ResetAsync() {
            this.stream.Position = this.initialPosition;
            return Task.CompletedTask;
        }

        public async Task<bool> TryReadBytesAsync(ArraySegment<byte> buffer) {
           try {
                int pos = 0;
                int read = 0;

                while ((read = await this.stream.ReadAsync(buffer.Array, buffer.Offset + pos, buffer.Count - pos)) > 0) {
                    pos += read;

                    if (pos + 1 >= buffer.Count) {
                        break;
                    }
                }

                if (pos + 1 >= buffer.Count) {
                    return true;
                }
                else {
                    return false;
                }
            }
            catch (IOException) {
                return false;
            }
            catch (NotSupportedException) {
                return false;
            }
            catch (ObjectDisposedException) {
                return false;
            }
        }

        public Task<(bool success, ArraySegment<byte> result)> TryReadBytesAsync(int count) {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[count]);

            return this.TryReadBytesAsync(buffer).ContinueWith(task => {
                return (task.Result, buffer);
            });
        }

        public async Task WriteToAsync(IBinarySerializer writer) {
            await writer.WriteAsync(await this.ReadToEndAsync());
        }
    }
}