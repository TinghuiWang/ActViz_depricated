using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace ActViz.Converters
{
    public class ResidentNameToColor : DependencyObject, IValueConverter
    {
        public ObservableCollection<ResidentViewModel> residentItems
        {
            get { return (ObservableCollection<ResidentViewModel>)GetValue(residentItemsProperty); }
            set { SetValue(residentItemsProperty, value); }
        }

        public static readonly DependencyProperty residentItemsProperty =
            DependencyProperty.Register("residentItems", 
                typeof(ObservableCollection<ResidentViewModel>), 
                typeof(ResidentNameToColor), 
                new PropertyMetadata(null, ResidentItemsChangedCallback));

        private static void ResidentItemsChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            Debug.WriteLine(args.Property + " : " + args.OldValue + " - " + args.NewValue);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string occupantId = value as string;
            ResidentViewModel resident = residentItems.ToList().Find(x => x.name == occupantId);
            if (resident != null)
            {
                return new SolidColorBrush(resident.color);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
