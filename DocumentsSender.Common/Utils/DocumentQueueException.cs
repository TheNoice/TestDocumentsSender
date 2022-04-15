using System;

namespace DocumentsSender.Common.Utils
{
    public class DocumentQueueException : Exception
    {
        public DocumentQueueException(string message) : base(message)
        {

        }
    }
}
