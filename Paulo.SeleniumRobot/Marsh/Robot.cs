using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using Paulo.Console.model.emailUtil;
using System.Text.RegularExpressions;
using System.Threading;

namespace Paulo.SeleniumRobot.Marsh
{
    public static class Robot
    {
        static ChromeOptions chromeOptions = new ChromeOptions();

        static string _DownloadPath = @"C:\Users\mt11201\Documents\Especificações\Marsh Risk\Testes\E-mails Recebidos\Pedidos";

        static string _OwPassword;
        static string _OwUser;
        static string _Url = "https://owg.okta.com/";

        private static void SetChromeOptions()
        {
            chromeOptions.AddUserProfilePreference("disable-popup-blocking", true);

            chromeOptions.AddUserProfilePreference("download.default_directory", _DownloadPath);
            //Turns off download prompt
            chromeOptions.AddUserProfilePreference("download.prompt_for_download", false);

            //To Turns off multiple download warning
            chromeOptions.AddUserProfilePreference("default_content_settings.popups", 0);
            chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.automatic_downloads", 1);
        }

        public static void GetFileFromOWEXCHANGE()
        {
            SetChromeOptions();

            GetCredentials();

            IWebDriver driver = new ChromeDriver(chromeOptions);

            driver.Url = _Url;

            driver.Manage().Window.Maximize();

            driver.FindElement(By.Id("okta-signin-username")).SendKeys(_OwUser);
            driver.FindElement(By.Id("okta-signin-password")).SendKeys(_OwPassword);
            driver.FindElement(By.Id("okta-signin-submit")).Click();

            Thread.Sleep(5000);

            driver.FindElement(By.CssSelector("#form60 > div.o-form-content.o-form-theme.clearfix > div.o-form-fieldset-container > a")).Click();

            Thread.Sleep(5000);

            var code = GetVerificationCode();

            driver.FindElement(By.Id("input69")).SendKeys(code);

            driver.FindElement(By.XPath("//*[@id='form60']/div[2]/input")).Click();

            Thread.Sleep(5000);

            //
            driver.Url = "https://owg.okta.com/home/oliverwyman_statetax_1/0oattp2yrzRBnEyQw2p6/alnttsanjaqwNo6ik2p6?fromHome=true";

            driver.FindElement(By.XPath("//*[@id='cabinets']/div[1]/ul/li[1]/span/a")).Click();

            //driver.FindElement(By.Id("search")).SendKeys(email.Subject);

            //driver.FindElement(By.Id("btnsearch")).Click();

            Thread.Sleep(1000);

            driver.FindElement(By.Id("fall_1")).Click();

            int qntArquivos = Convert.ToInt32(driver.FindElement(By.Id("selected_no")).Text);

            for (int i = 1; i <= qntArquivos; i++)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript(string.Format("getfile('fx_1_{0}');", i));
                Thread.Sleep(1000);
            }

            driver.Close();
        }

        private static string GetVerificationCode()
        {
            EmailConfiguration eConfig = new EmailConfiguration() { Port = 993, Server = "outlook.office365.com", Username = "mt11201@interfile.com.br", Password = "interfile@071115" };
            EmailService _em = new EmailService(eConfig);

            EmailMessage email = null;

            do
            {
                email = _em.GetAllJunk().LastOrDefault();
            } while (email == null);

            return Regex.Match(email.Content, "\\d{6}").ToString();
        }

        private static void GetCredentials()
        {
            _OwPassword = ConfigurationManager.AppSettings["OwPassword"].ToString();
            _OwUser = ConfigurationManager.AppSettings["OwUser"].ToString();
        }

        private static void ChangePassword()
        {
            var texto = File.ReadAllText("Paulo.WinForm.exe.config");

            var newPassword = RandomPassword();

            texto = texto.Replace(_OwPassword, newPassword);

            File.WriteAllText("Paulo.WinForm.exe.config", texto);
        }

        private static string RandomPassword()
        {
            Random random = new Random();

            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, 8).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
