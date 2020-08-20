using System;
using System.Runtime.Serialization;

namespace Bard.Db.Internal
{
    internal class BardException: Exception
    {
        protected BardException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal BardException(string message) : base(message)
        {
        }
    }
}