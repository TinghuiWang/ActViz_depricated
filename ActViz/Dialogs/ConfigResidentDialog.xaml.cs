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
    public sealed partial class ConfigResidentDialog : ContentDialog
    {
        private ResidentViewModel _residentViewModel;
        public ResidentViewModel residentViewModel { get { return _residentViewModel; } }

        ObservableCollection<Color> ResidentColorCollection = new ObservableCollection<Color>();

        public bool isResidentChanged = false;

        public ConfigResidentDialog(ResidentViewModel residentViewModel)
        {
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                ResidentColorCollection.Add((Color)color.GetValue(null));
            }
            this._residentViewModel = residentViewModel;
            this.InitializeComponent();
        }

        private void ConfigResidentDialog_SaveClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (isResidentChanged)
            {
                _residentViewModel.name = txtResidentName.Text;
                if (comboResidentColor.SelectedItem != null)
                {
                    _residentViewModel.color = (Color)comboResidentColor.SelectedItem;
                }
                _residentViewModel.isNoise = swIsNoise.IsOn;
                _residentViewModel.isIgnored = swIsIgnored.IsOn;
            }
        }

        private void ConfigResidentDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void swIsIgnored_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isResidentChanged)
            {
                isResidentChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
            if (swIsIgnored.IsOn)
            {
                swIsNoise.IsOn = false;
            }
        }

        private void swIsNoise_Toggled(object sender, RoutedEventArgs e)
        {
            if (!isResidentChanged)
            {
                isResidentChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
            if (swIsNoise.IsOn)
            {
                swIsIgnored.IsOn = false;
            }
        }

        private void txtResidentName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isResidentChanged)
            {
                isResidentChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
        }

        private void ConfigResidentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            comboResidentColor.SelectedItem = _residentViewModel.color;
        }

        private void comboResidentColor_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isResidentChanged)
            {
                isResidentChanged = true;
                this.IsPrimaryButtonEnabled = true;
            }
        }
    }
}
