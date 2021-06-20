using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

using System;
using System.IO;
using System.Collections.Generic;

namespace instagram_follower_bot
{
    class Instagram
    {
        private IWebDriver driver;
        private readonly String pathChromeProfile = @"C:\Users\Alan\AppData\Local\Google\Chrome\User Data";
        private readonly String pathChromeDriver = @"C:\Users\Alan\Documents\instagram-follower-bot\infra\";

        private readonly String pathFollowList = @"C:\Users\Alan\Documents\instagram-follower-bot\lists\follow.txt";
        private readonly String pathFollowingList = @"C:\Users\Alan\Documents\instagram-follower-bot\lists\following.txt";
        private readonly String pathRequestedList = @"C:\Users\Alan\Documents\instagram-follower-bot\lists\requested.txt";
        private readonly String pathhFailedList = @"C:\Users\Alan\Documents\instagram-follower-bot\lists\failed.txt";

        private readonly String baseUrl = "https://www.instagram.com/";
        private readonly String login = "";
        private readonly String password = "";
        private readonly int followLimit = 20;

        public void LoadTimeout()
        {
            System.Threading.Thread.Sleep(3000);
        }

        [SetUp]
        public void StartChrome()
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument($"--user-data-dir={pathChromeProfile}");

            driver = new ChromeDriver(pathChromeDriver, chromeOptions);
            driver.Manage().Window.Maximize();
        }

        [Test]
        public void OpenInstagram()
        {
            // Login();
            Follow();
        }

        private void Login()
        {
            driver.Url = Path.Combine(baseUrl, "accounts/login");

            LoadTimeout();

            IWebElement usernameInput = driver.FindElement(By.XPath(".//input[@name='username']"));
            usernameInput.SendKeys(login);

            IWebElement passwordInput = driver.FindElement(By.XPath(".//input[@name='password']"));
            passwordInput.SendKeys(password);

            IWebElement loginButton = driver.FindElement(By.XPath(".//div[text()='Entrar']"));
            loginButton.Click();

            LoadTimeout();
        }

        private void Follow()
        {
            List<string> followList = new List<string>(File.ReadAllLines(pathFollowList));
            List<string> followingList = new List<string>();
            List<string> requestedList = new List<string>();
            List<string> failedList = new List<string>();

            string status = string.Empty;

            for (int i = 0; i < followLimit; i++)
            {
                status = OpenProfile(followList[i]);

                if (status == "following")
                {
                    followingList.Add(followList[i]);
                }
                else if (status == "requested")
                {
                    requestedList.Add(followList[i]);
                }
                else
                {
                    failedList.Add(followList[i]);
                }
            }

            SaveLog(followList, followingList, requestedList, failedList);
        }

        private string OpenProfile(string username)
        {
            driver.Url = Path.Combine(baseUrl, username);

            LoadTimeout();

            string status = "failed";

            try
            {
                IWebElement followButton = driver.FindElement(By.XPath(".//button[text()='follow']"));

                if (followButton.Displayed)
                {
                    followButton.Click();

                    LoadTimeout();

                    status = Following();
                }
            }
            catch
            {
                status = Following();
            }

            return status;
        }

        private string Following()
        {
            string status = "failed";

            try
            {
                IWebElement sendmessageButton = driver.FindElement(By.XPath(".//div[text()='Enviar mensagem']"));

                if (sendmessageButton.Displayed)
                {
                    status = "following";
                }
            }
            catch
            {
                try
                {
                    IWebElement requestedButton = driver.FindElement(By.XPath(".//button[text()='Solicitado']"));

                    if (requestedButton.Displayed)
                    {
                        status = "requested";
                    }
                }
                catch
                {
                    status = "failed";
                }
            }

            return status;
        }

        private void SaveLog(List<string> followList, List<string> followingList, List<string> requestedList, List<string> failedList)
        {
            followList.RemoveAll(username => followingList.Contains(username));
            followList.RemoveAll(username => requestedList.Contains(username));
            followList.RemoveAll(username => failedList.Contains(username));

            File.Delete(pathFollowList);
            File.AppendAllLines(pathFollowList, followList);

            File.AppendAllLines(pathFollowingList, followingList);
            File.AppendAllLines(pathRequestedList, requestedList);
            File.AppendAllLines(pathhFailedList, failedList);
        }

        [TearDown]
        public void CloseChrome()
        {
            driver.Quit();
        }
    }
}