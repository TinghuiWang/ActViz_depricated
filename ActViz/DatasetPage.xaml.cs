using System;
using System.Collections.Generic;
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
using ActViz.Models;
using ActViz.ViewModels;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.ComponentModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DatasetPage : Page
    {
        private HomeViewModel _home;

        private Logging appLog = Logging.Instance;

        private bool isLogHorizontal = true;

        public DatasetPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _home = new HomeViewModel(e.Parameter as Dataset);
            MenuList.SelectedIndex = 0;
            base.OnNavigatedTo(e);
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            DatasetSplitView.IsPaneOpen = !DatasetSplitView.IsPaneOpen;
        }

        private async void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(_home != null)
            {
                if (_home.sensorEventCollection.NeedFlush)
                {
                    // Message Box Ask user if they want to save the modification
                    MessageDialog dlg = new MessageDialog("Event List is modified. Do you want to save your changes?", "Save event.csv");
                    dlg.Commands.Add(new UICommand("Yes") { Id = 0 });
                    dlg.Commands.Add(new UICommand("Cancel") { Id = 1 });
                    var result = await dlg.ShowAsync();
                    if (result.Label == "Yes")
                    {
                        await _home.SaveEvents();
                    }
                }
                switch (MenuList.SelectedIndex)
                {
                    case 0:
                        // Home
                        this.ActVizFrame.Navigate(typeof(HomePage), _home);
                        break;
                    case 1:
                        // Event View
                        try
                        {
                            this.ActVizFrame.Navigate(typeof(EventPage), _home);
                        }
                        catch (Exception except)
                        {
                            var dlg = new MessageDialog("Error loading Event Page.\n" + except.Message, "Error Loading Events");
                            await dlg.ShowAsync();
                            MenuList.SelectedIndex = 0;
                            return;
                        }
                        break;
                    case 2:
                        // Analysis View
                        this.ActVizFrame.Navigate(typeof(AnalysisPage), _home);
                        break;
                    case 3:
                        // Configure View
                        this.ActVizFrame.Navigate(typeof(ConfigPage), _home);
                        break;
                    case 4:
                        // Edit View
                        this.ActVizFrame.Navigate(typeof(EditView), _home);
                        break;
                    case 5:
                        // Exit
                        this.Frame.Navigate(typeof(MainPage));
                        break;
                    default:
                        return;
                }
            }
            else
            {
                MenuList.SelectedIndex = 0;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            HorizontalSplitter.ManipulationMode = ManipulationModes.All;
            HorizontalSplitter.ManipulationDelta += HorizontalSplitter_ManipulationDelta;
            VerticalSplitter.ManipulationMode = ManipulationModes.All;
            VerticalSplitter.ManipulationDelta += VerticalSplitter_ManipulationDelta;
        }

        #region Splitter
        private void HorizontalSplitter_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                gridLogText.Height = gridLogText.ActualHeight - e.Delta.Translation.Y;
            }
            catch
            {

            }
        }

        private void VerticalSplitter_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            try
            {
                gridLogText.Width = gridLogText.ActualWidth - e.Delta.Translation.X;
            }
            catch
            {

            }
        }
        #endregion

        private async void btnLogSave_Click(object sender, RoutedEventArgs e)
        {
            FileSavePicker dialog = new FileSavePicker();
            dialog.SuggestedStartLocation = PickerLocationId.Desktop;
            dialog.FileTypeChoices.Add("Log Files", new List<string>() { ".log" });
            dialog.SuggestedFileName = string.Format("ActViz_{0}", DateTime.Now.ToString("yyyyMMdd_HH_mm_ss"));
            StorageFile file = await dialog.PickSaveFileAsync();
            if(file != null)
            {
                await FileIO.WriteTextAsync(file, appLog.Log);
            }
        }

        private void LogPanelToggle_Checked(object sender, RoutedEventArgs e)
        {
            gridLogText.Visibility = Visibility.Visible;
            if (isLogHorizontal)
                HorizontalSplitter.Visibility = Visibility.Visible;
            else
                VerticalSplitter.Visibility = Visibility.Visible;
        }

        private void LogPanelToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            gridLogText.Visibility = Visibility.Collapsed;
            if (isLogHorizontal)
                HorizontalSplitter.Visibility = Visibility.Collapsed;
            else
                VerticalSplitter.Visibility = Visibility.Collapsed;
        }

        private void btnVerticalSplit_Click(object sender, RoutedEventArgs e)
        {
            Grid.SetRow(gridLogText, 0);
            Grid.SetRowSpan(gridLogText, 3);
            Grid.SetColumn(gridLogText, 2);
            Grid.SetColumnSpan(gridLogText, 1);
            Grid.SetRow(ActVizFrame, 0);
            Grid.SetRowSpan(ActVizFrame, 3);
            Grid.SetColumn(ActVizFrame, 0);
            Grid.SetColumnSpan(ActVizFrame, 1);
            VerticalSplitter.Visibility = Visibility.Visible;
            HorizontalSplitter.Visibility = Visibility.Collapsed;
            btnVerticalSplit.Visibility = Visibility.Collapsed;
            btnHorizontalSplit.Visibility = Visibility.Visible;
            gridLogText.Width = gridLogText.MinWidth;
            gridLogText.Height = double.NaN;
            UpdateLayout();
            isLogHorizontal = false;
        }

        private void btnHorizontalSplit_Click(object sender, RoutedEventArgs e)
        {
            Grid.SetColumn(gridLogText, 0);
            Grid.SetColumnSpan(gridLogText, 3);
            Grid.SetRow(gridLogText, 2);
            Grid.SetRowSpan(gridLogText, 1);
            Grid.SetColumn(ActVizFrame, 0);
            Grid.SetColumnSpan(ActVizFrame, 3);
            Grid.SetRow(ActVizFrame, 0);
            Grid.SetRowSpan(ActVizFrame, 1);
            HorizontalSplitter.Visibility = Visibility.Visible;
            VerticalSplitter.Visibility = Visibility.Collapsed;
            btnVerticalSplit.Visibility = Visibility.Visible;
            btnHorizontalSplit.Visibility = Visibility.Collapsed;
            gridLogText.Height = gridLogText.MinHeight;
            gridLogText.Width = double.NaN;
            UpdateLayout();
            isLogHorizontal = true;
        }
    }
}
