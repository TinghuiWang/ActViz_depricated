using ActViz.Dialogs;
using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ActViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Dataset> RecentDatasetsList;
        private Logging appLog = Logging.Instance;

        public MainPage()
        {
            this.RecentDatasetsList = new ObservableCollection<Dataset>();
            this.InitializeComponent();
        }

        private async void ImportDataset(object sender, RoutedEventArgs e)
        {
            appLog.Info(this.GetType().Name, "Import Dataset...");
            DatasetImportDialog dlg = new DatasetImportDialog();
            var result = await dlg.ShowAsync();
            if (result != ContentDialogResult.Secondary)
            {
                // Finish imported
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(dlg.homeViewModel.dataset.Folder, dlg.homeViewModel.dataset.Name);
                this.RecentDatasetsList.Add(dlg.homeViewModel.dataset);
            }
        }

        private async void OpenDataset(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();

            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            folderPicker.FileTypeFilter.Add(".json");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                appLog.Info(this.GetType().Name, "Open Dataset at " + folder.Path);
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                // Check if the folder is already in the list
                foreach (Dataset dataset in this.RecentDatasetsList)
                {
                    if (dataset.Path == folder.Path)
                    {
                        appLog.Warn(this.GetType().Name, string.Format("Dataset folder {0} already exists.", folder.Path));
                        return;
                    }
                }
                // Read Dataset Information from JSON file
                appLog.Info(this.GetType().Name, string.Format("Load json file from dataset at {0}", folder.Path));
                Dataset newDataset = new Dataset();
                try
                {
                    await newDataset.FromStorageFolder(folder);
                }
                catch (Exception except)
                {
                    var dlg = new Windows.UI.Popups.MessageDialog(except.Message, "Error");
                    await dlg.ShowAsync();
                    appLog.Error(this.GetType().Name, string.Format("Failed to create dataset object from dataset at {0}, error message: {1}", folder.Path, except.Message));
                    return;
                }
                newDataset.Path = folder.Path;
                newDataset.Folder = folder;
                this.RecentDatasetsList.Add(newDataset);
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder, newDataset.Name);
                appLog.Info(this.GetType().Name, string.Format("Dataset {0} Added to the list successfully.", newDataset.Name));
            }
        }

        private async void LoadDataset(object sender, RoutedEventArgs e)
        {
            if (datasetListView.SelectedIndex == -1)
            {
                var dialog = new Windows.UI.Popups.MessageDialog("Please select a dataset to load.", "Error Loading Dataset");
                await dialog.ShowAsync();
                return;
            }
            this.Frame.Navigate(typeof(DatasetPage), RecentDatasetsList[datasetListView.SelectedIndex]);
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            appLog.Info(this.GetType().Name, "Welcome to ActViz.");
            this.RecentDatasetsList.Clear();
            // Read Recent Datasets from Future Access List
            var futureAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            foreach (var entry in futureAccessList.Entries)
            {
                try
                {
                    StorageFolder folder = await futureAccessList.GetFolderAsync(entry.Token);
                    Dataset curDataset = new Dataset();
                    await curDataset.FromStorageFolder(folder);
                    this.RecentDatasetsList.Add(curDataset);
                }
                catch (Exception)
                {
                    futureAccessList.Remove(entry.Token);
                }
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            Dataset selectedDataset = this.RecentDatasetsList[listView.SelectedIndex];
            string strDatasetReadme = "";
            try
            {
                StorageFile datasetReadme = await selectedDataset.Folder.GetFileAsync("Readme.txt");
                strDatasetReadme = await FileIO.ReadTextAsync(datasetReadme);
            }
            catch (Exception)
            { }
            this.txtDatasetSummary.Text = strDatasetReadme;
        }
    }
}
