#region

using System;
using System.Net;
using System.Net.Http;

#endregion

namespace ParserWebCore.chrome
{
    internal static class Http
    {
        private static readonly Lazy<HttpClient> HttpClientLazy = new Lazy<HttpClient>(() =>
        {
            var handler = new HttpClientHandler
            {
                UseCookies = false
            };

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            return new HttpClient(handler, true);
        });

        public static HttpClient Client => HttpClientLazy.Value;
    }
}