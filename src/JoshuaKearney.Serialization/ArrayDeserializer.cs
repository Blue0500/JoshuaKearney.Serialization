using System;
using System.Collections.Generic;
using System.Text;

namespace JoshuaKearney.Serialization {
    public class ArrayDeserializer : IBinaryDeserializer {
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
                return true;
            }
        }

        public ArraySegment<byte> ReadRemaining() {
            return new ArraySegment<byte>(this.array.Array, this.array.Offset + this.pos, this.pos);
        }

        public void Reset() {
            this.pos = 0;
        }
    }
}