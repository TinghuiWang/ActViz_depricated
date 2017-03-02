using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using ActViz.ViewModels;
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
    public sealed partial class AnalysisFilterDialog : ContentDialog
    {
        private Logging appLog = Logging.Instance;
        private ObservableCollection<ActivityViewModel> activityCollection;
        private Dictionary<string, bool> _activityVisibilityBackup = new Dictionary<string, bool>();
        public bool isOffStateHidden;

        private void _BackupActivityVisibility()
        {
            bool _visibilityPlaceHolder;
            foreach(ActivityViewModel _activityViewModel in activityCollection)
            {
                string activityName = _activityViewModel.name;
                if(_activityVisibilityBackup.TryGetValue(activityName, out _visibilityPlaceHolder)) {
                    _activityVisibilityBackup[activityName] = _activityViewModel.isVisible;
                } else {
                    _activityVisibilityBackup.Add(activityName, _activityViewModel.isVisible);
                }
            }
        }

        private void _RestoreActivityVisibility()
        {
            foreach (ActivityViewModel _activityViewModel in activityCollection)
            {
                string activityName = _activityViewModel.name;
                try
                {
                    _activityViewModel.isVisible = _activityVisibilityBackup[activityName];
                }
                catch(Exception e)
                {
                    appLog.Error(this.GetType().Name, string.Format("Restore Activity Visibility failed with message: {0}", e.Message));
                }
            }
        }

        public AnalysisFilterDialog(ObservableCollection<ActivityViewModel> activityCollection, bool isOffStateHidden = false)
        {
            this.activityCollection = activityCollection;
            _BackupActivityVisibility();
            this.isOffStateHidden = isOffStateHidden;
            this.InitializeComponent();
            this.tgOffHidden.IsOn = !this.isOffStateHidden;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.isOffStateHidden = !tgOffHidden.IsOn;
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Reverse the change
            _RestoreActivityVisibility();
        }
    }
}
