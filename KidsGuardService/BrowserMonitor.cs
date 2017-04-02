using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace WindowsKidsGuardService
{
    class BrowserMonitor
    {
        public enum BrowserType { Chrome, IE, Firefox };

        public static string GetURL(IntPtr intPtr, BrowserType browserType)
        {
            string temp = null;
            if (browserType== BrowserType.Chrome)
            {
                temp = GetChromeUrl(intPtr);
            }
            if (browserType == BrowserType.IE)
            {
                foreach (SHDocVw.InternetExplorer ie in new SHDocVw.ShellWindows())
                {
                    var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(ie.FullName);
                    if (fileNameWithoutExtension != null)
                    {
                        var filename = fileNameWithoutExtension.ToLower();
                        if (filename.Equals("iexplore"))
                        {
                            temp += ie.LocationURL + " ";
                        }
                    }
                }
            }
            
            /*
            if (programName.Equals("firefox"))
            {
                DdeClient dde = new DdeClient("Firefox", "WWW_GetWindowInfo");
                dde.Connect();
                string url1 = dde.Request("URL", int.MaxValue);
                dde.Disconnect();
                temp = url1.Replace("\"", "").Replace("\0", "");
            }
            */

            return temp;
        }

        public static string GetChromeUrl(IntPtr intPtr)
        {
            if (intPtr == IntPtr.Zero)
                return null;

            AutomationElement element = AutomationElement.FromHandle(intPtr);
            if (element == null)
                return null;

            AutomationElementCollection edits5 = element.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
            AutomationElement edit = edits5[0];
            string vp = ((ValuePattern)edit.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
            Console.WriteLine(vp);
            return vp;
        }
    }
}
