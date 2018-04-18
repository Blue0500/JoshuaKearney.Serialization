using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization.Linq {
    public class BinaryNode : IBinarySerializable {
        private BinaryNode parent = null;
        private ArraySegment<byte> content;
        private readonly List<BinaryNode> children = new List<BinaryNode>();

        public string Name { get; }

        public IReadOnlyList<BinaryNode> Children => this.children;

        public BinaryNode Parent {
            get {
                return this.parent;
            }
            set {
                if (this.parent != null && value != null) {
                    throw new InvalidOperationException("This node already has a parent");
                }

                this.parent = value;
            }
        }

        public BinaryNode NextSibling { get; private set; }

        public BinaryNode PreviousSibling { get; private set; }

        public BinaryNode this[int index] {
            get {
                return this.GetChild(index);
            }
        }

        public BinaryNode this[string name] {
            get {
                return this.GetChild(name);
            }
        }

        public BinaryNode(string name, params BinaryNode[] children) {
            this.Name = name;
            this.children = children.ToList();
            this.content = new ArraySegment<byte>(new byte[0]);

            for (int i = 0; i < this.children.Count; i++) {
                this.children[i].Parent = this;

                if (i > 0) {
                    this.children[i - 1].NextSibling = this.children[i];
                    this.children[i].PreviousSibling = this.children[i - 1];
                }

                if (i < this.children.Count - 1) {
                    this.children[i].NextSibling = this.children[i + 1];
                    this.children[i + 1].PreviousSibling = this.children[i];
                }
            }
        }

        public BinaryNode(string name, ArraySegment<byte> content, params BinaryNode[] children) : this(name, children) {
            this.content = content;
        }

        public BinaryNode(string name, IBinarySerializable content, params BinaryNode[] children) : this(name, children) {
            using (var serial = new ArraySerializer()) {
                serial.WriteAsync(content).Wait();
                this.content = serial.Array;
            }
        }

        public BinaryNode(string name, Func<IBinarySerializer, Task> content, params BinaryNode[] children) : this(name, BinarySerializable.Create(content), children) { }

        public IBinaryDeserializer GetContentReader() => new ArrayDeserializer(this.content);

        public IBinarySerializer GetContentWriter() => new BinaryNodeSerializer(this);

        public async Task WriteToAsync(IBinarySerializer writer) {
            await writer.WriteAsync(this.Name);

            if (this.content == null) {
                await writer.WriteSequenceAsync(new byte[0]);
            }
            else {
                await writer.WriteSequenceAsync(this.content);
            }

            await writer.WriteSequenceAsync(this.Children);
        }

        public void AddSiblingAfter(BinaryNode node) {
            node.Parent = this.Parent;
            this.Parent.children.Add(node);

            if (this.NextSibling != null) {
                this.NextSibling.PreviousSibling = node;
            }
            this.NextSibling = node;
        }

        public void AddSiblingBefore(BinaryNode node) {
            node.Parent = this.Parent;
            this.Parent.children.Add(node);

            if (this.PreviousSibling != null) {
                this.PreviousSibling.NextSibling = node;
            }
            this.PreviousSibling = node;
        }

        public void AddChild(BinaryNode node) {
            node.parent = this;
            this.children.Add(node);

            var last = this.children.LastOrDefault();
            if (last != null) {
                last.NextSibling = node;
                node.PreviousSibling = last;
            }
        }

        public bool RemoveChild(BinaryNode node) {
            if (!this.children.Contains(node)) {
                return false;
            }

            if (node.PreviousSibling != null) {
                node.PreviousSibling.NextSibling = node.NextSibling;
            }

            if (node.NextSibling != null) {
                node.NextSibling.PreviousSibling = node.PreviousSibling;
            }

            return this.children.Remove(node);
        }

        public BinaryNode GetChild(string name) {
            return this.children.First(x => x.Name == name);
        }

        public BinaryNode GetChild(int index) {
            return this.children[index];
        }

        public IEnumerable<BinaryNode> AllDecendents() {
            return this.Children.Concat(this.Children.SelectMany(x => x.AllDecendents()));
        }

        private class BinaryNodeSerializer : ArraySerializer {
            private BinaryNode node;

            public BinaryNodeSerializer(BinaryNode node) {
                this.node = node;
            }

            public override void Dispose() {
                if (!this.IsDisposed) {
                    node.content = this.Array;
                }

                base.Dispose();
            }
        }
    }

    public static partial class SerializationExtensions {
        public static async Task<BinaryNode> ReadBinaryNodeAsync(this IBinaryDeserializer reader) {
            string name = await reader.ReadStringAsync();
            var content = await reader.ReadByteSequenceAsync();
            var children = await reader.ReadSequenceAsync(rd => rd.ReadBinaryNodeAsync());

            return new BinaryNode(name, content, children.ToArray());
        }
    }
}