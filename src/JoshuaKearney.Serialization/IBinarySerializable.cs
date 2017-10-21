using System;
using System.Collections.Generic;
using System.Text;

namespace JoshuaKearney.Serialization {
    public interface IBinarySerializable {
        void WriteTo(IBinarySerializer writer);
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

            public void WriteTo(IBinarySerializer writer) {
                this.potential(writer);
            }
        }
    }
}