using System;

namespace Laobian.Common.Blog
{
    /// <summary>
    /// Exception occured during parsing post
    /// </summary>
    public class PostParseException : Exception
    {
        public PostParseException()
        {
        }

        public PostParseException(string message) : base(message)
        {
        }

        public PostParseException(string message, Exception innerException):base(message, innerException) { }
    }
}
