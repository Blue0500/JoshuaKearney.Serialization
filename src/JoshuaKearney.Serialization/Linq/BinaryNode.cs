using System;
using System.Collections.Generic;
using System.Text;

namespace JoshuaKearney.Serialization.Linq {
    public class BinaryNode {
        public BinaryNode Parent { get; }

        public BinaryNode NextSibling { get; }

        public BinaryNode PreviousSibling { get; }

        public ICollection<BinaryNode> Children { get; }

        public IBinaryDeserializer Content { get; }
    }
}