using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Provider;

namespace ActViz.ViewModels
{
    public class HomeViewModel : NotificationBase
    {
        private Logging appLog = Logging.Instance;
        private Dataset _Dataset;

        private SensorEventCollection _sensorEventCollection = new SensorEventCollection();
        public SensorEventCollection sensorEventCollection { get { return _sensorEventCollection; } set { SetProperty(ref _sensorEventCollection, value); } }

        private int _SelectedEventIndex;
        public int SelectedEventIndex
        {
            get { return _SelectedEventIndex; }
            set {
                if (SetProperty(ref _SelectedEventIndex, value))
                {
                    RaisePropertyChanged(nameof(SelectedEvent));
                }
            }
        }
        public SensorEventViewModel SelectedEvent
        {
            get { return (_SelectedEventIndex >= 0 && _SelectedEventIndex < _sensorEventCollection.Count) ? (SensorEventViewModel) _sensorEventCollection[_SelectedEventIndex] : null; }
        }

        private Dictionary<string, int> _lastFiredSensorStat = new Dictionary<string, int>();
        private int _lastSelectedUnfilteredEventIndex = 0;
        public Dictionary<string, int> SensorLastFireStat { get { return _lastFiredSensorStat; } }

        /// <summary>
        /// Get Sensor Firing Status - so it can be displayed after apply a SORT function on time.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetSensorFireStatus()
        {
            int unfilteredIndex = _sensorEventCollection.UnfilteredStorage.IndexOf(SelectedEvent);
            if(unfilteredIndex <= 0)
            {
                foreach(string sensorName in _lastFiredSensorStat.Keys.ToList())
                {
                    _lastFiredSensorStat[sensorName] = -1;
                }
                _lastFiredSensorStat[sensorEventCollection.UnfilteredStorage[0].SensorName] = 0;
            }
            else
            {
                // If Going Forward - Only need to update the difference
                if (unfilteredIndex > _lastSelectedUnfilteredEventIndex)
                {
                    for (int index = unfilteredIndex; index > _lastSelectedUnfilteredEventIndex; index--)
                    {
                        string sensorName = sensorEventCollection.UnfilteredStorage[index].SensorName;
                        if (_lastFiredSensorStat[sensorName] < index)
                        {
                            _lastFiredSensorStat[sensorName] = index;
                        }
                    }
                }
                if(unfilteredIndex < _lastSelectedUnfilteredEventIndex)
                {
                    // If Going Backward - Update For those with index greater than current selected index
                    HashSet<string> TargetSensorSet = new HashSet<string>();
                    // Find what sensors need update
                    foreach(string sensorName in _lastFiredSensorStat.Keys.ToList())
                    {
                        if(_lastFiredSensorStat[sensorName] > unfilteredIndex)
                        {
                            _lastFiredSensorStat[sensorName] = -1;
                            TargetSensorSet.Add(sensorName);
                        }
                    }
                    // Trace back until all those sensorName are updated
                    for(int index = unfilteredIndex; index >= 0; index--)
                    {
                        string sensorName = _sensorEventCollection.UnfilteredStorage[index].SensorName;
                        if (TargetSensorSet.Contains(sensorName))
                        {
                            _lastFiredSensorStat[sensorName] = index;
                            TargetSensorSet.Remove(sensorName);
                        }
                        if (TargetSensorSet.Count == 0)
                            break;
                    }
                }
            }
            _lastSelectedUnfilteredEventIndex = unfilteredIndex;
            return _lastFiredSensorStat;
        }

        public List<KeyValuePair<string, int>> GetSensorFireStatusSorted()
        {
            GetSensorFireStatus();
            var SensorFireStatusSorted = from entry in _lastFiredSensorStat orderby entry.Value descending select entry;
            return SensorFireStatusSorted.ToList();
        }

        public void InitSensorFireStatus()
        {
            _lastFiredSensorStat.Clear();
            foreach(Sensor sensor in dataset.SensorList)
            {
                _lastFiredSensorStat.Add(sensor.name, -1);
            }
        }

        private ObservableCollection<ActivityViewModel> _activityCollection = new ObservableCollection<ActivityViewModel>();
        public ObservableCollection<ActivityViewModel> ActivityCollection { get { return _activityCollection; } }

