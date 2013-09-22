using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace FourWalledCubicle.DataSizeViewerExt
{
    [Guid(GuidList.guidToolWindowPersistanceString)]
    public class DataSizeToolWindow : ToolWindowPane
    {
        public DataSizeToolWindow() :
            base(null)
        {
            this.Caption = Resources.ToolWindowTitle;

            this.BitmapResourceID = 302;
            this.BitmapIndex = 0;

            base.Content = new DataSizeViewerUI();
        }
    }
}
