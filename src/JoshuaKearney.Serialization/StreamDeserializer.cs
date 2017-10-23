using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public ArraySegment<byte> ReadToEnd() {
            ResizableArray<byte> array = new ResizableArray<byte>();
            int read = 0;
            byte[] buffer = new byte[1024];
            
            while ((read = this.stream.Read(buffer, 0, buffer.Length)) > 0) {
                array.AddRange(buffer);
            }

            return array.ToArraySegment();
        }

        public void Reset() {
            this.stream.Position = this.initialPosition;
        }

        public bool TryReadBytes(int count, out ArraySegment<byte> buffer) {
            try {
                byte[] bytes = new byte[count];
                int pos = 0;
                int read = 0;

                while ((read = this.stream.Read(bytes, pos, count - pos)) > 0) {
                    pos += read;

                    if (pos + 1 >= count) {
                        break;
                    }
                }

                if (pos + 1 >= count) {
                    buffer = new ArraySegment<byte>(bytes);
                    return true;
                }
                else {
                    buffer = default;
                    return false;
                }
            }
            catch (IOException) {
                buffer = default;
                return false;
            }
            catch (NotSupportedException) {
                buffer = default;
                return false;
            }
            catch (ObjectDisposedException) {
                buffer = default;
                return false;
            }
        }
    }
}