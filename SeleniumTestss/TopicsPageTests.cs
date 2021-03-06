﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using OpenQA.Selenium.Support.UI;
using System.Threading.Tasks;

namespace SeleniumTestss
{
    [TestClass]
    public class TopicsPageTests
    {
        private TestContext testContextInstance;
        private IWebDriver driver;
        private string appURL;

        [TestMethod]
        [TestCategory("Chrome")]
        public void DataSciencsePageServes()
        {
            appURL = "https://www.careercircle.com/Topic/Data-Science";
            driver.Navigate().GoToUrl(appURL + "/");
            Assert.IsTrue(driver.PageSource.Contains("Data Science Courses"), "Verified Data Science topics page");
        }

        [TestMethod]
        [TestCategory("Chrome")]
        public void CyberSecurityPageServes()
        {
            appURL = "https://www.careercircle.com/Topic/Cyber-Security";
            driver.Navigate().GoToUrl(appURL + "/");
            Assert.IsTrue(driver.PageSource.Contains("Cyber Security Courses"), "Verfied Cyber Security topics page");
        }


        [TestMethod]
        [TestCategory("Chrome")]
        public void FullStackPageServes()
        {
            appURL = "https://www.careercircle.com/Topic/MEAN-Stack";
            driver.Navigate().GoToUrl(appURL + "/");
            Assert.IsTrue(driver.PageSource.Contains("Full Stack Courses"), "Verified Cyber Security topics page");
        }







        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        [TestInitialize()]
        public void SetupTest()
        {
            appURL = "https://careercirclewebapp.azurewebsites.net/home/index";

            string browser = "Chrome";
            switch (browser)
            {
                case "Chrome":
                    driver = new ChromeDriver();
                    break;
                case "Firefox":
                    driver = new FirefoxDriver();
                    break;
                case "IE":
                    driver = new InternetExplorerDriver();
                    break;
                default:
                    driver = new ChromeDriver();
                    break;
            }

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);


        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            driver.Quit();
        }
    }
}
