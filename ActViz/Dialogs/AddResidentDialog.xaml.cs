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
    public sealed partial class AddResidentDialog : ContentDialog
    {
        private ResidentViewModel _residentViewModel;
        public ResidentViewModel residentViewModel { get { return _residentViewModel; } }

        ObservableCollection<Color> ResidentColorCollection = new ObservableCollection<Color>();

        public AddResidentDialog(int num_resident)
        {
            _residentViewModel = new ResidentViewModel(num_resident);
            foreach (var color in typeof(Colors).GetRuntimeProperties())
            {
                ResidentColorCollection.Add((Color)color.GetValue(null));
            }
            this.InitializeComponent();
        }

        private void AddResidentDialog_AddClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void AddResidentDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
