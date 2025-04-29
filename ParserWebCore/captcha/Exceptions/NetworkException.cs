#region

using System;

#endregion

namespace TwoCaptcha.Exceptions
{
    public class NetworkException : Exception
    {
        public NetworkException(string message) : base(message)
        {
        }
    }
}