        private ObservableCollection<ResidentViewModel> _residentCollection = new ObservableCollection<ResidentViewModel>();
        public ObservableCollection<ResidentViewModel> ResidentCollection { get { return _residentCollection; } }

        #region ConfigFlags
        private bool _isNameChanged = false;
        public bool IsNameChanged { get { return _isNameChanged; } set { SetProperty(ref _isNameChanged, value); } }
        private bool _isActivityChanged = false;
        public bool IsActivityChanged { get { return _isActivityChanged; } set { SetProperty(ref _isActivityChanged, value); } }
        private bool _isResidentChanged = false;
        public bool IsResidentChanged { get { return _isResidentChanged; } set { SetProperty(ref _isResidentChanged, value); } }
        #endregion

        public Dataset dataset { get { return _Dataset; } }

        public string DatasetName { get { return _Dataset.Name; } set { _Dataset.Name = value; RaisePropertyChanged(nameof(DatasetName)); } }

        public HomeViewModel(Dataset dataset)
        {
            _Dataset = dataset;
            _SelectedEventIndex = -1;
            // Construct Activity Collection and Resident Collection
            foreach (Activity activity in dataset.ActivityList)
            {
                _activityCollection.Add(new ActivityViewModel(activity));
            }
            foreach (Resident resident in dataset.ResidentList)
            {
                _residentCollection.Add(new ResidentViewModel(resident));
            }
        }

        #region ActivityList
        public void AddActivity(ActivityViewModel activityViewModel)
        {
            _activityCollection.Add(activityViewModel);
            _Dataset.AddActivity(activityViewModel.activity);
        }

        public void RemoveActivity(ActivityViewModel activityViewModel)
        {
            _activityCollection.Remove(activityViewModel);
            _Dataset.RemoveActivity(activityViewModel.activity);
        }
        #endregion

        #region ResidentList
        public void AddResident(ResidentViewModel residentViewModel)
        {
            _residentCollection.Add(residentViewModel);
            _Dataset.AddResident(residentViewModel.resident);
        }

        public void RemoveResident(ResidentViewModel residentViewModel)
        {
            foreach(ResidentViewModel resident in _residentCollection)
            {
                if(resident.id > residentViewModel.id)
                {
                    resident.id -= 1;
                }
            }
            _residentCollection.Remove(residentViewModel);
            _Dataset.RemoveResident(residentViewModel.resident);
        }
        #endregion

        public async Task<int> LoadDateAsync(DateTime datetime, ObservableCollection<AnnotationFile> annotationFileList = null, int numBefore = 10)
        {
            int count = await _sensorEventCollection.LoadDateAsync(datetime, annotationFileList, _activityCollection, numBefore);
            SelectedEventIndex = -1;
            return count;
        }

        /// <summary>
        /// LoadEvents() is called when user navigates to Analysis page or Event page to display
        /// a list of user events. This function looks and see if sensor events in a dataset has
        /// loaded into _sensorEventCollection. If no event has been loaded, it opens the event.csv
        /// file and load the events into dataset. Otherwise, it just prepares the first day in
        /// in the events list and display that.
        /// </summary>
        public async Task LoadEvents()
        {
            if(_sensorEventCollection.Count == 0)
            {
                // Get Storage File and a Read Stream
                StorageFile eventFile = await _Dataset.Folder.GetFileAsync("events.csv");
                using (var inputStream = await eventFile.OpenReadAsync())
                using (var classicStream = inputStream.AsStreamForRead())
                using (var streamReader = new StreamReader(classicStream))
                {
                    await _sensorEventCollection.LoadEventFromFile(streamReader);
                }
            }
            else
            {
                await _sensorEventCollection.LoadDateAsync(_sensorEventCollection.FirstEventDate);
            }
        }

        public async Task SaveEvents()
        {
            // Get Storage File and a Read Stream
            StorageFile eventFile = await _Dataset.Folder.GetFileAsync("events.csv");
            CachedFileManager.DeferUpdates(eventFile);
            await FileIO.WriteTextAsync(eventFile, "");
            FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(eventFile);
            using (var outputStream = await eventFile.OpenAsync(FileAccessMode.ReadWrite))
            using (var classicStream = outputStream.AsStreamForWrite())
            using (var streamWriter = new StreamWriter(classicStream))
            {
                await _sensorEventCollection.FlushToFile(streamWriter);
            }
        }
    }
}
