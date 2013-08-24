using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace FourWalledCubicle.DataSizeViewerExt
{
    public class OptionsPage : DialogPage
    {
        private bool mVerifyLocations = true;

        [DisplayName("Verify Symbol Locations")]
        [Description("Verify that a symbol's location source file exists; if not, the symbol is grayed out in the symbol list.")]
        public bool VerifyLocations
        {
            get { return mVerifyLocations; }
            set { mVerifyLocations = value; }
        }
    }
}
