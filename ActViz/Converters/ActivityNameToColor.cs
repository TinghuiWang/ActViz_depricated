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
    public class ActivityNameToColor : DependencyObject, IValueConverter
    {
        public ObservableCollection<ActivityViewModel> activityItems
        {
            get { return (ObservableCollection<ActivityViewModel>)GetValue(activityItemsProperty); }
            set { SetValue(activityItemsProperty, value); }
        }

        public static readonly DependencyProperty activityItemsProperty =
            DependencyProperty.Register("activityItems",
                typeof(ObservableCollection<ActivityViewModel>),
                typeof(ActivityNameToColor),
                new PropertyMetadata(null, ActivityItemsCallBack));

        private static void ActivityItemsCallBack(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            Debug.WriteLine(args.Property + " : " + args.OldValue + " - " + args.NewValue);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ActivityViewModel activity = activityItems.ToList().Find(x => x.name == (string)value);
            if(activity != null)
            {
                return new SolidColorBrush(activity.color);
            }
            return new SolidColorBrush(Colors.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
