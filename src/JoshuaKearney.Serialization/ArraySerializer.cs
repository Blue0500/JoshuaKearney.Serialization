using System;
using System.Collections.Generic;
using System.Text;

namespace JoshuaKearney.Serialization {
    public class ArraySerializer : IBinarySerializer, IBinarySerializable {
        private ResizableArray<byte> array = new ResizableArray<byte>();
        private bool finalized = false;

        public IBinarySerializer Write(ArraySegment<byte> bytes) {
            if (this.finalized) {
                throw new InvalidOperationException("Cannot write to array after finalization");
            }

            this.array.AddRange(bytes);
            return this;
        }

        public ArraySegment<byte> Finalize() {
            this.finalized = true;
            return this.array.ToArraySegment();
        }

        public void Dispose() { }

        public void WriteTo(IBinarySerializer writer) {
            writer.Write(this.array.ToArraySegment());
        }
    }
}