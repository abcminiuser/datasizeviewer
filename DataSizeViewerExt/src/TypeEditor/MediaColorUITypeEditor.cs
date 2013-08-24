using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using WinFormsColor = System.Drawing.Color;
using WPFColor = System.Windows.Media.Color;

namespace FourWalledCubicle.DataSizeViewerExt.TypeEditor
{
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
            WinFormsColor winFormsColor = WinFormsColor.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

            winFormsColor = (WinFormsColor)colorEditorInstance.EditValue(context, provider, winFormsColor);
            wpfColor = WPFColor.FromArgb(winFormsColor.A, winFormsColor.R, winFormsColor.G, winFormsColor.B);

            return wpfColor;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            WPFColor wpfColor = (WPFColor)e.Value;
            WinFormsColor winFormsColor = WinFormsColor.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

            e.Graphics.FillRectangle(new SolidBrush(winFormsColor), e.Bounds);
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }     
    }
}
