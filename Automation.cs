using System;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace FormFieldTesting
{
    public class RobustFormTester
    {
        private IWebDriver driver;
        private WebDriverWait wait;
        private const int TimeoutSeconds = 10;

        public RobustFormTester()
        {
            InitializeDriver();
        }

        private void InitializeDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            
            driver = new ChromeDriver(options);
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TimeoutSeconds));
        }

        private IWebElement FindElementRobustly(List<By> selectors, string fieldName)
        {
            foreach (var selector in selectors)
            {
                try
                {
                    var element = wait.Until(ExpectedConditions.ElementIsVisible(selector));
                    Console.WriteLine($"✓ Found {fieldName} using: {selector}");
                    return element;
                }
                catch (WebDriverTimeoutException)
                {
                    Console.WriteLine($"✗ Failed to find {fieldName} using: {selector}");
                    continue;
                }
            }
            
            throw new NoSuchElementException($"Could not find {fieldName} using any of the provided selectors");
        }

        public bool TestFirstNameField()
        {
            Console.WriteLine("\n=== Testing First Name Field ===");
            
            var firstNameSelectors = new List<By>
            {
                By.CssSelector("input[placeholder='Name']:first-of-type"),
                By.XPath("//label[contains(text(), 'First Name')]/following-sibling::input | //label[contains(text(), 'First Name')]/..//input"),
                By.CssSelector("input[name*='first'], input[name*='First'], input[id*='first'], input[id*='First']"),
                By.XPath("//input[contains(@placeholder, 'Name') and not(contains(@placeholder, 'Sur'))]"),
                By.XPath("//div[contains(., 'First Name')]//input"),
                By.CssSelector("form input[type='text']:first-of-type"),
                By.XPath("(//input[@type='text'])[1]")
            };

            try
            {
                var firstNameField = FindElementRobustly(firstNameSelectors, "First Name");
                
                string testValue = "John";
                firstNameField.Clear();
                firstNameField.SendKeys(testValue);
                
                string actualValue = firstNameField.GetAttribute("value");
                bool isSuccess = actualValue.Equals(testValue);
                
                Console.WriteLine($"Test Value: '{testValue}', Actual Value: '{actualValue}'");
                Console.WriteLine($"First Name Field Test: {(isSuccess ? "PASSED" : "FAILED")}");
                
                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"First Name Field Test: FAILED - {ex.Message}");
                return false;
            }
        }

        public bool TestEmailField()
        {
            Console.WriteLine("\n=== Testing Email Field ===");
            
            var emailSelectors = new List<By>
            {
                By.CssSelector("input[type='email']"),
                By.CssSelector("input[placeholder*='Email'], input[placeholder*='email']"),
                By.XPath("//label[contains(text(), 'Email')]/following-sibling::input | //label[contains(text(), 'Email')]/..//input"),
                By.CssSelector("input[name*='email'], input[name*='Email'], input[id*='email'], input[id*='Email']"),
                By.XPath("//div[contains(., 'Email')]//input"),
                By.XPath("//input[contains(@placeholder, 'Email') or contains(@name, 'email') or contains(@id, 'email')]"),
                By.XPath("//input[contains(@class, 'email') or contains(@data-field, 'email')]")
            };

            try
            {
                var emailField = FindElementRobustly(emailSelectors, "Email");
                
                string testEmail = "test@example.com";
                emailField.Clear();
                emailField.SendKeys(testEmail);
                
                string actualValue = emailField.GetAttribute("value");
                bool isSuccess = actualValue.Equals(testEmail);
                
                Console.WriteLine($"Test Email: '{testEmail}', Actual Value: '{actualValue}'");
                Console.WriteLine($"Email Field Test: {(isSuccess ? "PASSED" : "FAILED")}");
                
                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email Field Test: FAILED - {ex.Message}");
                return false;
            }
        }

        public bool TestGenderField()
        {
            Console.WriteLine("\n=== Testing Gender Field ===");
            
            var genderSelectors = new List<By>
            {
                By.XPath("//input[@type='radio' and (@value='Male' or @value='male' or following-sibling::text()[contains(., 'Male')])]"),
                By.CssSelector("input[type='radio'][value*='Male'], input[type='radio'][value*='male']"),
                By.XPath("//label[contains(text(), 'Male')]/input[@type='radio'] | //label[contains(text(), 'Male')]/..//input[@type='radio']"),
                By.XPath("//div[contains(., 'Gender')]//input[@type='radio'][1]"),
                By.XPath("//fieldset//input[@type='radio'][1] | //div[contains(@class, 'gender')]//input[@type='radio'][1]"),
                By.CssSelector("input[type='radio']:first-of-type"),
                By.XPath("(//input[@type='radio'])[1]")
            };

            try
            {
                var genderRadio = FindElementRobustly(genderSelectors, "Gender (Male)");
                
                bool wasSelected = genderRadio.Selected;
                
                if (!wasSelected)
                {
                    genderRadio.Click();
                    Thread.Sleep(500);
                }
                
                bool isNowSelected = genderRadio.Selected;
                bool isSuccess = isNowSelected;
                
                Console.WriteLine($"Was Selected: {wasSelected}, Is Now Selected: {isNowSelected}");
                Console.WriteLine($"Gender Field Test: {(isSuccess ? "PASSED" : "FAILED")}");
                
                return isSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Gender Field Test: FAILED - {ex.Message}");
                return false;
            }
        }

        public void RunAllTests(string url)
        {
            try
            {
                Console.WriteLine($"Navigating to: {url}");
                driver.Navigate().GoToUrl(url);
                
                wait.Until(ExpectedConditions.ElementExists(By.TagName("form")));
                
                Console.WriteLine("Page loaded successfully. Starting field tests...");
                
                bool firstNameResult = TestFirstNameField();
                bool emailResult = TestEmailField();
                bool genderResult = TestGenderField();
                
                Console.WriteLine("\n" + new string('=', 50));
                Console.WriteLine("TEST SUMMARY");
                Console.WriteLine(new string('=', 50));
                Console.WriteLine($"First Name Field: {(firstNameResult ? "✓ PASSED" : "✗ FAILED")}");
                Console.WriteLine($"Email Field:      {(emailResult ? "✓ PASSED" : "✗ FAILED")}");
                Console.WriteLine($"Gender Field:     {(genderResult ? "✓ PASSED" : "✗ FAILED")}");
                
                int passedCount = (firstNameResult ? 1 : 0) + (emailResult ? 1 : 0) + (genderResult ? 1 : 0);
                Console.WriteLine($"\nOverall Result: {passedCount}/3 tests passed");
                
                if (passedCount == 3)
                {
                    Console.WriteLine("🎉 All tests PASSED! The form fields are working correctly.");
                }
                else
                {
                    Console.WriteLine("⚠️  Some tests FAILED. Please check the form implementation.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test execution failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var tester = new RobustFormTester();
            
            try
            {
                string formUrl = "https://your-form-url.com";
                
                Console.WriteLine("Robust Form Field Testing System");
                Console.WriteLine("================================");
                Console.WriteLine("This program tests form fields using multiple identification strategies");
                Console.WriteLine("to ensure tests remain stable even when UI elements change position or properties.\n");
                
                Console.WriteLine("Note: Replace 'formUrl' variable with your actual form URL before running.");
                
                tester.RunAllTests(formUrl);
                
                Console.WriteLine("\nTest strategies implemented:");
                Console.WriteLine("1. Primary: Specific selectors (placeholder, labels, types)");
                Console.WriteLine("2. Fallback: Flexible attribute-based selectors");
                Console.WriteLine("3. Last resort: Position-based selectors");
                Console.WriteLine("\nThis multi-layered approach ensures maximum test stability!");
            }
            finally
            {
                tester.Dispose();
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
