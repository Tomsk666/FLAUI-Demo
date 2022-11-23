using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;

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
        //create a conditon factory to filter elements
        ConditionFactory cf = new ConditionFactory(new UIA3PropertyLibrary());

        [SetUp]
        public void Setup()
        {
            kpApp = Application.Launch(@"C:\Program Files\KeePass Password Safe 2\KeePass.exe");

            //get the KeePass Window
            Thread.Sleep(3000);
            var loginWin = kpApp.GetAllTopLevelWindows(automation)[0];

            loginWin.FindFirstDescendant(cf.ByAutomationId("m_tbPassword")).AsTextBox().Enter("password");
            loginWin.FindFirstDescendant(cf.ByAutomationId("m_btnOK")).AsButton().Invoke();
        }

        [Test]
        public void NewEntryTest()
        {
            Thread.Sleep(3000);
            //Switch to new Main KeePass Window
            var mainWin = kpApp.GetAllTopLevelWindows(automation)[0];

            //New Entry Button Click
            mainWin.FindFirstByXPath("/ToolBar/Button[@Name=\"Add Entry\"]").AsButton().Click();

            //Get Add Entry Window
            Thread.Sleep(1000);
            Window AddEntryWin = mainWin.FindFirstByXPath("/Window[@AutomationId=\"PwEntryForm\"]").AsWindow(); 
            AddEntryWin.FindFirstByXPath("/Tab/Pane/Edit[@AutomationId=\"m_tbTitle\"]").AsTextBox().Enter("edgewords");
            AddEntryWin.FindFirstByXPath("/Tab/Pane/Edit[@AutomationId=\"m_tbUserName\"]").AsTextBox().Enter("tomm");
            AddEntryWin.FindFirstByXPath("/Tab/Pane/Edit[@AutomationId=\"m_tbUrl\"]").AsTextBox().Enter("www.edgewords.co.uk");
            AddEntryWin.FindFirstByXPath("/Button[@AutomationId=\"m_btnOK\"]").AsButton().Click();

            //Back to main window
            Thread.Sleep(2000);
            //Check entry is there:
            var firstListItem = mainWin.FindFirstByXPath("//List[@AutomationId=\"m_lvEntries\"]/ListItem");
            Assert.That(firstListItem.Properties.Name, Is.EqualTo("edgewords"));

            //Delete the entry & tidy up
            firstListItem.Click();
            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.DELETE);
            Thread.Sleep(2000);
            mainWin.FindFirstByXPath("/Window[@Name=\"KeePass\"]/Button[@Name=\"Yes\"]").AsButton().Click();
            
            //Save & Close the app
            Thread.Sleep(1000);
            mainWin.FindFirstByXPath("/ToolBar/Button[@Name=\"Save Database\"]").AsButton().Click();
            mainWin.Close();
        }
    }
}