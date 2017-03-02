using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ActViz.Converters
{
    public class ColorToString : IValueConverter
    {
        Dictionary<Color, string> colorDictionary = new Dictionary<Color, string>();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(colorDictionary.Count == 0)
            {
                foreach (var color in typeof(Colors).GetRuntimeProperties())
                {
                    try
                    {
                        colorDictionary.Add((Color)color.GetValue(null), color.Name);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            string colorName;
            if(colorDictionary.TryGetValue((Color)value, out colorName)) {
                colorName = colorName + " (" + ((Color)value).ToString() + ")";
                return colorName;
            }
            else
            {
                return "(" + ((Color)value).ToString() + ")";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
