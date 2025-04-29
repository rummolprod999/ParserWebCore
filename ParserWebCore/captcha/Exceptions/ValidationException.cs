#region

using System;

#endregion

namespace TwoCaptcha.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}