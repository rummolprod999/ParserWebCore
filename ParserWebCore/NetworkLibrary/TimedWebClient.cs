using System;
using System.Net;

namespace ParserWebCore.NetworkLibrary
{
    public class TimedWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = base.GetWebRequest(address);
            wr.Timeout = 600000;
            return wr;
        }
    }

    public class TimedWebClientUa : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            wr.Timeout = 600000;
            wr.UserAgent = "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0";
            return wr;
        }
    }

    public class WebDownload : WebClient
    {
        public int Timeout { get; set; }

        public WebDownload() : this(60000)
        {
        }

        public WebDownload(int timeout)
        {
            Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
            {
                request.Timeout = Timeout;
            }

            return request;
        }
    }
}