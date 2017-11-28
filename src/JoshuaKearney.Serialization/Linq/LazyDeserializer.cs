using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization.Linq {
    internal class LazyDeserializer : BinaryDeserializer, IBinarySerializable {
        private bool isDisposed = false;
        private IBinarySerializable content;
        private ArrayDeserializer deserializer;

        public LazyDeserializer(IBinarySerializable content) {
            this.content = content;
        }

        public override async Task<ArraySegment<byte>> ReadToEndAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(LazyDeserializer));
            }

            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            return await this.deserializer.ReadToEndAsync();
        }

        public override async Task<Stream> GetStreamAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(LazyDeserializer));
            }

            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            return await this.deserializer.GetStreamAsync();
        }

        public override async Task ResetAsync() {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(LazyDeserializer));
            }

            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            await this.deserializer.ResetAsync();
        }

        public override async Task<int> TryReadBytesAsync(ArraySegment<byte> result) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(LazyDeserializer));
            }

            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            return await this.deserializer.TryReadBytesAsync(result);
        }

        public override async Task<ArraySegment<byte>> TryReadBytesAsync(int count) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(LazyDeserializer));
            }

            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            return await this.deserializer.TryReadBytesAsync(count);
        }

        public async Task WriteToAsync(BinarySerializer writer) {
            if (this.isDisposed) {
                throw new ObjectDisposedException(nameof(LazyDeserializer));
            }

            if (this.deserializer == null) {
                await this.InitializeDeserializer();
            }

            await this.deserializer.WriteToAsync(writer);
        }

        private async Task InitializeDeserializer() {
            using (ArraySerializer serial = new ArraySerializer()) {
                await serial.WriteAsync(this.content);
                this.deserializer = new ArrayDeserializer(serial.Array);
                this.content = null;
            }
        }

        public override void Dispose() {
            if (this.isDisposed) {
                return;
            }

            this.isDisposed = true;
            this.deserializer.Dispose();
            this.content = null;

            GC.SuppressFinalize(this);
        }
    }
}