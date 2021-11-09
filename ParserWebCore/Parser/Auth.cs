using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace ParserWebCore.Parser
{
    public interface Auth
    {
        void Auth(ChromeDriver driver, WebDriverWait wait);
    }
}