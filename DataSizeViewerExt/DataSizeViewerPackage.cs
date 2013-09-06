using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace FourWalledCubicle.DataSizeViewerExt
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideToolWindow(typeof(DataSizeToolWindow), MultiInstances = false, Style = VsDockStyle.Tabbed, Transient = false, Orientation = ToolWindowOrientation.Bottom, Window = ToolWindowGuids.Outputwindow)]
    [ProvideOptionPageAttribute(typeof(OptionsPage), "Extensions", "Data Size Viewer", 15600, 1912, true)]
    [Guid(GuidList.guidDataSizeViewerPkgString)]
    public sealed class DataSizeViewerPackage : Package
    {
        internal static OptionsPage Options { get; private set; }
        internal static Package Package { get; private set; }
        
        public DataSizeViewerPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        private void ShowToolWindow(object sender, EventArgs e)
        {
            ToolWindowPane window = this.FindToolWindow(typeof(DataSizeToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            Package = this;

            // Get extension configuration options
            try
            {
                Options = GetDialogPage(typeof(OptionsPage)) as OptionsPage;
            }
            catch
            {
                Options = new OptionsPage();
            }

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidDataSizeViewerCmdSet, (int)PkgCmdIDList.cmdidELFSymbolSizes);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }
        }
    }
}
