#region

using System;

#endregion

namespace TwoCaptcha.Exceptions
{
    public class TimeoutException : Exception
    {
        public TimeoutException(string message) : base(message)
        {
        }
    }
}