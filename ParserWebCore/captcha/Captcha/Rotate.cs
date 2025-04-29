#region

using System;
using System.IO;

#endregion

namespace TwoCaptcha.Captcha
{
    public class Rotate : Captcha
    {
        public Rotate()
        {
            parameters["method"] = "rotatecaptcha";
        }


        public void SetBase64(string base64)
        {
            parameters["body"] = base64;
        }

        public void SetAngle(double angle)
        {
            parameters["angle"] = Convert.ToString(angle).Replace(',', '.');
        }

        public void SetLang(string lang)
        {
            parameters["lang"] = lang;
        }

        public void SetHintText(string hintText)
        {
            parameters["textinstructions"] = hintText;
        }

        public void SetHintImg(string base64)
        {
            parameters["imginstructions"] = base64;
        }

        public void SetHintImg(FileInfo hintImg)
        {
            files["imginstructions"] = hintImg;
        }
    }
}