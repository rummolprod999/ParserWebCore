#region

using System;
using System.IO;

#endregion

namespace TwoCaptcha.Captcha
{
    public class Grid : Captcha
    {
        public Grid()
        {
            parameters["recaptcha"] = "1";
        }

        public Grid(string filePath) : this(new FileInfo(filePath))
        {
        }

        public Grid(FileInfo file) : this()
        {
            SetFile(file);
        }

        public void SetFile(string filePath)
        {
            SetFile(new FileInfo(filePath));
        }

        public void SetFile(FileInfo file)
        {
            files["file"] = file;
        }

        public void SetBase64(string base64)
        {
            parameters["body"] = base64;
        }

        public void SetRows(int rows)
        {
            parameters["recaptcharows"] = Convert.ToString(rows);
        }

        public void SetCols(int cols)
        {
            parameters["recaptchacols"] = Convert.ToString(cols);
        }

        public void SetPreviousId(int previousId)
        {
            parameters["previousID"] = Convert.ToString(previousId);
        }

        public void SetCanSkip(bool canSkip)
        {
            parameters["can_no_answer"] = canSkip ? "1" : "0";
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