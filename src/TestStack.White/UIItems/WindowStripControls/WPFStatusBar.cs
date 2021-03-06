using System.Linq;
using System.Windows.Automation;
using TestStack.White.UIItems.Actions;
using TestStack.White.UIItems.Finders;

namespace TestStack.White.UIItems.WindowStripControls
{
    public class WPFStatusBar : UIItem
    {
        protected WPFStatusBar() {}
        public WPFStatusBar(AutomationElement automationElement, IActionListener actionListener) : base(automationElement, actionListener) {}

        public virtual UIItemCollection Items
        {
            get
            {
                var uiItemCollection = factory.CreateAll(SearchCriteria.All, actionListener, automationElement.Current.FrameworkId)
                    .Where(i=>i.AutomationElement.Current.ClassName == "StatusBarItem");
                return new UIItemCollection(uiItemCollection);
            }
        }
    }
}