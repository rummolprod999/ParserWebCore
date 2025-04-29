#region

using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

#endregion

namespace ParserWebCore.Parser
{
    public interface Auth
    {
        void Auth(ChromeDriver driver, WebDriverWait wait);
    }
}