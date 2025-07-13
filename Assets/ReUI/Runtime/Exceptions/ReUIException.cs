using System;

namespace Abyse.ReUI
{
    public class ReUIException : Exception
    {
        public ReUIException(string message) : base(message)
        {
        }

        public ReUIException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}