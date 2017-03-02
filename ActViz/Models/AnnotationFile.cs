using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class AnnotationEntry
    {
        public DateTime TimeTag { get; set; }
        public string ResidentName { get; set; }
        public string ActivityLabel { get; set; }
        private List<Tuple<string, double>> _activityProbability = new List<Tuple<string, double>>();
        public List<Tuple<string, double>> ActivityProbability
        {
            get { return _activityProbability; }
            set { _activityProbability = value; }
        }

        public AnnotationEntry(string[] tokenList)
        {
            TimeTag = Convert.ToDateTime(tokenList[0] + " " + tokenList[1]);
            ActivityLabel = tokenList[2];
            if(tokenList.Length > 3)
            {
                ResidentName = tokenList[3];
            }
            else
            {
                ResidentName = "";
            }
        }
    }

    public class AnnotationFile
    {
        private Logging appLog = Logging.Instance;

        public string Name { get; set; }
        public string Path { get; set; }

        // List of Back-Annotated Predictions
        private List<AnnotationEntry> _predictionList = new List<AnnotationEntry>();
        private Dictionary<DateTime, int> _predictionOffsetDict = new Dictionary<DateTime, int>();
        private List<DateTime> _predictionDateList = new List<DateTime>();
        private int _predictionCount = 0;

        public List<AnnotationEntry> PredictionList { get { return _predictionList; } }

        // Get Start Offset
        public int GetOffsetByDate(DateTime date)
        {
            DateTime _curDate = date.Date;
            if(_predictionDateList.Contains(_curDate))
            {
                return _predictionOffsetDict[_curDate];
            } else {
                return -1;
            }
        }

        // Load From File
        public async Task<bool> LoadAnnotationFromFile(StreamReader streamReader)
        {
            appLog.Info(this.GetType().Name, "Start Loading Annotation...");
            int tmpLine;
            string[] tokenList;
            string[] oldTokenList = new string[] { };
            // Make sure the prediction list and dict are clean
            _predictionList.Clear();
            _predictionOffsetDict.Clear();
            _predictionCount = 0;
            // Start Loading Event
            while (streamReader.Peek() >= 0)
            {
                string curPredictionString = streamReader.ReadLine();
                // Get Date from it
                tokenList = curPredictionString.Split(new char[] { ',', ' '});
                if (oldTokenList.Count() == 0) oldTokenList = tokenList;
                DateTime TimeTag = Convert.ToDateTime(tokenList[0]);
                try
                {
                    _predictionList.Add(new AnnotationEntry(tokenList));
                    if (!_predictionOffsetDict.TryGetValue(TimeTag, out tmpLine))
                    {
                        _predictionOffsetDict.Add(TimeTag, _predictionCount);
                        _predictionDateList.Add(TimeTag);
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
                    _predictionCount++;
                }
                catch (Exception e)
                {
                    appLog.Error(this.GetType().Name, string.Format("Failed at line {0} with error message {1}", _predictionCount + 1, e.Message));
                    return false;
                }
            }
            appLog.Info(this.GetType().Name, string.Format("{0} annotations loaded.", _predictionCount));
            return true;
        }

        internal async Task<bool> LoadAnnotationProbFromFile(StreamReader streamReader)
        {
            appLog.Info(this.GetType().Name, "Start Loading Annotation Probability...");
            int i = 0;
            string[] tokenList;
            // Start Loading Event
            while (streamReader.Peek() >= 0)
            {
                string curPredictionString = streamReader.ReadLine();
                // Get Date from it
                tokenList = curPredictionString.Split(new char[] { ',', ' ' });
                DateTime TimeTag = Convert.ToDateTime(tokenList[0] + " " + tokenList[1]);
                if(TimeTag == _predictionList[i].TimeTag)
                {
                    for(int j = 2; j < tokenList.Length; j++)
                    {
                        if(tokenList[j] != "")
                        {
                            // Add Prediction Array
                            string[] subTokenList = tokenList[j].Split(new char[] { '(', ')' });
                            _predictionList[i].ActivityProbability.Add(new Tuple<string, double>(subTokenList[0], Convert.ToDouble(subTokenList[1])));
                        }
                    }
                }
                else
                {
                    appLog.Error(this.GetType().Name, string.Format("Time mismatch at line {0}: {1}", i, TimeTag));
                }
                i++;
            }
            return true;
        }

        public void UnloadPrediction()
        {
            _predictionList.Clear();
            _predictionOffsetDict.Clear();
        }
    }
}
