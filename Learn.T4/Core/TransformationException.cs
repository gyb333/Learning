using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Learn.T4
{
    [Serializable]
    public class TransformationException : Exception
    {
        public TransformationException() { }
        public TransformationException(string message) : base(message) { }
        public TransformationException(string message, Exception inner) : base(message, inner) { }
        protected TransformationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
