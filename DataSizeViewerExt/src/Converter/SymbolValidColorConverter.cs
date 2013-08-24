using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FourWalledCubicle.DataSizeViewerExt.Converter
{
    public class SymbolValidColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? Colors.Black : Colors.DarkGray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
