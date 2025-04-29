#region

using System;

#endregion

namespace TwoCaptcha.Exceptions
{
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message)
        {
        }
    }
}