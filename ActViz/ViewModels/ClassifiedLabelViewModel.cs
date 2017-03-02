using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ActViz.ViewModels
{
    public class ClassifiedLabelViewModel : NotificationBase<ClassifiedLabel>
    {
        private static Color WrongPrediction = Colors.Red;
        private static Color CorrectPrediction = Colors.Green;
        private static Color IgnorePrediction = Colors.Yellow;
        private static Color NoPrediction = Colors.LightGray;

        private string _groundTruth = "";
        public string GroundTruth
        {
            get { return _groundTruth; }
            set {
                _groundTruth = value;
                _recolor();
            }
        }

        public string ActivityLabel
        {
            get { return This.ActivityLabel; }
            set
            {
                SetProperty(This.ActivityLabel, value, () => This.ActivityLabel = value);
                _recolor();
            }
        }

        private Color _borderColor;
        public Color BorderColor
        {
            get { return _borderColor; }
            set { SetProperty(ref _borderColor, value); }
        }

        public bool IsGlitch
        {
            get { return This.IsGlitch; }
            set
            {
                SetProperty(This.IsGlitch, value, () => This.IsGlitch = value);
            }
        }

        public List<Tuple<string, double>> ActivityProbability { get; internal set; }

        private void _recolor()
        {
            if(_groundTruth == "")
            {
                BorderColor = IgnorePrediction;
            }
            else
            {
                if (ActivityLabel == "") BorderColor = NoPrediction;
                else if (ActivityLabel == _groundTruth) BorderColor = CorrectPrediction;
                else BorderColor = WrongPrediction;
            }
        }
    }
}
