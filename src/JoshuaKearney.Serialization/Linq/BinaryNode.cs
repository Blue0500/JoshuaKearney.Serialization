using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JoshuaKearney.Serialization.Linq {
    public class BinaryNode : IBinarySerializable {
        private BinaryNode parent = null;
        private List<BinaryNode> children = new List<BinaryNode>();

        public string Name { get; }

        public IReadOnlyCollection<BinaryNode> Children => this.children;

        public BinaryDeserializer Content { get; }

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

        public BinaryNode(string name, params BinaryNode[] children) {
            this.Name = name;
            this.children = children.ToList();

            for (int i = 0; i < children.Length; i++) {
                children[i].Parent = this;

                if (i > 0) {
                    children[i - 1].NextSibling = children[i];
                }

                if (i < children.Length - 1) {
                    children[i].NextSibling = children[i + 1];
                }
            }
        }

        public BinaryNode(string name, BinaryDeserializer content, params BinaryNode[] children) : this(name, children) {
            this.Content = content;
        }

        public BinaryNode(string name, BuilderPotential<BinarySerializer> content, params BinaryNode[] children) : this(name, children) {
            this.Content = new LazyDeserializer(content.AsSerializable());
        }

        public async Task WriteToAsync(BinarySerializer writer) {
            await writer.WriteAsync(this.Name);
            await writer.WriteSequenceAsync(await this.Content.ReadToEndAsync());
            await writer.WriteSequenceAsync(this.Children, item => item.WriteToAsync(writer));
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

        public IEnumerable<BinaryNode> AllDecendents() {
            return this.Children.Concat(this.Children.SelectMany(x => x.AllDecendents()));
        }
    }

    public static partial class SerializationExtensions {
        public static async Task<BinaryNode> ReadBinaryNodeAsync(this BinaryDeserializer reader) {
            string name = await reader.ReadStringAsync();
            var content = new ArrayDeserializer(await reader.ReadByteSequenceAsync());
            var children = await reader.ReadSequenceAsync(reader.ReadBinaryNodeAsync);

            return new BinaryNode(name, content, children.ToArray());
        }
    }
}