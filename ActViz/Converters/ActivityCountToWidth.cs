using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace ActViz.Converters
{
    class ActivityCountToWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((int) value) * 150;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
