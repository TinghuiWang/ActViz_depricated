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
    public sealed partial class ActivityConfigPanel : Page
    {
        private HomeViewModel _home;
        public HomeViewModel Home { get { return _home; } set { _home = value; } }

        public ActivityConfigPanel()
        {
            this.InitializeComponent();
        }

        private async void btnAddActivity_Click(object sender, RoutedEventArgs e)
        {
            AddActivityDialog dlg = new AddActivityDialog();
            dlg.MinWidth = Window.Current.Bounds.Width * 0.8;
            dlg.MaxWidth = Window.Current.Bounds.Width * 0.8;
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                _home.AddActivity(dlg.activityViewModel);
                await _home.dataset.SaveToFolder();
            }
        }

        private async void btnDeleteActivity_Click(object sender, RoutedEventArgs e)
        {
            ActivityViewModel selectedActivity = ActivityListView.SelectedItem as ActivityViewModel;
            string message = "Do you want to remove activity " + selectedActivity.name + "?";
            var dlg = new MessageDialog(message, "Remove Activity");
            dlg.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(this.DeleteActivityDialogHandler)));
            dlg.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(this.DeleteActivityDialogHandler)));
            dlg.DefaultCommandIndex = 1;
            dlg.CancelCommandIndex = 1;
            await dlg.ShowAsync();
        }

        private async void DeleteActivityDialogHandler(IUICommand command)
        {
            if(command.Label == "Yes")
            {
                _home.RemoveActivity(ActivityListView.SelectedItem as ActivityViewModel);
                await _home.dataset.SaveToFolder();
            }
        }

        private async void btnModifyActivity_Click(object sender, RoutedEventArgs e)
        {
            ConfigActivityDialog dlg = new ConfigActivityDialog(ActivityListView.SelectedItem as ActivityViewModel);
            dlg.MinWidth = Window.Current.Bounds.Width * 0.8;
            dlg.MaxWidth = Window.Current.Bounds.Width * 0.8;
            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                await _home.dataset.SaveToFolder();
            }
        }
    }
}
