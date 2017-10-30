using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class StreamDeserializer : IBinaryDeserializer, IDisposable {
        private Stream stream;
        private long initialPosition;

        public StreamDeserializer(Stream stream) {
            this.stream = stream;
            this.initialPosition = stream.Position;
        }

        ~StreamDeserializer() {
            GC.SuppressFinalize(this);
            this.Dispose();
        }

        public void Dispose() {
            this.stream.Dispose();
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

        public void Reset() {
            this.stream.Position = this.initialPosition;
        }

        public bool TryReadBytes(int count, out ArraySegment<byte> buffer) {
            var result = new ArraySegment<byte>(new byte[count]);
            if (!this.TryReadBytes(result)) {
                buffer = default;
                return false;
            }

            buffer = result;
            return true;
        }

        public bool TryReadBytes(ArraySegment<byte> buffer) {
            try {
                int pos = 0;
                int read = 0;

                while ((read = this.stream.Read(buffer.Array, buffer.Offset + pos, buffer.Count - pos)) > 0) {
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
    }
}