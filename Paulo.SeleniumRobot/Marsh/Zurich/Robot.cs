using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;

namespace Paulo.SeleniumRobot.Marsh.Zurich
{
    public static class Robot
    {
        static ChromeOptions chromeOptions = new ChromeOptions();
        static string _DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";

        public static void GetFileFromZurich(string path, string[] credentials)
        {
            IWebDriver driver = new ChromeDriver(chromeOptions);

            driver.Url = credentials[2];

            driver.Manage().Window.Maximize();

            try
            {
                driver.FindElement(By.XPath(@"//*[@id='content']/tbody/tr[5]/td[2]/a/b")).Click();
            }
            catch (Exception){}

            driver.FindElement(By.Id("form_username")).Clear();

            driver.FindElement(By.Id("form_username")).SendKeys(credentials[0]);

            driver.FindElement(By.Id("form_password")).SendKeys(credentials[1]);

            driver.FindElement(By.Id("submit_button")).Click();

            var divPackageBox = driver.FindElement(By.ClassName("package_box"));

            var tablePackageBox = divPackageBox.FindElement(By.CssSelector("table"));

            var rowsPackagebox = tablePackageBox.FindElements(By.CssSelector("tr"));

            foreach (var tr in rowsPackagebox)
            {
                var td = tr.FindElements(By.CssSelector("td"));
                if (td.Count > 1)
                {
                    td[1].FindElement(By.CssSelector("a")).Click();
                    var fileName = td[0].FindElements(By.CssSelector("input"))[1].GetAttribute("value");
                    File.Move(_DownloadPath + fileName, path + fileName);
                }
            }
            driver.Close();
        }

    }
}
