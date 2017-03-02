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
    public sealed partial class ConfigSensorDialog : ContentDialog
    {
        private int _sensorTupleIndex;
        private string _sensorName;
        private string _sensorType;
        private string _sensorDescription;
        private double _defaultWidth;
        private double _defaultHeight;
        private double _maxWidth;
        private double _maxHeight;
        private EditView _parentPage;
        private ObservableCollection<SensorType> sensorTypeList;

        public int sensorTupleIndex { get { return _sensorTupleIndex; } }
        public string sensorName { get { return _sensorName; } }
        public string sensorType { get { return _sensorType; } }
        public string sensorDescription { get { return _sensorDescription; } }
        public double sensorWidth
        {
            get
            {
                if (tbSensorWidth.Text != "")
                {
                    return double.Parse(tbSensorWidth.Text);
                }
                else
                {
                    return _defaultWidth;
                }
            }
        }
        public double sensorHeight
        {
            get
            {
                if (tbSensorHeight.Text != "")
                {
                    return double.Parse(tbSensorHeight.Text);
                }
                else
                {
                    return _defaultHeight;
                }
            }
        }


        public ConfigSensorDialog(EditView parentPage, int sensorTupleIndex, string sensorName,
            string sensorType, double defaultWidth, double defaultHeight,
            double maxWidth, double maxHeight, string sensorDescription)
        {
            this.InitializeComponent();
            _parentPage = parentPage;
            _sensorTupleIndex = sensorTupleIndex;
            _sensorName = sensorName;
            _sensorType = sensorType;
            _defaultWidth = defaultWidth;
            _defaultHeight = defaultHeight;
            _maxWidth = maxWidth;
            _maxHeight = maxHeight;
            _sensorDescription = sensorDescription;
            sensorTypeList = SensorType.GetSensorTypes();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            txtNameConfigSensor.Text = _sensorName;
            txtSensorDescription.Text = _sensorDescription;
            int typeIndex = SensorType.GetSensorTypeIndex(_sensorType);
            if (typeIndex >= 0)
            {
                comboTypeConfigSensor.SelectedIndex = typeIndex;
            }
            tbSensorHeight.Text = _defaultHeight.ToString("F1");
            tbSensorWidth.Text = _defaultWidth.ToString("F1");
        }

        private void ConfigSensorDialog_SaveClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _sensorName = txtNameConfigSensor.Text;
            _sensorDescription = txtSensorDescription.Text;
            if (tbSensorHeight.Text != "")
            {
                double newHeight = double.Parse(tbSensorHeight.Text);
                if (newHeight > 0 && newHeight < _maxHeight)
                {
                    _defaultHeight = newHeight;
                }
            }
            if (tbSensorWidth.Text != "")
            {
                double newWidth = double.Parse(tbSensorWidth.Text);
                if (newWidth > 0 && newWidth < _maxHeight)
                {
                    _defaultWidth = newWidth;
                }
            }
        }

        private void ConfigSensorDialog_CancelClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            txtNameConfigSensor.Text = _sensorName;
            txtSensorDescription.Text = _sensorDescription;
            tbSensorHeight.Text = _defaultHeight.ToString("F1");
            tbSensorWidth.Text = _defaultWidth.ToString("F1");
        }

        private async void tbSensorWidth_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSensorHeight.Text != "" && tbSensorWidth.Text != "")
            {
                double newWidth = double.Parse(tbSensorWidth.Text);
                double newHeight = double.Parse(tbSensorHeight.Text);
                if (newWidth < _maxWidth && newHeight < _maxHeight)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        _parentPage.TemporaryUpdateSensorDisplay(_sensorTupleIndex, newWidth, newHeight);
                    });
                }
            }
        }

        private async void tbSensorHeight_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSensorHeight.Text != "" && tbSensorWidth.Text != "")
            {
                double newWidth = double.Parse(tbSensorWidth.Text);
                double newHeight = double.Parse(tbSensorHeight.Text);
                if (newWidth < _maxWidth && newHeight < _maxHeight)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        _parentPage.TemporaryUpdateSensorDisplay(_sensorTupleIndex, newWidth, newHeight);
                    });
                }
            }
        }

        private void Button_tbSensorWidthIncreaseClick(object sender, RoutedEventArgs e)
        {
            if (tbSensorWidth.Text != "")
            {
                tbSensorWidth.Text = (double.Parse(tbSensorWidth.Text) + 1.0).ToString("F1");
            }
            else
            {
                tbSensorWidth.Text = (_defaultWidth + 1.0).ToString("F1");
            }
        }

        private void Button_tbSensorWidthDecreaseClick(object sender, RoutedEventArgs e)
        {
            if (tbSensorWidth.Text != "")
            {
                tbSensorWidth.Text = (double.Parse(tbSensorWidth.Text) - 1.0).ToString("F1");
            }
            else
            {
                tbSensorWidth.Text = (_defaultWidth - 1.0).ToString("F1");
            }
        }

        private void Button_tbSensorHeightIncreaseClick(object sender, RoutedEventArgs e)
        {
            if (tbSensorHeight.Text != "")
            {
                tbSensorHeight.Text = (double.Parse(tbSensorHeight.Text) + 1.0).ToString("F1");
            }
            else
            {
                tbSensorHeight.Text = (_defaultHeight + 1.0).ToString("F1");
            }
        }

        private void Button_tbSensorHeightDecreaseClick(object sender, RoutedEventArgs e)
        {
            if (tbSensorHeight.Text != "")
            {
                tbSensorHeight.Text = (double.Parse(tbSensorHeight.Text) - 1.0).ToString("F1");
            }
            else
            {
                tbSensorHeight.Text = (_defaultHeight - 1.0).ToString("F1");
            }
        }
    }
}
