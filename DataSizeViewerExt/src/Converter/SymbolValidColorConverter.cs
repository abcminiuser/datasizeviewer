﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace FourWalledCubicle.DataSizeViewerExt.Converter
{
    public class SymbolValidColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color foregroundColor = Colors.Black;

            if (value != null)
            {
                ItemSize symbolInfo = (ItemSize)value;

                if (symbolInfo.Storage.Contains("Data"))
                {
                   foregroundColor = symbolInfo.LocationExists ?
                            DataSizeViewerPackage.Options.DataSymbolColor :
                            DataSizeViewerPackage.Options.UnavailableDataSymbolColor;
                }
                else
                {
                    foregroundColor = symbolInfo.LocationExists ?
                            DataSizeViewerPackage.Options.TextSymbolColor :
                            DataSizeViewerPackage.Options.UnavailableTextSymbolColor;
                }
            }

            return new SolidColorBrush(foregroundColor);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
