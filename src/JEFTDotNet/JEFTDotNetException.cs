using System;
using System.Collections.Generic;
using System.Text;

namespace JEFTDotNet
{
    public class JEFTDotNetException : Exception
    {
        public JEFTDotNetException(string message)
            : base(message)
        {
        }
    }
}
