using ActViz.Models;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Dialogs
{
    public sealed partial class AddActivityDialog : ContentDialog
    {
        private ActivityViewModel _activityViewModel = new ActivityViewModel();
        public ActivityViewModel activityViewModel { get { return _activityViewModel; } }

        ObservableCollection<Color> ActivityColorCollection = new ObservableCollection<Color>();

        public AddActivityDialog()
        {
            this.InitializeComponent();
            foreach(var color in typeof(Colors).GetRuntimeProperties())
            {
                ActivityColorCollection.Add((Color) color.GetValue(null));
            }
        }

        private void AddActivityDialog_AddClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //_activityViewModel.color = ((Tuple<Color, string>)comboActivityColor.SelectedItem).Item1;
        }

        private void AddActivityDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
