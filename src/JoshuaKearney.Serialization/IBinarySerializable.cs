using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization {
    public interface IBinarySerializable {
        Task WriteToAsync(IBinarySerializer writer);
    }

    public static partial class SerializationExtensions {
        public static IBinarySerializable AsSerializable(this BuilderPotential<IBinarySerializer> potential) {
            return new PotentialSerializable(potential);
        }

        private class PotentialSerializable : IBinarySerializable {
            private readonly BuilderPotential<IBinarySerializer> potential;

            public PotentialSerializable(BuilderPotential<IBinarySerializer> poten) {
                this.potential = poten;
            }

            public Task WriteToAsync(IBinarySerializer writer) {
                return this.potential(writer);
            }
        }
    }
}