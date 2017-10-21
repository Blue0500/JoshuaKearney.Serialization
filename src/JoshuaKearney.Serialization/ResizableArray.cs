using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JoshuaKearney.Serialization {
    internal class ResizableArray<T> : IEnumerable<T>, ICollection<T>, IReadOnlyCollection<T>, IList<T>, IReadOnlyList<T> {
        private T[] array;

        public int OffSet { get; private set; }

        private int version = 0;

        public ref T[] Array => ref this.array;

        public int Count { get; private set; } = 0;

        public bool IsReadOnly => false;

        public T this[int index] {
            get => this.Array[this.OffSet + index];
            set => this.Array[this.OffSet + index] = value;
        }

        public ResizableArray() {
            this.Clear();
        }

        public void AddRange(IEnumerable<T> bytes) {
            foreach (var item in bytes) {
                this.Add(item);
            }
        }

        public void AddRange(T[] array) {
            this.AddRange(new ArraySegment<T>(array));
        }

        public void AddRange(ArraySegment<T> array) {
            if (this.OffSet + this.Count + array.Count > this.Array.Length - 1) {
                T[] newArray = new T[this.array.Length + this.Count + array.Count];
                this.array.CopyTo(newArray, 0);
                this.array = newArray;
            }

            System.Array.ConstrainedCopy(array.Array, array.Offset, this.array, this.OffSet + this.Count, array.Count);
            this.Count += array.Count;
        }

        public ResizableArray(IEnumerable<T> enumerable) {
            foreach (var item in enumerable) {
                this.Add(item);
            }
        }

        public void Add(T item) {
            this.version++;
            int endPos = this.OffSet + this.Count;

            if (endPos >= this.array.Length) {
                this.ResizeEnd();
            }

            this.array[endPos] = item;
            this.Count++;
        }

        public void Insert(int index, T item) {
            this.version++;

            if (index == 0) {
                if (this.OffSet <= 0) {
                    this.ResizeFront();
                }

                this.array[--this.OffSet] = item;
            }
            else {
                T next = item;

                for (int i = index; i < this.Count + 1; i++) {
                    T temp = this[i];
                    this[i] = next;
                    next = temp;
                }
            }

            this.Count++;
        }

        private void ResizeEnd() {
            T[] newArray = new T[this.array.Length + this.Count];
            this.array.CopyTo(newArray, 0);
            this.array = newArray;
        }

        private void ResizeFront() {
            T[] newArray = new T[this.array.Length + this.Count];
            this.array.CopyTo(newArray, this.Count);
            this.OffSet = this.Count;

            this.array = newArray;
        }

        public IEnumerator<T> GetEnumerator() => new ResizableArrayEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int IndexOf(T item) {
            for (int i = 0; i < this.Count; i++) {
                if (this[i].Equals(item)) {
                    return i;
                }
            }

            return -1;
        }

        public void RemoveAt(int index) {
            T prev = default(T);

            for (int i = this.Count - 1; i >= index; i--) {
                T temp = this[i];
                this[i] = prev;
                prev = temp;
            }

            this.Count--;
        }

        public void Clear() {
            this.array = new T[16];
            this.OffSet = 8;
            this.Count = 0;
        }

        public bool Contains(T item) {
            return this.IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            this.Array.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item) {
            int index = this.IndexOf(item);
            if (index < 0) {
                return false;
            }
            else {
                this.RemoveAt(index);
                return true;
            }
        }

        public ArraySegment<T> ToArraySegment() {
            return new ArraySegment<T>(this.Array, this.OffSet, this.Count);
        }

        private class ResizableArrayEnumerator : IEnumerator<T> {
            private ResizableArray<T> list;
            private int pos = 0;
            private int version;

            public T Current { get; private set; } = default(T);

            object IEnumerator.Current => this.Current;

            public ResizableArrayEnumerator(ResizableArray<T> list) {
                this.list = list;
                this.version = list.version;
            }

            public void Dispose() { }

            public bool MoveNext() {
                if (this.version != this.list.version) {
                    throw new InvalidOperationException();
                }

                if (this.pos >= this.list.Count) {
                    this.Current = default(T);
                    return false;
                }
                else {
                    this.Current = this.list.Array[this.list.OffSet + this.pos++];
                    return true;
                }
            }

            public void Reset() {
                if (this.version != this.list.version) {
                    throw new InvalidOperationException();
                }

                this.pos = 0;
            }
        }
    }
}