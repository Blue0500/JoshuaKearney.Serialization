using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public class ArraySerializer : BinarySerializer, IBinarySerializable {
        private ResizableArray<byte> array = new ResizableArray<byte>();

        public ArraySegment<byte> Array => this.array.ToArraySegment();

        public override Task WriteAsync(ArraySegment<byte> bytes) {
            this.array.AddRange(bytes);

            return Task.CompletedTask;
        }

        public Task WriteToAsync(BinarySerializer writer) {
            return writer.WriteAsync(this.array.ToArraySegment());
        }

        public override void Dispose() { }
    }
}