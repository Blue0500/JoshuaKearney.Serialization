using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public interface IBinarySerializable {
        Task WriteToAsync(IBinarySerializer writer);
    }

    public static class BinarySerializable {
        public static IBinarySerializable Create(Func<IBinarySerializer, Task> func) {
            return new PotentialSerializable(func);
        }

        public static IBinarySerializable AppendTo(this IBinarySerializable first, IBinarySerializable second) {
            return Create(async wr => {
                await wr.WriteAsync(second);
                await wr.WriteAsync(first);
            });
        }

        public static IBinarySerializable PrependTo(this IBinarySerializable first, IBinarySerializable second) {
            return second.AppendTo(first);
        }

        public static IBinarySerializable AppendTo(this IBinarySerializable first, Func<IBinarySerializer, Task> second) {
            return first.AppendTo(Create(second));
        }

        public static IBinarySerializable PrependTo(this IBinarySerializable first, Func<IBinarySerializer, Task> second) {
            return first.PrependTo(Create(second));
        }

        public static IBinarySerializable CompressGzip(this IBinarySerializable data) {
            return Create(async wr => {
                using (var comp = await wr.CompressGzipAsync()) {
                    await comp.WriteAsync(data);
                }
            });
        }

        public static IBinarySerializable CompressDeflate(this IBinarySerializable data) {
            return Create(async wr => {
                using (var comp = await wr.CompressDeflateAsync()) {
                    await comp.WriteAsync(data);
                }
            });
        }

        public static IBinarySerializable EncryptAes(this IBinarySerializable data, byte[] key) {
            return Create(async wr => {
                using (var comp = await wr.EncryptAesAsync(key)) {
                    await comp.WriteAsync(data);
                }
            });
        }

        private class PotentialSerializable : IBinarySerializable {
            private readonly Func<IBinarySerializer, Task> potential;

            public PotentialSerializable(Func<IBinarySerializer, Task> poten) {
                this.potential = poten;
            }

            public Task WriteToAsync(IBinarySerializer writer) {
                return this.potential(writer);
            }
        }
    }
}