using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArraySerializer : IBinarySerializer, IBinarySerializable {
        private ResizableArray<byte> array = new ResizableArray<byte>();
        private bool finalized = false;

        public Task WriteAsync(ArraySegment<byte> bytes) {
            if (this.finalized) {
                throw new InvalidOperationException("Cannot write to array after finalization");
            }

            this.array.AddRange(bytes);
            return Task.CompletedTask;
        }

        public ArraySegment<byte> Close() {
            this.finalized = true;
            return this.array.ToArraySegment();
        }

        public void Dispose() { }

        public Task WriteToAsync(IBinarySerializer writer) {
            return writer.WriteAsync(this.array.ToArraySegment());
        }
    }
}