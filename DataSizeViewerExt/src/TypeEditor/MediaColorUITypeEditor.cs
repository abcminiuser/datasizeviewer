using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;

namespace FourWalledCubicle.DataSizeViewerExt.TypeEditor
{
    class MediaColorUITypeEditor : UITypeEditor
    {
        private static ColorEditor colorEditorInstance = new System.Drawing.Design.ColorEditor();

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            System.Windows.Media.Color wpfColor = (System.Windows.Media.Color)value;
            Color winFormsColor = Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

            winFormsColor = (Color)colorEditorInstance.EditValue(context, provider, winFormsColor);
            wpfColor = System.Windows.Media.Color.FromArgb(winFormsColor.A, winFormsColor.R, winFormsColor.G, winFormsColor.B);

            return wpfColor;
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            System.Windows.Media.Color wpfColor = (System.Windows.Media.Color)e.Value;
            Color winFormsColor = Color.FromArgb(wpfColor.A, wpfColor.R, wpfColor.G, wpfColor.B);

            e.Graphics.FillRectangle(new SolidBrush(winFormsColor), e.Bounds);
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }     
    }
}
