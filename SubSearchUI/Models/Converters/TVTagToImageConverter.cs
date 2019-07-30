/// Source: https://www.codeproject.com/Articles/21248/A-Simple-WPF-Explorer-Tree

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SubSearchUI.Models.Converters
{
    #region HeaderToImageConverter

    [ValueConversion(typeof(TreeViewItemTag), typeof(bool))]
    public class TVTagToImageConverter : IValueConverter
    {
        public static TVTagToImageConverter Instance = new TVTagToImageConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TreeViewItemTag tag = (value as TreeViewItemTag);

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
