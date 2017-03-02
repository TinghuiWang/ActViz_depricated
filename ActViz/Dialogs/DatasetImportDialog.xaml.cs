using ActViz.Models;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
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
    public sealed partial class DatasetImportDialog : ContentDialog
    {
        private HomeViewModel _homeViewModel;
        public HomeViewModel homeViewModel { get { return _homeViewModel; } set { _homeViewModel = value; } }

        private StorageFolder _targetFolder;
        private StorageFile _selectedFile;
        private StorageFile _floorPlanFile;

        private bool finishImport = false;

        public DatasetImportDialog()
        {
            Dataset _dataset = new Dataset();
            _homeViewModel = new HomeViewModel(_dataset);
            this.InitializeComponent();
        }

        private bool DatasetImportDialog_ValidationCheck()
        {
            if (_homeViewModel.DatasetName == "")
                return false;
            if (txtDatasetFilePath.Text == "")
                return false;
            if (txtFolderPath.Text == "")
                return false;
            return true;
        }

        private void DatasetImportDialog_DisableControls()
        {
            btnDatasetFilePath.IsEnabled = false;
            btnFloorPlanFilePath.IsEnabled = false;
            btnFolderSelection.IsEnabled = false;
            txtDatasetName.IsEnabled = false;
            IsPrimaryButtonEnabled = false;
            IsSecondaryButtonEnabled = false;
        }

        private void DatasetImportDialog_EnableControls()
        {
            btnDatasetFilePath.IsEnabled = true;
            btnFloorPlanFilePath.IsEnabled = true;
            btnFolderSelection.IsEnabled = true;
            txtDatasetName.IsEnabled = true;
            IsSecondaryButtonEnabled = true;
            IsPrimaryButtonEnabled = DatasetImportDialog_ValidationCheck();
        }

        private async void DatasetImportDialog_ImportButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string[] colorList =
            {
                Colors.Red.ToString(),
                Colors.Blue.ToString(),
                Colors.Green.ToString(),
                Colors.BlueViolet.ToString(),
                Colors.Aqua.ToString(),
                Colors.Fuchsia.ToString(),
                Colors.Orange.ToString(),
                Colors.DeepSkyBlue.ToString(),
                Colors.Olive.ToString(),
                Colors.MediumVioletRed.ToString(),
                Colors.Pink.ToString(),
                Colors.SlateGray.ToString(),
                Colors.PaleGreen.ToString(),
                Colors.RosyBrown.ToString(),
                Colors.Sienna.ToString(),
                Colors.YellowGreen.ToString(),
                Colors.Bisque.ToString(),
                Colors.SpringGreen.ToString(),
                Colors.LightSteelBlue.ToString()
            };
            // Disable all controls
            DatasetImportDialog_DisableControls();
            progressBarImport.Visibility = Visibility.Visible;
            txtProgressImportStatus.Visibility = Visibility.Visible;
            // Show Status Bar
            txtProgressImportStatus.Text = "Loading File: " + txtDatasetFilePath.Text;
            // Populate All Sensors and Activities in the dataset file
            HashSet<string> SensorDictionary = new HashSet<string>();
            HashSet<string> ActivityDictionary = new HashSet<string>();
            // Get Streams to Store Reorganized events ready
            StorageFile eventFile = await this._targetFolder.CreateFileAsync("events.csv", CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream eventStream = await eventFile.OpenAsync(FileAccessMode.ReadWrite);
            StreamWriter eventStreamWriter = new StreamWriter(eventStream.AsStreamForWrite());
            StorageFile temperatureFile = await this._targetFolder.CreateFileAsync("temperature.csv", CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream temperatureStream = await temperatureFile.OpenAsync(FileAccessMode.ReadWrite);
            StreamWriter temperatureStreamWriter = new StreamWriter(temperatureStream.AsStreamForWrite());
            StorageFile powerFile = await this._targetFolder.CreateFileAsync("power_useage.csv", CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream powerStream = await powerFile.OpenAsync(FileAccessMode.ReadWrite);
            StreamWriter powerStreamWriter = new StreamWriter(powerStream.AsStreamForWrite());
            // Get Storage File and a Read Stream
            using (var inputStream = await this._selectedFile.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                int lineNum = 0;
                // Read Line by Line
                while (streamReader.Peek() >= 0)
                {
                    lineNum++;
                    string curEventString = streamReader.ReadLine();
                    string[] tokens = curEventString.Split(new char[] { ' ' });
                    string eventString = tokens[0] + "," + tokens[1].Split(new char[] { '.' })[0];
                    // Sensor IDs
                    if (tokens.Length > 2)
                    {
                        // Add Sensor ID to Sensor Dictionary
                        if(!SensorDictionary.Contains(tokens[2]))
                        {
                            SensorDictionary.Add(tokens[2]);
                        }
                        eventString += "," + tokens[2];
                    }
                    eventString += "," + tokens[3];
                    // Activity List
                    if (tokens.Length > 4)
                    {
                        // Add Activity Label to Activity Dictionary
                        if(!ActivityDictionary.Contains(tokens[4]))
                        {
                            ActivityDictionary.Add(tokens[4]);
                        }
                        eventString += ",," + tokens[4];
                    }
                    if(lineNum % 1000 == 0)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() => txtProgressImportStatus.Text = "Parsing " + lineNum.ToString() + " events"));
                    }
                    // Rewrite current event
                    switch(SensorType.GuessSensorTypeFromName(tokens[2]))
                    {
                        case "Motion":
                        case "Door":
                        case "Item":
                        case "Light":
                            eventStreamWriter.WriteLine(eventString);
                            break;
                        case "Temperature":
                            temperatureStreamWriter.WriteLine(eventString);
                            break;
                        case "Power":
                            powerStreamWriter.WriteLine(eventString);
                            break;
                        default:
                            break;
                    }
                }
            }
            // Release all Stream Resources
            eventStreamWriter.Dispose();
            eventStream.Dispose();
            temperatureStreamWriter.Dispose();
            temperatureStream.Dispose();
            powerStreamWriter.Dispose();
            powerStream.Dispose();
            // Populate Home Structure
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() => txtProgressImportStatus.Text = "Update Dataset Structure..."));
            // Set Dataset Name
            _homeViewModel.DatasetName = txtDatasetName.Text;
            // Set Folder and Path
            _homeViewModel.dataset.Folder = this._targetFolder;
            _homeViewModel.dataset.Path = this._targetFolder.Path;
            // Copy Floor Plan
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() => txtProgressImportStatus.Text = "Copy Floor Plan..."));
            StorageFile newfloorPlanFile = await this._floorPlanFile.CopyAsync(this._targetFolder, this._floorPlanFile.Name, NameCollisionOption.ReplaceExisting);
            _homeViewModel.dataset.pathToFloorPlan = this._floorPlanFile.Name;
            // Configure Sensors
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() => txtProgressImportStatus.Text = "Configure Sensors..."));
            foreach (string sensorName in SensorDictionary)
            {
                // Add Sensor To Dataset
                _homeViewModel.dataset.AddSensor(sensorName);
            }
            // Configure Activity Labels
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() => txtProgressImportStatus.Text = "Configure Activities..."));
            int colorIndex = 0;
            foreach (string activityLabel in ActivityDictionary)
            {
                // Add Activity To Dataset
                _homeViewModel.AddActivity(new ActivityViewModel(new Activity(activityLabel, colorList[colorIndex], false, false)));
                colorIndex++;
            }
            // Save Dataset to File
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() => txtProgressImportStatus.Text = "Save Dataset To File..."));
            await _homeViewModel.dataset.SaveToFolder();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, (() => txtProgressImportStatus.Text = "Done..."));
            // Show Loading Status
            progressBarImport.Visibility = Visibility.Collapsed;
            txtProgressImportStatus.Visibility = Visibility.Collapsed;
            DatasetImportDialog_EnableControls();
            finishImport = true;
            this.Hide();
        }

        private void DatasetImportDialog_CancelButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            finishImport = true;
        }

        private async void btnDatasetFilePath_Click(object sender, RoutedEventArgs e)
        {
            DatasetImportDialog_DisableControls();
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            filePicker.FileTypeFilter.Add("*");
            StorageFile datasetFile = await filePicker.PickSingleFileAsync();
            if (datasetFile != null)
            {
                this._selectedFile = datasetFile;
                txtDatasetFilePath.Text = datasetFile.Path;
            }
            DatasetImportDialog_EnableControls();
        }

        private async void btnFolderSelection_Click(object sender, RoutedEventArgs e)
        {
            DatasetImportDialog_DisableControls();
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add(".json");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                this._targetFolder = folder;
                txtFolderPath.Text = folder.Path;
            }
            DatasetImportDialog_EnableControls();
        }

        private async void btnFloorPlanFilePath_Click(object sender, RoutedEventArgs e)
        {
            DatasetImportDialog_DisableControls();
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
            filePicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".bmp");
            StorageFile floorPlanFile = await filePicker.PickSingleFileAsync();
            if (floorPlanFile != null)
            {
                this._floorPlanFile = floorPlanFile;
                txtFloorPlanFilePath.Text = floorPlanFile.Path;
            }
            DatasetImportDialog_EnableControls();
        }

        private void ValidateInputs(object sender, TextChangedEventArgs e)
        {
            this.IsPrimaryButtonEnabled = DatasetImportDialog_ValidationCheck();
        }

        private void DatasetImportDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if(!finishImport)
            {
                args.Cancel = true;
            }
        }
    }
}
