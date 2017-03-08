using ActViz.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace ActViz.ViewModels
{
    public class SensorEventCollection : IList, INotifyCollectionChanged
    {
        private Logging appLog = Logging.Instance;

        private bool _isOffStateHidden = true;
        public bool IsOffStateHidden
        {
            get { return _isOffStateHidden; }
            set {
                _isOffStateHidden = value;
                SetOffStateVisibility();
            }
        }

        void SetOffStateVisibility()
        {
            foreach(var sensorEventEntry in _storage)
            {
                sensorEventEntry.IsVisible = (_isOffStateHidden && sensorEventEntry.SensorState.ToUpper() == "OFF") ? false : true;
            }
        }

        #region IList
        public int Count { get { return _storage.Count; } }
        public bool IsReadOnly { get { return true; } }
        public bool IsFixedSize { get { return true; } }
        public bool IsSynchronized { get { return true; } }

        public object SyncRoot
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        object IList.this[int index]
        {
            get
            {
                return ((IList)_storage)[index];
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public SensorEventViewModel this[int index]
        {
            get
            {
                if (index < 0 || index >= Count) return null;
                else return _storage[index];
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            return ((IList)_storage).Contains(value);
        }

        public int IndexOf(object value)
        {
            return ((IList)_storage).IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            ((IList)_storage).CopyTo(array, index);
        }

        public int IndexOf(SensorEventViewModel item)
        {
            return _storage.IndexOf(item);
        }

        public void Insert(int index, SensorEventViewModel item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void Add(SensorEventViewModel item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(SensorEventViewModel item)
        {
            return _storage.Contains(item);
        }

        public void CopyTo(SensorEventViewModel[] array, int arrayIndex)
        {
            _storage.CopyTo(array, arrayIndex);
        }

        public bool Remove(SensorEventViewModel item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<SensorEventViewModel> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _storage.GetEnumerator();
        }
        #endregion

        #region State
        public List<SensorEventViewModel> UnfilteredStorage { get { return _internal_storage; } }
        // The Actual List
        private List<SensorEventViewModel> _storage;
        private List<SensorEventViewModel> _internal_storage;
        // List of event strings
        private List<string> _eventList;
        private int _eventCount;
        // Holds Event No. to File Offset Translation for faster locating
        private Dictionary<DateTime, int> _eventOffsetList;
        private List<DateTime> _eventDateList;
        // First Event Date and Last Event Date
        private DateTime _firstEventDate;
        private DateTime _lastEventDate;
        private DateTime _curDate;

        // Record Changed Items
        private HashSet<int> _changedIndices = new HashSet<int>();

        // Async Loading Sync Flag
        private bool _busy = false;

        // CollectionChanged Event
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // Public Properties
        public DateTime FirstEventDate { get { return _firstEventDate; } }
        public DateTime LastEventDate { get { return _lastEventDate; } }
        public DateTime CurDate { get { return _curDate; } }
        public bool NeedFlush { get { return _changedIndices.Count > 0; } }
        public string log { get; set; }
        #endregion

        #region Setup
        public SensorEventCollection()
        {
            // Initialize private variables
            _storage = new List<SensorEventViewModel>();
            _internal_storage = new List<SensorEventViewModel>();
            _eventList = new List<string>();
            _eventDateList = new List<DateTime>();
            _eventOffsetList = new Dictionary<DateTime, int>();
            log = "";
        }

        /// <summary>
        /// Load sensor events from event.csv file through StreamReader interface.
        /// </summary>
        /// <param name="streamReader"></param>
        /// <returns></returns>
        public async Task LoadEventFromFile(StreamReader streamReader)
        {
            appLog.Info(this.GetType().Name, "Loading events...");
            // Local parameters
            int tmpLine;
            string[] tokenList;
            string[] oldTokenList = new string[] { };
            // Reset Event Count
            _eventCount = 0;
            // Clear Event List and Dictionary
            _eventOffsetList.Clear();
            _eventList.Clear();
            // Start Loading Event
            while (streamReader.Peek() >= 0)
            {
                string curEventString = streamReader.ReadLine();
                try
                {
                    tokenList = curEventString.Split(new char[] { ',' });
                    if (oldTokenList.Count() == 0) oldTokenList = tokenList;
                    _eventList.Add(curEventString);
                    // Get the Date of the String and add to dictionary
                    DateTime TimeTag = Convert.ToDateTime(tokenList[0]);
                    if (!_eventOffsetList.TryGetValue(TimeTag, out tmpLine))
                    {
                        // Cannot get value of the timetag
                        _eventOffsetList.Add(TimeTag, _eventCount);
                        _eventDateList.Add(TimeTag);
                        // Test and see if there is time leap
                        DateTime firstEventNewDay = Convert.ToDateTime(tokenList[0] + " " + tokenList[1]);
                        DateTime lastEventlastDay = Convert.ToDateTime(oldTokenList[0] + " " + oldTokenList[1]);
                        var TimeDifference = firstEventNewDay - lastEventlastDay;
                        if (TimeDifference.TotalDays >= 0.5)
                        {
                            appLog.Warn(this.GetType().Name, string.Format("Time Leap {0} days {1}H {2}M {3}s after {4}", TimeDifference.Days, TimeDifference.Hours,
                                TimeDifference.Minutes, TimeDifference.Seconds, lastEventlastDay.ToString("G")));
                        }
                    }
                    oldTokenList = tokenList;
                    _eventCount++;
                }
                catch (Exception e)
                {
                    appLog.Error(this.GetType().Name, string.Format("Failed at line {0} with error message {1}", _eventCount + 1, e.Message));
                }
            }
            appLog.Info(this.GetType().Name, string.Format("Finished loading {0} events from dataset.", _eventCount));
            // Display First Day Events
            _firstEventDate = _eventDateList.First();
            _lastEventDate = _eventDateList.Last();
            // Load First Day
            await LoadDateAsync(_firstEventDate);
        }

        public Windows.Foundation.IAsyncOperation<int> LoadDateAsync(DateTime datetime, ObservableCollection<AnnotationFile> annotationFileList = null,
            ObservableCollection<ActivityViewModel> activityCollection = null, int numBefore = 10)
        {
            if (_busy)
            {
                throw new InvalidOperationException("Only one operation in flight at a time");
            }

            _busy = true;

            DateTime dateToLoad = datetime;
            if (datetime == null) dateToLoad = _firstEventDate;

            return AsyncInfo.Run((c) => LoadDateAsync(c, dateToLoad, annotationFileList, activityCollection, numBefore));
        }

        private async Task<int> LoadDateAsync(CancellationToken c, DateTime datetime, 
            ObservableCollection<AnnotationFile> annotationFileList = null,
            ObservableCollection<ActivityViewModel> activityCollection = null, int numBefore = 10)
        {
            List<int> annotationStartOffset = new List<int>();
            List<int> annotationCurIndex = new List<int>();
            appLog.Info(this.GetType().Name, string.Format("Loading events on {0} into Listview.", datetime.ToString("D")));
            try
            {
                DateTime dateTimeToLoad = datetime.Date;
                int offset = 0;
                int nextOffset = 0;
                int Count = 0;
                if (dateTimeToLoad < _firstEventDate)
                {
                    offset = _eventOffsetList[_firstEventDate];
                    nextOffset = _eventOffsetList[_eventDateList[1]];
                    _curDate = _firstEventDate;
                }
                else if (dateTimeToLoad >= _lastEventDate)
                {
                    offset = _eventOffsetList[_lastEventDate];
                    nextOffset = _eventList.Count;
                    _curDate = _lastEventDate;
                }
                else
                {
                    while (!_eventOffsetList.TryGetValue(dateTimeToLoad, out offset))
                    {
                        dateTimeToLoad = dateTimeToLoad.AddDays(1);
                    }
                    nextOffset = _eventOffsetList[_eventDateList[_eventDateList.IndexOf(dateTimeToLoad) + 1]];
                    _curDate = dateTimeToLoad;
                }
                Count = nextOffset - offset;

                // Prepare tuple for start location for each annotated file
                if (annotationFileList != null)
                {
                    for (int i = 0; i < annotationFileList.Count; i++)
                    {
                        int startOffset = annotationFileList[i].GetOffsetByDate(_curDate);
                        annotationStartOffset.Add(startOffset);
                        annotationCurIndex.Add(startOffset);
                    }
                }

                _internal_storage.Clear();

                for (int i = offset; i < nextOffset; i++)
                {
                    SensorEventViewModel sensorEventViewModel = new SensorEventViewModel(new Models.SensorEvent());
                    sensorEventViewModel.FromString(_eventList[i]);
                    sensorEventViewModel.index = i - offset;
                    if(_isOffStateHidden && sensorEventViewModel.SensorState.ToUpper() == "OFF")
                    {
                        sensorEventViewModel.Skip = true;
                        sensorEventViewModel.IsVisible = false;
                    }
                    _internal_storage.Add(sensorEventViewModel);
                    // Populate annotation if specified
                    if (annotationFileList != null)
                    {
                        var emptyProbability = new List<Tuple<string, double>>();
                        for (int j = 0; j < annotationFileList.Count; j++)
                        {
                            ClassifiedLabelViewModel predictionModel = new ClassifiedLabelViewModel();
                            predictionModel.GroundTruth = sensorEventViewModel.ActivityLabel;
                            if (annotationCurIndex[j] <= 0)
                            {
                                predictionModel.ActivityLabel = "";
                            }
                            else
                            {
                                var curPredictionEntry = annotationFileList[j].PredictionList[annotationCurIndex[j]];
                                var lastPredictionEntry = (annotationCurIndex[j] == 0) ? null : annotationFileList[j].PredictionList[annotationCurIndex[j] - 1];
                                string previousLabel = (lastPredictionEntry == null) ? "" : lastPredictionEntry.ActivityLabel;
                                List<Tuple<string, double>> previousProbability = (lastPredictionEntry == null) ? emptyProbability : lastPredictionEntry.ActivityProbability;
                                if (sensorEventViewModel.TimeTag >= curPredictionEntry.TimeTag)
                                {
                                    predictionModel.ActivityLabel = curPredictionEntry.ActivityLabel;
                                    predictionModel.ActivityProbability = curPredictionEntry.ActivityProbability;
                                    annotationCurIndex[j]++;
                                } else
                                {
                                    predictionModel.ActivityLabel = previousLabel;
                                    predictionModel.ActivityProbability = previousProbability;
                                }
                            }
                            sensorEventViewModel.ClassifiedActivityLabels.Add(predictionModel);
                        }
                    }
                }

                // Apply Filter if necessary
                if (activityCollection != null) ApplyActivityFilters(activityCollection);
                // Populate list view
                Refresh();
                return Count;
            }
            finally
            {
                _busy = false;
            }
        }

        public void Refresh()
        {
            _storage.Clear();
            // Reset Event Fired
            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            CollectionChanged(this, args);
            
            foreach(var sensorEvent in _internal_storage)
            {
                if(!sensorEvent.Skip && sensorEvent.IsVisible)
                {
                    _storage.Add(sensorEvent);
                }
            }

            // Now notify of the new items
            NotifyOfInsertedItems(0, _storage.Count);
            appLog.Info(this.GetType().Name, string.Format("{0} events loaded.", _storage.Count));
            return;
        }

        public void ApplyActivityFilters(ObservableCollection<ActivityViewModel> activityCollection, int numBefore = 10)
        {
            int i = 0;
            Dictionary<string, bool> activityVisibile = new Dictionary<string, bool>();
            foreach(var activity in activityCollection)
            {
                activityVisibile.Add(activity.name, activity.isVisible);
            }
            for (i = 0; i < _internal_storage.Count; i++)
            {
                string activityLabel = _internal_storage[i].ActivityLabel;
                _internal_storage[i].IsSaparatorVisible = false;
                _internal_storage[i].Skip = false;
                // Check Off State Hidden First
                if (_isOffStateHidden && _internal_storage[i].SensorState.ToUpper() == "OFF")
                {
                    _internal_storage[i].Skip = true;
                    _internal_storage[i].IsVisible = false;
                    continue;
                }
                // Check if one of the predicted label is of visibility
                bool visible = activityVisibile[activityLabel];
                foreach(var _classifiedActivityLabel in _internal_storage[i].ClassifiedActivityLabels)
                {
                    if(_classifiedActivityLabel.ActivityLabel != "")
                    {
                        visible |= activityVisibile[_classifiedActivityLabel.ActivityLabel];
                    }
                }
                // Mark visibility
                if (visible) {
                    _internal_storage[i].IsVisible = true;
                    // Back trace
                    int j = 1;
                    int beforeCount = 0;
                    while(beforeCount <= numBefore)
                    {
                        if (i - j >= 0)
                        {
                            if (_internal_storage[i - j].IsVisible)
                                break;
                            if (!_internal_storage[i - j].Skip)
                            {
                                _internal_storage[i - j].IsVisible = true;
                                beforeCount++;
                            }
                            j++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(beforeCount >= numBefore)
                    {
                        _internal_storage[i - j + 1].IsSaparatorVisible = true;
                    }
                }
                else
                {
                    // Hide it.
                    _internal_storage[i].IsVisible = false;
                }
            }
        }

        void NotifyOfInsertedItems(int baseIndex, int count)
        {
            if (CollectionChanged == null)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _storage[i + baseIndex], i + baseIndex);
                CollectionChanged(this, args);
            }
        }
        #endregion

        public void HideStateOffEvents()
        {
            foreach(var eventEntry in _storage)
            {
                if(eventEntry.SensorState.ToUpper() == "OFF")
                {
                    eventEntry.IsVisible = false;
                }
            }
        }

        public void ShowStateOffEvents()
        {
            foreach (var eventEntry in _storage)
            {
                if (eventEntry.SensorState.ToUpper() == "OFF")
                {
                    eventEntry.IsVisible = true;
                }
            }
        }

        public void TagActivities(string activity, List<SensorEventViewModel> eventList)
        {
            int startIndex = _eventOffsetList[_curDate];
            foreach (SensorEventViewModel sensorEvent in eventList)
            {
                int index = sensorEvent.index;
                if(!_changedIndices.Contains(startIndex + index))
                {
                    _changedIndices.Add(startIndex + index);
                }
                _storage[index].ActivityLabel = activity;
                _eventList[startIndex + index] = _storage[index].ToString();
            }
        }

        public void TagResidents(string resident, List<SensorEventViewModel> eventList)
        {
            int startIndex = _eventOffsetList[_curDate];
            foreach (SensorEventViewModel sensorEvent in eventList)
            {
                int index = sensorEvent.index;
                if (!_changedIndices.Contains(startIndex + index))
                {
                    _changedIndices.Add(startIndex + index);
                }
                _storage[index].OccupantId = resident;
                _eventList[startIndex + index] = _storage[index].ToString();
            }
        }

        public async Task FlushToFile(StreamWriter streamWriter)
        {
            if(_changedIndices.Count > 0)
            {
                foreach(string eventString in _eventList)
                {
                    streamWriter.WriteLine(eventString);
                }
            }
            _changedIndices.Clear();
        }
    }
}
