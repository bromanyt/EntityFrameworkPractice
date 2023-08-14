using System;
namespace WebRetail.Models
{
    public class RetailException : ApplicationException
    {
        public RetailException() : base() { }

        public RetailException(string message) : base(message) { }
    }
}

