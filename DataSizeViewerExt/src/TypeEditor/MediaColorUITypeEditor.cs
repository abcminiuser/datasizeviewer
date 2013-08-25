using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using WinFormsColor = System.Drawing.Color;
using WPFColor = System.Windows.Media.Color;

namespace FourWalledCubicle.DataSizeViewerExt.TypeEditor
{
    static internal class ColorUtil
    {
        public static WinFormsColor ToWinFormsColor(this WPFColor wpfColor)
        {
            return WinFormsColor.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);
        }
        
        public static WPFColor ToWPFColor(this WinFormsColor winFormsColor)
        {
            return WPFColor.FromArgb(winFormsColor.A, winFormsColor.R, winFormsColor.G, winFormsColor.B);
        }
    }

    class MediaColorUITypeEditor : UITypeEditor
    {
        private static ColorEditor colorEditorInstance = new ColorEditor();

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            WPFColor wpfColor = (WPFColor)value;
            WinFormsColor winFormsColor = wpfColor.ToWinFormsColor();

            winFormsColor = (WinFormsColor)colorEditorInstance.EditValue(context, provider, winFormsColor);
            wpfColor = winFormsColor.ToWPFColor();

            return wpfColor;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            WPFColor wpfColor = (WPFColor)e.Value;
            WinFormsColor winFormsColor = wpfColor.ToWinFormsColor();

            e.Graphics.FillRectangle(new SolidBrush(winFormsColor), e.Bounds);
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }     
    }
}
