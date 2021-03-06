﻿using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;

namespace FourWalledCubicle.DataSizeViewerExt
{
    [System.ComponentModel.DesignerCategory("")]
    public class OptionsPage : DialogPage
    {
        private bool mVerifyLocations = true;
        private Color mTextSymbolColor = Colors.DarkGreen;
        private Color mUnavailableTextSymbolColor = Colors.DarkGray;
        private Color mDataSymbolColor = Colors.DarkBlue;
        private Color mUnavailableDataSymbolColor = Colors.DarkGray;
        private bool mUseRegExFilter = false;

        [DisplayName("Verify Symbol Locations")]
        [Description("Verify that a symbol's location source file exists; if not, the unavailable symbol color is used.")]
        public bool VerifyLocations
        {
            get { return mVerifyLocations; }
            set { mVerifyLocations = value; }
        }

        [DisplayName("Text Symbol Color")]
        [Description("Color used for symbols in the Text section, if their source file is available.")]
        [Editor(typeof(MediaColorUITypeEditor), typeof(UITypeEditor))]
        public Color TextSymbolColor
        {
            get { return mTextSymbolColor; }
            set { mTextSymbolColor = value; }
        }

        [DisplayName("Unavailable Text Symbol Color")]
        [Description("Color used for symbols in the Text section, if their source file is unavailable.")]
        [Editor(typeof(MediaColorUITypeEditor), typeof(UITypeEditor))]
        public Color UnavailableTextSymbolColor
        {
            get { return mUnavailableTextSymbolColor; }
            set { mUnavailableTextSymbolColor = value; }
        }

        [DisplayName("Data Symbol Color")]
        [Description("Color used for symbols in the Data section, if their source file is available.")]
        [Editor(typeof(MediaColorUITypeEditor), typeof(UITypeEditor))]
        public Color DataSymbolColor
        {
            get { return mDataSymbolColor; }
            set { mDataSymbolColor = value; }
        }

        [DisplayName("Unavailable Data Symbol Color")]
        [Description("Color used for symbols in the Data section, if their source file is unavailable.")]
        [Editor(typeof(MediaColorUITypeEditor), typeof(UITypeEditor))]
        public Color UnavailableDataSymbolColor
        {
            get { return mUnavailableDataSymbolColor; }
            set { mUnavailableDataSymbolColor = value; }
        }

        [DisplayName("Use Regular Expression Filtering")]
        [Description("Use Regular Expressions instead of simple matching in the filter box.")]
        public bool UseRegExFiltering
        {
            get { return mUseRegExFilter; }
            set { mUseRegExFilter = value; }
        }    
    }
}
