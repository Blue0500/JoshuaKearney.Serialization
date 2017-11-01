using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization.Linq {
    internal class LazyDeserializer : IBinaryDeserializer, IBinarySerializable {
        private IBinarySerializable content;
        private ArrayDeserializer deserializer;

        public LazyDeserializer(IBinarySerializable content) {
            this.content = content;
        }

        public async Task<ArraySegment<byte>> ReadToEndAsync() {
            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            return await this.deserializer.ReadToEndAsync();
        }

        public async Task ResetAsync() {
            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            await this.deserializer.ResetAsync();
        }

        public async Task<bool> TryReadBytesAsync(ArraySegment<byte> result) {
            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            return await this.deserializer.TryReadBytesAsync(result);
        }

        public async Task<(bool success, ArraySegment<byte> result)> TryReadBytesAsync(int count) {
            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            return await this.deserializer.TryReadBytesAsync(count);
        }

        public async Task WriteToAsync(IBinarySerializer writer) {
            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            await this.deserializer.WriteToAsync(writer);
        }

        private async Task InitializeDeserializer() {
            ArraySerializer serial = new ArraySerializer();
            await serial.WriteAsync(this.content);
            this.deserializer = new ArrayDeserializer(serial.Close());
            this.content = null;
        }
    }
}