using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class AddSensorDialog : ContentDialog
    {
        private ObservableCollection<SensorType> allSensorTypes;

        public AddSensorDialog()
        {
            this.InitializeComponent();
            allSensorTypes = SensorType.GetSensorTypes();
        }

        public string sensorName { get { return txtNameAddSensor.Text; } }
        public string sensorType { get { return allSensorTypes[comboTypeAddSensor.SelectedIndex].type; } }

        private void AddSensorDialog_AddSensorClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void AddSensorDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

    }
}
