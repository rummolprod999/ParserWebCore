namespace TwoCaptcha.Captcha
{
    public class AudioCaptcha : Captcha
    {
        public AudioCaptcha()
        {
            parameters["method"] = "audio";
        }

        public void SetBase64(string base64)
        {
            parameters["body"] = base64;
        }

        public void SetLang(string lang)
        {
            parameters["lang"] = lang;
        }
    }
}