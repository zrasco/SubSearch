using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SubSearchUI.Models.Converters
{
    #region LVTagToImageConverter

    [ValueConversion(typeof(ListViewItemTag), typeof(bool))]
    public class LVTagToImageConverter : IValueConverter
    {
        public static LVTagToImageConverter Instance = new LVTagToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ListViewItemTag tag = (value as ListViewItemTag);

            Uri uri = new Uri(tag.ImageUri);
            BitmapImage source = new BitmapImage(uri);
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("Cannot convert back");
        }
    }

    #endregion // TagToImageConverter
}
