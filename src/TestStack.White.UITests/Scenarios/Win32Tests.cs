﻿using Castle.Core.Logging;
using NUnit.Framework;
using System.Diagnostics;
using System.Linq;
using System.Windows.Automation;
using TestStack.White.Configuration;
using TestStack.White.Factory;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.ListBoxItems;
using TestStack.White.UIItems.MenuItems;
using TestStack.White.UIItems.WindowStripControls;
using TestStack.White.WindowsAPI;

namespace TestStack.White.UITests.Scenarios
{
    [TestFixture]
    public class Win32Tests
    {
        const string ExeSourceFile = @"C:\Windows\system32\calc.exe";
        const string Notepad = @"C:\Windows\system32\notepad.exe";
        const string InternetExplorer = @"C:\Program Files\Internet Explorer\iexplore.exe";

        [Test]
        public void NotepadTests()
        {
            using (var app = Application.Launch(Notepad))
            using (var window = app.GetWindow("Untitled - Notepad"))
            {
                window.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.ALT);
                window.Keyboard.Enter("o");
                window.Keyboard.Enter("f");

                using (var modalWindow = window.ModalWindow("Font"))
                {
                    Assert.That(modalWindow, Is.Not.Null);
                }
            }
        }

        [Test]
        public void NotepadTests_FileExit()
        {
            CoreAppXmlConfiguration.Instance.LoggerFactory = new ConsoleFactory(LoggerLevel.Debug);
            CoreAppXmlConfiguration.Instance.RawElementBasedSearch = true;
            var applicationName = "notepad.exe";
            Application application = Application.Launch(applicationName);
            var window = application.GetWindow("Untitled - Notepad", InitializeOption.NoCache);
            var menubar = window.MenuBars[1];
            var file = menubar.MenuItem("File");
            file.Click();
            window.PopupMenuParentIsDesktop = false;
            var x = window.Popup;
            var exit = x.Item("Exit");
            exit.Click();
        }

        [Test]
        public void NotepadTests_DrawHighlight()
        {
            CoreAppXmlConfiguration.Instance.LoggerFactory = new ConsoleFactory(LoggerLevel.Debug);
            CoreAppXmlConfiguration.Instance.RawElementBasedSearch = true;
            var applicationName = "notepad.exe";
            Application application = Application.Launch(applicationName);
            var window = application.GetWindow("Untitled - Notepad", InitializeOption.NoCache);
            var menubar = window.MenuBars[1];
            var file = menubar.MenuItem("File");
            file.Click();
            window.PopupMenuParentIsDesktop = false;
            var x = window.Popup;
            x.DrawHighlight();
            var exit = x.Item("Exit");
            exit.Click();
        }

        [Test]
        public void NotepadTests_ComboBox()
        {
            CoreAppXmlConfiguration.Instance.LoggerFactory = new ConsoleFactory(LoggerLevel.Debug);
            CoreAppXmlConfiguration.Instance.RawElementBasedSearch = true;
            var applicationName = "notepad.exe";
            Application application = Application.Launch(applicationName);
            var main_window = application.GetWindow("Untitled - Notepad", InitializeOption.NoCache);
            var menubar = main_window.MenuBars[1];
            var format = menubar.MenuItem("Format");
            format.Click();
            main_window.PopupMenuParentIsDesktop = false;
            var popup = main_window.Popup;
            var font = popup.Item("Font...");
            font.Click();
            var font_dialog = main_window.ModalWindow("Font");
            var combo_size = font_dialog.Get<ComboBox>(SearchCriteria.ByText("Size:"));
            combo_size.Select("72");
        
            var combo_script = font_dialog.Get<ComboBox>(SearchCriteria.ByText("Script:"));
            combo_script.Select("Greek");
            combo_script.DrawHighlight();
            combo_script.Select(0);


            var cancel = font_dialog.Get<Button>(SearchCriteria.ByText("Cancel"));
            cancel.Click();

            var file = menubar.MenuItem("File");
            file.Click();
            popup = main_window.Popup;
            var exit = popup.Item("Exit");
            exit.Click();
        }


        [Test]
        [Category("NeedsFix")]
        [Ignore("NeedsFix")]
        public void LegacyIAccessibleTest()
        {
            const uint STATE_SYSTEM_CHECKED = 0x10; // from oleacc.h

            using (var app = Application.Launch(Notepad))
            using (var window = app.GetWindow("Untitled - Notepad"))
            {
                var formatMenu = window.AutomationElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, "Format"));
                
