using System;
using System.Runtime.Serialization;

namespace LabFusion.Data
{
    [Serializable]
    public class DataCorruptionException : Exception
    {
        public DataCorruptionException(string message) : base(message) { }
        public DataCorruptionException(string message, Exception inner) : base(message, inner) { }
        protected DataCorruptionException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}

