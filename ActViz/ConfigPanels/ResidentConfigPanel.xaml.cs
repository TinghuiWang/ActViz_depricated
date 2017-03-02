using ActViz.Dialogs;
using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz.ConfigPanels
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResidentConfigPanel : Page
    {
        private HomeViewModel _home;
        public HomeViewModel Home { get { return _home; } set { _home = value; } }

        public ResidentConfigPanel()
        {
            this.InitializeComponent();
        }

        private async void btnAddResident_Click(object sender, RoutedEventArgs e)
        {
            AddResidentDialog dlg = new AddResidentDialog(_home.ResidentCollection.Count);
            dlg.MinWidth = Window.Current.Bounds.Width * 0.8;
            dlg.MaxWidth = Window.Current.Bounds.Width * 0.8;
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _home.AddResident(dlg.residentViewModel);
                await _home.dataset.SaveToFolder();
            }
        }

        private async void btnModifyResident_Click(object sender, RoutedEventArgs e)
        {
            ConfigResidentDialog dlg = new ConfigResidentDialog(ResidentListView.SelectedItem as ResidentViewModel);
            dlg.MinWidth = Window.Current.Bounds.Width * 0.8;
            dlg.MaxWidth = Window.Current.Bounds.Width * 0.8;
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _home.dataset.SaveToFolder();
            }
        }

        private async void btnDeleteResident_Click(object sender, RoutedEventArgs e)
        {
            ResidentViewModel selectedResident = ResidentListView.SelectedItem as ResidentViewModel;
            string message = "Do you want to remove resident " + selectedResident.name + "?";
            var dlg = new MessageDialog(message, "Remove Resident");
            dlg.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.DeleteResidentDialogHandler)));
            dlg.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.DeleteResidentDialogHandler)));
            dlg.DefaultCommandIndex = 1;
            dlg.CancelCommandIndex = 1;
            await dlg.ShowAsync();
        }

        private async void DeleteResidentDialogHandler(IUICommand command)
        {
            if (command.Label == "Yes")
            {
                _home.RemoveResident(ResidentListView.SelectedItem as ResidentViewModel);
                await _home.dataset.SaveToFolder();
            }
        }
    }
}