                var menuBar = window.Get<MenuBar>();
                
                // notepad seems to remember last state of Word Wrap, so don't know what it will be
                var wordWrap = menuBar.MenuItem("Format", "Word Wrap");
                var legacyPattern = wordWrap.GetPattern<LegacyIAccessiblePattern>();
                bool isChecked = (legacyPattern.Current.State & STATE_SYSTEM_CHECKED) != 0;
                wordWrap.Click();

                var wordWrap2nd = menuBar.MenuItem("Format", "Word Wrap");
                var legacyPattern2nd = wordWrap2nd.GetPattern<LegacyIAccessiblePattern>();
                bool isChecked2nd = (legacyPattern2nd.Current.State & STATE_SYSTEM_CHECKED) != 0;
                Assert.AreNotEqual(isChecked, isChecked2nd);
            }
        }

        [Test]
        [Ignore("There are different IE versions which make this test fail")]
        public void InternetExplorerTests()
        {
            using (var app = Application.Launch(InternetExplorer))
            {
                using (var window = app.GetWindows().Single())
                {
                    var button = window.Get<Button>(SearchCriteria.ByAutomationId("Item 3"));
                    //check if we can get a win32 tooltip
                    Assert.That(window.GetToolTipOn(button).Text, Is.EqualTo("Tools (Alt+X)"));
                    button.Click();
                    window.PopupMenu("Internet options").Click();
                    using (var internetOptions = window.ModalWindow("Internet Options"))
                    {
                        var textBox = internetOptions.Get<TextBox>(SearchCriteria.ByAutomationId("1487"));

                        textBox.Text = "http://google.com";

                        Assert.That(textBox.Text, Is.EqualTo("http://google.com"));
                    }
                }
            }
        }

        [Test]
        [Category("NeedsFix")]
        [Ignore("NeedsFix")]
        public void CalculatorTests()
        {
            //strat process for the above exe file location
            var psi = new ProcessStartInfo(ExeSourceFile);
            // launch the process through white application
            using (var application = Application.AttachOrLaunch(psi))
            using (var mainWindow = application.GetWindow(SearchCriteria.ByText("Calculator"), InitializeOption.NoCache))
            {
                // Verify can click on menu twice
                var menuBar = mainWindow.Get<MenuBar>(SearchCriteria.ByText("Application"));
                menuBar.MenuItem("Edit", "Copy").Click();
                menuBar.MenuItem("Edit", "Copy").Click();

                mainWindow.Keyboard.HoldKey(KeyboardInput.SpecialKeys.CONTROL);
                mainWindow.Keyboard.Enter("E");
                mainWindow.Keyboard.LeaveKey(KeyboardInput.SpecialKeys.CONTROL);

                //On Date window find the difference between dates.
                //Set value into combobox
                mainWindow.Get<ComboBox>(SearchCriteria.ByAutomationId("4003")).Select("Calculate the difference between two dates");
                //Click on Calculate button
                mainWindow.Get<Button>(SearchCriteria.ByAutomationId("4009")).Click();

                mainWindow.Keyboard.HoldKey(KeyboardInput.SpecialKeys.CONTROL);
                mainWindow.Keyboard.PressSpecialKey(KeyboardInput.SpecialKeys.F4);
                mainWindow.Keyboard.LeaveKey(KeyboardInput.SpecialKeys.CONTROL);

                var menuView = mainWindow.Get<Menu>(SearchCriteria.ByText("View"));
                menuView.Click();
                var menuViewBasic = mainWindow.Get<Menu>(SearchCriteria.ByText("Basic"));
                menuViewBasic.Click();

                PerformSummationOnCalculator(mainWindow);
            }
        }

        /// <summary>
        /// method to Perform Addition of two numbers and validate the result
        /// </summary>
        private static void PerformSummationOnCalculator(UIItemContainer mainWindow)
        {
            mainWindow.Get<Button>(SearchCriteria.ByText("1")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("2")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("3")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("4")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("Add")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("5")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("6")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("7")).Click();
            mainWindow.Get<Button>(SearchCriteria.ByText("8")).Click();
            //Button with text as +(for sum)
            //Read button to get the result
            mainWindow.Get<Button>(SearchCriteria.ByText("Equals")).Click();

            //Get the result
            var resultLable = mainWindow.Get<Label>(SearchCriteria.ByAutomationId("150"));
            var result = resultLable.Text;
            Assert.That(result, Is.EqualTo("6912"));
        }
    }
}