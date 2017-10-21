using System;
using System.Collections.Generic;
using System.Text;

namespace JoshuaKearney.Serialization {
    public interface IBinarySerializable {
        void WriteTo(IBinarySerializer writer);
    }
}