using ActViz.ConfigPanels;
using ActViz.ViewModels;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ActViz
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConfigPage : Page
    {
        private HomeViewModel _home;
        private ObservableCollection<ConfigPanelItem> ConfigPanels;

        public ConfigPage()
        {
            this.InitializeComponent();
            ConfigPanels = new ObservableCollection<ConfigPanelItem>();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Get cur dataset from Navigation Parameter
            _home = e.Parameter as HomeViewModel;
            // Recreate Config Panels
            ConfigPanels.Clear();
            // General Config Panel
            GeneralConfigPanel generalConfigPanel = new GeneralConfigPanel();
            generalConfigPanel.Home = _home;
            ActivityConfigPanel activityConfigPanel = new ActivityConfigPanel();
            activityConfigPanel.Home = _home;
            ResidentConfigPanel residentConfigPanel = new ResidentConfigPanel();
            residentConfigPanel.Home = _home;
            ConfigPanels.Add(new ConfigPanelItem("General Configuration", generalConfigPanel));
            ConfigPanels.Add(new ConfigPanelItem("Activities", activityConfigPanel));
            ConfigPanels.Add(new ConfigPanelItem("Residents", residentConfigPanel));
        }
    }

    public class ConfigPanelItem
    {
        public string Name { get; set; }
        public Page ConfigPanel { get; set; }

        public ConfigPanelItem(string name, Page panel)
        {
            Name = name;
            ConfigPanel = panel;
        }
    }
}
