using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace ActViz.Converters
{
    public class ColorStringTupleCastToColor : DependencyObject, IValueConverter
    {
        public ObservableCollection<Tuple<Color, string>> colorList
        {
            get { return (ObservableCollection<Tuple<Color, string>>)GetValue(colorListProperty); }
            set { SetValue(colorListProperty, value); }
        }

        public static readonly DependencyProperty colorListProperty =
            DependencyProperty.Register("colorList",
                typeof(ObservableCollection<Tuple<Color, string>>),
                typeof(ColorStringTupleCastToColor),
                new PropertyMetadata(null, ColorListChangedCallback));

        private static void ColorListChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            Debug.WriteLine(args.Property + " : " + args.OldValue + " - " + args.NewValue);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            foreach (Tuple<Color, string> colorTuple in colorList)
            {
                if (colorTuple.Item1 == (Color) value)
                {
                    return colorTuple;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if(value != null)
            {
                return ((Tuple<Color, string>)value).Item1;
            }
            else
            {
                return Colors.LightGray;
            }
        }
    }
}
