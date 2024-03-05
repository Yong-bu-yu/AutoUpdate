using System;
using System.Collections.Generic;
using System.Text;

namespace AutoUpdate.Exceptions
{
    public class AutoUpdateException : Exception
    {
        public AutoUpdateException(string message = "") : base(message)
        {   }
    }
}
