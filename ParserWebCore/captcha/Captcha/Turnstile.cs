namespace TwoCaptcha.Captcha
{
    public class Turnstile : Captcha
    {
        public Turnstile()
        {
            parameters["method"] = "turnstile";
        }

        public void SetSiteKey(string siteKey)
        {
            parameters["sitekey"] = siteKey;
        }

        public void SetUrl(string url)
        {
            parameters["pageurl"] = url;
        }
    }
}