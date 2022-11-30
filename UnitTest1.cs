using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Input;
using FlaUI.Core.Logging;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using NUnit.Framework.Internal;

// This uses the NuGet package FlaUI.UIA3.Signed (4.0.0)
// Also, use chocolatey to install FlaUIInspect (choco install flauinspect), then run from cmd flauinspect
// Set the mode in inspect to show xpath.
// YouTube video series to help: https://www.youtube.com/watch?v=qPhfXR8vDME&list=PLacgMXFs7kl_fuSSe6lp6YRaeAp6vqra9&index=4
// FlaUI docs: https://github.com/FlaUI/FlaUI/wiki
// Sync & Timeouts example: https://www.lambdatest.com/automation-testing-advisor/csharp/classes/FlaUI.Core.Tools.Retry

namespace FLAU_Demo
{
    public class Tests
    {
        Application kpApp;
        UIA3Automation automation = new UIA3Automation();
        //create a conditon factory to filtering elements
        ConditionFactory cf = new ConditionFactory(new UIA3PropertyLibrary());

        [SetUp]
        public void Setup()
        {
            //Window loginWin;
            kpApp = Application.Launch(@"C:\Program Files\KeePass Password Safe 2\KeePass.exe");
            kpApp.WaitWhileBusy(TimeSpan.FromSeconds(4)); //This doesn't do much here

            //get the KeePass Window
            //wait for the window using retry instead of a thread.sleep
            Retry.WhileException(() =>
            {
                var loginWin = kpApp.GetAllTopLevelWindows(automation)[0];
            
                var passwordField = WaitForElement(() => loginWin?.FindFirstDescendant(cf => cf.ByAutomationId("m_tbPassword")).AsTextBox());
                passwordField?.Enter("password");

                loginWin.FindFirstDescendant(cf => cf.ByAutomationId("m_btnOK")).AsButton().Invoke();

            }, TimeSpan.FromSeconds(30), null, true);
        }

        [Test]
        public void NewEntryTest()
        {
            Console.WriteLine("Login Successful");

            Retry.WhileException(() =>
            {
                //Switch to new Main KeePass Window (refresh getting first child window of kpApp)
                var mainWin = kpApp.GetAllTopLevelWindows(automation)[0];

                //New Entry Button Click
                mainWin.FindFirstByXPath("/ToolBar/Button[@Name=\"Add Entry\"]").AsButton().Click();

                //Get Add Entry Window
                Window AddEntryWin = WaitForElement(() => mainWin.FindFirstByXPath("/Window[@AutomationId=\"PwEntryForm\"]").AsWindow())
                    ?? throw new NullReferenceException("Unable to find Add Entry Window!");

                AddEntryWin?.FindFirstDescendant(cf => cf.ByAutomationId("m_tbTitle")).AsTextBox().Enter("edgewords");
                AddEntryWin?.FindFirstDescendant(cf => cf.ByAutomationId("m_tbUserName")).AsTextBox().Enter("tomm");
                AddEntryWin?.FindFirstDescendant(cf => cf.ByAutomationId("m_tbUrl")).AsTextBox().Enter("www.edgewords.co.uk");
                AddEntryWin?.FindFirstDescendant(cf => cf.ByAutomationId("m_btnOK")).AsButton().Click();
  
                //Back to main window
                //Check entry is there:
                var firstListItem = mainWin.FindFirstByXPath("//List[@AutomationId=\"m_lvEntries\"]/ListItem");
                Assert.That(firstListItem.Properties.Name, Is.EqualTo("edgewords"));

                //Delete the entry & tidy up
                firstListItem.Click();
                Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.DELETE);
                //dialog asking are you sure appears:
                var yesBtn = WaitForElement(() => mainWin.FindFirstByXPath("/Window[@Name=\"KeePass\"]/Button[@Name=\"Yes\"]").AsButton());
                yesBtn.Click();

                //Save & Close the app
                var saveBtn = WaitForElement(() => mainWin.FindFirstByXPath("/ToolBar/Button[@Name=\"Save Database\"]").AsButton());
                saveBtn.Click();

                mainWin.Close();

            }, TimeSpan.FromSeconds(30), null, true);
        }

        private T WaitForElement<T>(Func<T> getter)
        {
            var retry = Retry.WhileNull<T>(() => getter(), TimeSpan.FromMilliseconds(10000));

            return retry.Result;
        }
    }
}