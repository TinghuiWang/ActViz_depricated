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
    public sealed partial class ConfigActivityDialog : ContentDialog
    {
        ObservableCollection<Color> ActivityColorCollection = new ObservableCollection<Color>();

        private ActivityViewModel _activityViewModel;
        public ActivityViewModel activityViewModel { get { return _activityViewModel; } }

        public bool isActivityChanged = false;

        public ConfigActivityDialog(ActivityViewModel activityViewModel)
        {
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                ActivityColorCollection.Add((Color)color.GetValue(null));
            }
            this.InitializeComponent();
            this._activityViewModel = activityViewModel;
            if(!ActivityColorCollection.Contains(_activityViewModel.color))
            {
                ActivityColorCollection.Add(_activityViewModel.color);
            }
        }

        private void ConfigActivityDialog_SaveClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if(isActivityChanged)
            {
                _activityViewModel.name = txtActivityName.Text;
                if (comboActivityColor.SelectedItem != null)
                {
                    _activityViewModel.color = (Color)comboActivityColor.SelectedItem;
                }
                _activityViewModel.isNoise = swIsNoise.IsOn;
                _activityViewModel.isIgnored = swIsIgnored.IsOn;
            }
        }

        private void ConfigActivityDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void txtActivityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isActivityChanged)
            {
                isActivityChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
        }

        private void comboActivityColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isActivityChanged)
            {
                isActivityChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
        }

        private void swIsIgnored_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isActivityChanged)
            {
                isActivityChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
            if (swIsIgnored.IsOn)
            {
                swIsNoise.IsOn = false;
            }
        }

        private void swIsNoise_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isActivityChanged)
            {
                isActivityChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
            if (swIsNoise.IsOn)
            {
                swIsIgnored.IsOn = false;
            }
        }

        private void ConfigActivityDialog_Loaded(object sender, RoutedEventArgs e)
        {
            comboActivityColor.SelectedItem = _activityViewModel.color;
        }
    }
}
