using System;
using System.Collections.Generic;
using System.Text;

namespace JoshuaKearney.Serialization {
    public class SerializationException : Exception {
        public SerializationException() {
        }

        public SerializationException(string message) : base(message) {
        }

        public SerializationException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}