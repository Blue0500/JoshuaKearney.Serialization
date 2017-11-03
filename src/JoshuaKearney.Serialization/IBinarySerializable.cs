using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public interface IBinarySerializable {
        Task WriteToAsync(BinarySerializer writer);
    }

    public static partial class SerializationExtensions {
        public static IBinarySerializable AsSerializable(this BuilderPotential<BinarySerializer> potential) {
            return new PotentialSerializable(potential);
        }

        private class PotentialSerializable : IBinarySerializable {
            private readonly BuilderPotential<BinarySerializer> potential;

            public PotentialSerializable(BuilderPotential<BinarySerializer> poten) {
                this.potential = poten;
            }

            public Task WriteToAsync(BinarySerializer writer) {
                return this.potential(writer);
            }
        }
    }
}