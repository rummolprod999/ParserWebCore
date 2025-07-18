#region

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace TwoCaptcha.Captcha
{
    public abstract class Captcha
    {
        protected Dictionary<string, FileInfo> files;

        protected Dictionary<string, string> parameters;

        public Captcha()
        {
            parameters = new Dictionary<string, string>();
            files = new Dictionary<string, FileInfo>();
        }

        public string Id { get; set; }
        public string Code { get; set; }

        public void SetProxy(string type, string uri)
        {
            parameters["proxy"] = uri;
            parameters["proxytype"] = type;
        }

        public void SetSoftId(int softId)
        {
            parameters["soft_id"] = Convert.ToString(softId);
        }

        public void SetCallback(string callback)
        {
            parameters["pingback"] = callback;
        }

        public Dictionary<string, string> GetParameters()
        {
            var parameters = new Dictionary<string, string>(this.parameters);

            if (!parameters.ContainsKey("method"))
            {
                if (parameters.ContainsKey("body"))
                {
                    parameters["method"] = "base64";
                }
                else
                {
                    parameters["method"] = "post";
                }
            }

            return parameters;
        }

        public Dictionary<string, FileInfo> GetFiles()
        {
            return new Dictionary<string, FileInfo>(files);
        }
    }
}