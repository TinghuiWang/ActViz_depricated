using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.ViewModels
{
    /// <summary>
    /// This is the sensor event view model containing all the property to display a sensor event
    /// within a ListView controller.
    /// 
    /// Firstly, it is based on SensorEvent class that represents the underlying sensor event data
    /// collected in a smart home (with ground truth).
    /// 
    /// To add analysis functionality, the SensorEventViewModel is enriched with additional properties
    /// including ClassifiedActivityLabels (for loading predicted activity labels by ML algorithms),
    /// IsVisible property (for hiding current events according to user filtering or other needs of
    /// data analysis).
    /// </summary>
    public class SensorEventViewModel : NotificationBase<SensorEvent>
    {
        public SensorEventViewModel(SensorEvent sensorEvent = null) : base(sensorEvent) {
            this.ClassifiedActivityLabels = new ObservableCollection<ClassifiedLabelViewModel>();
        }

        private ObservableCollection<ClassifiedLabelViewModel> _ClassifiedActivityLabels;
        public ObservableCollection<ClassifiedLabelViewModel> ClassifiedActivityLabels {
            get { return this._ClassifiedActivityLabels; }
            set { this._ClassifiedActivityLabels = value; }
        }

        public int index { get; set; }

        public DateTime TimeTag
        {
            get { return This.TimeTag; }
            set { SetProperty(This.TimeTag, value, () => This.TimeTag = value); }
        }

        public string SensorName
        {
            get { return This.SensorName; }
            set { SetProperty(This.SensorName, value, () => This.SensorName = value); }
        }

        public string SensorType
        {
            get { return This.SensorType; }
            set { SetProperty(This.SensorType, value, () => This.SensorType = value); }
        }

        public string SensorState
        {
            get { return This.SensorState; }
            set { SetProperty(This.SensorState, value, () => This.SensorState = value); }
        }

        public string OccupantId
        {
            get { return This.OccupantId; }
            set { SetProperty(This.OccupantId, value, () => This.OccupantId = value); }
        }

        public string ActivityLabel
        {
            get { return This.ActivityLabel; }
            set {
                SetProperty(This.ActivityLabel, value, () => This.ActivityLabel = value);
                // Ground truth modified, it needs to be broadcasted to prediction labels as well.
                for (int i = 0; i < ClassifiedActivityLabels.Count; i++)
                {
                    ClassifiedActivityLabels[i].GroundTruth = This.ActivityLabel;
                }
            }
        }

        public string Comments {
            get { return This.Comments; }
            set { SetProperty(This.Comments, value, () => This.Comments = value); }
        }

        private bool _skip = false;
        public bool Skip
        {
            get { return _skip; }
            set { SetProperty(ref _skip, value); }
        }

        private bool _isVisible = true;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        private bool _isSaparatorVisible = false;
        public bool IsSaparatorVisible
        {
            get { return _isSaparatorVisible; }
            set { SetProperty(ref _isSaparatorVisible, value); }
        }

        public void FromString(string curEventEntry)
        {
            This.FromString(curEventEntry);
            RaisePropertyChanged(String.Empty);
        }

        public override string ToString()
        {
            return This.ToString();
        }
    }
}
