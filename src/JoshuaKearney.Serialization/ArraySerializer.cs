using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArraySerializer : BinarySerializer, IBinarySerializable {
        private bool isClosed = false;
        private ResizableArray<byte> array = new ResizableArray<byte>();

        public override Task WriteAsync(ArraySegment<byte> bytes) {
            if (this.isClosed) {
                throw new InvalidOperationException("Cannot write to stream after closing");
            }

            this.array.AddRange(bytes);
            return Task.CompletedTask;
        }

        public Task WriteToAsync(BinarySerializer writer) {
            return writer.WriteAsync(this.array.ToArraySegment());
        }

        public ArraySegment<byte> Close() {
            this.isClosed = true;
            return this.array.ToArraySegment();
        }
    }
}