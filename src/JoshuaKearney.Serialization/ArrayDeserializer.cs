using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArrayDeserializer : IBinaryDeserializer, IBinarySerializable {
        private ArraySegment<byte> array;
        private int pos;

        public ArrayDeserializer(byte[] array) : this(new ArraySegment<byte>(array)) { }

        public ArrayDeserializer(ArraySegment<byte> array) {
            this.array = array;
        }

        public void Dispose() { }

        public bool TryReadBytes(int count, out ArraySegment<byte> buffer) {
            if (this.pos + count > this.array.Count) {
                buffer = new ArraySegment<byte>(new byte[0]);
                return false;
            }
            else {
                buffer = new ArraySegment<byte>(this.array.Array, this.array.Offset + this.pos, count);
                this.pos += count;
                return true;
            }
        }

        public Task ResetAsync() {
            this.pos = 0;
            return Task.CompletedTask;
        }

        public Task<ArraySegment<byte>> ReadToEndAsync() {
            return Task.FromResult(
                new ArraySegment<byte>(
                    this.array.Array, 
                    this.array.Offset + this.pos,
                    this.array.Count - this.pos
                )
            );
        }

        public Task<bool> TryReadBytesAsync(ArraySegment<byte> result) {
            return this.TryReadBytesAsync(result.Count).ContinueWith(task => {
                if (task.Result.success) {
                    Buffer.BlockCopy(
                        task.Result.result.Array, 
                        task.Result.result.Offset, 
                        result.Array, 
                        result.Offset, 
                        result.Count
                    );
                }

                return task.Result.success;
            });
        }

        public Task<(bool success, ArraySegment<byte> result)> TryReadBytesAsync(int count) {
            if (this.pos + count > this.array.Count) {
                return Task.FromResult((false, default(ArraySegment<byte>)));
            }
            else {
                var buffer = new ArraySegment<byte>(this.array.Array, this.array.Offset + this.pos, count);
                this.pos += count;
                return Task.FromResult((true, buffer));
            }
        }

        public async Task WriteToAsync(IBinarySerializer writer) {
            await writer.WriteAsync(await this.ReadToEndAsync());
        }
    }
}