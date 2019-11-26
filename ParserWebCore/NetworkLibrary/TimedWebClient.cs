using System;
using System.IO;
using System.Net;
using System.Text;

namespace ParserWebCore.NetworkLibrary
{
    public class TimedWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Timeout = 600000;
                return wr;
            }

            return null;
        }
    }

    public class TimedWebClientUa : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            if (wr != null)
            {
                wr.Timeout = 600000;
                wr.UserAgent = "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0";
                return wr;
            }

            return null;
        }
    }
    
    public class TimedWebSber : WebClient
    {
        private readonly int page;
        public TimedWebSber(int page)
        {
            this.page = page;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = (HttpWebRequest) base.GetWebRequest(address);
            if (wr != null)
            {
                
                wr.Method = "POST";
                wr.Headers.Add("x-requested-with", "XMLHttpRequest");
                wr.ContentType = "application/json";
                wr.Headers.Add("Sec-Fetch-Site", "same-origin");
                wr.Headers.Add("Sec-Fetch-Mode", "cors");
                wr.Headers.Add("Origin", "https://sberb2b.ru");
                wr.Headers.Add("Referer", "https://sberb2b.ru/request/public-requests");
                wr.Headers.Add("Cookie", "SFSESSID=oaqs0kfphnf9od0casnpirvtrp; SL_GWPT_Show_Hide_tmp=1; SL_wptGlobTipTmp=1");
                wr.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                using (var streamWriter = new StreamWriter(wr.GetRequestStream()))
                {
                    
                    var json = "{}";
                    var byteArray = Encoding.UTF8.GetBytes(json);
                    wr.ContentLength = byteArray.Length;
                    streamWriter.Write(byteArray);
                    streamWriter.Flush();
                }
                wr.Timeout = 600000;
                wr.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.99 Safari/537.36 Vivaldi/2.9.1705.41";
                
                return wr;
            }

            return null;
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