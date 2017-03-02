using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.Dialogs
{
    public sealed partial class AnalysisConfigDialog : ContentDialog
    {
        private Logging appLog = Logging.Instance;

        private ObservableCollection<AnnotationFile> _AnnotatedFilesList;
        public ObservableCollection<AnnotationFile> AnnotatedFilesList {
            get { return _AnnotatedFilesList; }
            set { _AnnotatedFilesList = value; }
        }

        private StorageFile _annotationFile = null;
        private StorageFile _annotationProbFile = null;
        private int _selectedIndex = -1;

        public AnalysisConfigDialog(ObservableCollection<AnnotationFile> annotationFileList = null, bool isOffStateHidden=false)
        {
            this.InitializeComponent();
            if (annotationFileList == null) {
                this._AnnotatedFilesList = new ObservableCollection<AnnotationFile>();
            } else {
                this._AnnotatedFilesList = annotationFileList;
            }
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async Task<bool> loadAnnotationFile()
        {
            bool result = false;
            appLog.Info(this.GetType().Name, string.Format("Loading annotation {0} from file {1}", 
                this.txtAnnotationFileName.Text, this.txtAnnotationFilePath.Text));
            AnnotationFile annotationFile = new AnnotationFile();
            annotationFile.Name = txtAnnotationFileName.Text;
            annotationFile.Path = _annotationFile.Path;
            using (var inputStream = await _annotationFile.OpenReadAsync())
            using (var classicStream = inputStream.AsStreamForRead())
            using (var streamReader = new StreamReader(classicStream))
            {
                result = await annotationFile.LoadAnnotationFromFile(streamReader);
                if (result) _AnnotatedFilesList.Add(annotationFile);
            }
            if (_annotationProbFile != null)
            {
                appLog.Info(this.GetType().Name, string.Format("Loading annotation probability of {0} from file {1}",
                    this.txtAnnotationFileName.Text, this.txtAnnotationProbFilePath.Text));
                using (var inputStream = await _annotationProbFile.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                using (var streamReader = new StreamReader(classicStream))
                {
                    result = await annotationFile.LoadAnnotationProbFromFile(streamReader);
                }
            }
            return result;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void btnAddAnnotation_Click(object sender, RoutedEventArgs e)
        {
            this.listview_AnnotatedFiles.IsEnabled = false;
            gridAnnotationConfig_Clear();
            gridAnnotationConfig_Enable();
            this.gridAnnotationFileConfig.Visibility = Visibility.Visible;
            this.btnAdd.Visibility = Visibility.Visible;
            this.btnAdd.IsEnabled = false;
            this.btnModify.Visibility = Visibility.Collapsed;
        }

        private void btnModifyAnnotation_Click(object sender, RoutedEventArgs e)
        {
            this.listview_AnnotatedFiles.IsEnabled = false;
            this.txtAnnotationFileName.Text = AnnotatedFilesList[_selectedIndex].Name;
            this.txtAnnotationFileName.IsEnabled = true;
            this.txtAnnotationFilePath.Text = AnnotatedFilesList[_selectedIndex].Path;
            this.gridAnnotationFileConfig.Visibility = Visibility.Visible;
            this.btnModify.Visibility = Visibility.Visible;
            this.btnModify.IsEnabled = false;
            this.btnAdd.Visibility = Visibility.Collapsed;
        }

        private void btnDeleteAnnotation_Click(object sender, RoutedEventArgs e)
        {
            _AnnotatedFilesList.Remove(listview_AnnotatedFiles.SelectedItem as AnnotationFile);
        }

        private async void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FileOpenPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFile file = await folderPicker.PickSingleFileAsync();
            if (file != null)
            {
                _annotationFile = file;
                this.txtAnnotationFilePath.Text = _annotationFile.Path;
                txtAnnotationFileName_TextChanged(sender, null);
            }
        }

        private async void btnOpenProbFile_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FileOpenPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            StorageFile file = await folderPicker.PickSingleFileAsync();
            if (file != null)
            {
                _annotationProbFile = file;
                this.txtAnnotationProbFilePath.Text = file.Path;
            }
        }

        private void gridAnnotationConfig_Clear()
        {
            txtAnnotationFileName.Text = "";
            txtAnnotationFilePath.Text = "";
            txtAnnotationProbFilePath.Text = "";
        }

        private void gridAnnotationConfig_Disable()
        {
            txtAnnotationFileName.IsEnabled = false;
            txtAnnotationFilePath.IsEnabled = false;
            txtAnnotationProbFilePath.IsEnabled = false;
            btnOpenFile.IsEnabled = false;
        }

        private void gridAnnotationConfig_Enable()
        {
            txtAnnotationFileName.IsEnabled = true;
            txtAnnotationFilePath.IsEnabled = true;
            txtAnnotationProbFilePath.IsEnabled = true;
            btnOpenFile.IsEnabled = true;
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            btnAdd.IsEnabled = false;
            gridAnnotationConfig_Disable();
            progbarLoadAnnotation.IsEnabled = true;
            progbarLoadAnnotation.IsIndeterminate = true;
            progbarLoadAnnotation.Visibility = Visibility.Visible;
            // Load the file
            if (await loadAnnotationFile())
            {
                gridAnnotationFileConfig.Visibility = Visibility.Collapsed;
                btnAdd.Visibility = Visibility.Collapsed;
                // Hide the table
                gridAnnotationFileConfig.Visibility = Visibility.Collapsed;
                btnAdd.Visibility = Visibility.Collapsed;
            }
            else
            {
                gridAnnotationConfig_Enable();
                btnAdd.IsEnabled = true;
            }
            this.listview_AnnotatedFiles.IsEnabled = true;
            progbarLoadAnnotation.IsEnabled = false;
            progbarLoadAnnotation.Visibility = Visibility.Collapsed;
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            AnnotatedFilesList[_selectedIndex].Name = txtAnnotationFileName.Text;
            // Hide the table
            this.listview_AnnotatedFiles.IsEnabled = true;
            gridAnnotationFileConfig.Visibility = Visibility.Collapsed;
            btnModify.Visibility = Visibility.Collapsed;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.listview_AnnotatedFiles.IsEnabled = true;
            btnAdd.Visibility = Visibility.Collapsed;
            btnAdd.IsEnabled = false;
            btnModify.Visibility = Visibility.Collapsed;
            btnModify.IsEnabled = false;
            gridAnnotationConfig_Disable();
            gridAnnotationFileConfig.Visibility = Visibility.Collapsed;
        }

        private void txtAnnotationFileName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtAnnotationFileName.Text != "" && txtAnnotationFilePath.Text != "")
            {
                if (btnAdd.Visibility == Visibility.Visible)
                {
                    // Add a new annotation file
                    btnAdd.IsEnabled = true;
                    return;
                }
                if (btnModify.Visibility == Visibility.Visible)
                {
                    // Modify an existing annotation
                    if (txtAnnotationFileName.Text != AnnotatedFilesList[_selectedIndex].Name || 
                        txtAnnotationFilePath.Text != AnnotatedFilesList[_selectedIndex].Path)
                    {
                        btnModify.IsEnabled = true;
                    }
                    else
                    {
                        btnModify.IsEnabled = false;
                    }
                    return;
                }
            } else
            {
                btnAdd.IsEnabled = false;
                btnModify.IsEnabled = false;
            }
        }

        private void listview_AnnotatedFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnModifyAnnotation.IsEnabled = true;
            btnDeleteAnnotation.IsEnabled = true;
            _selectedIndex = listview_AnnotatedFiles.SelectedIndex;
        }
    }
}
