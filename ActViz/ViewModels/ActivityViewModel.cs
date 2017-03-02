using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ActViz.ViewModels
{
    public class ActivityViewModel : NotificationBase
    {
        private Activity _activity;
        public Activity activity { get { return _activity; } }

        public string name { get { return _activity.name; } set { SetProperty(ref _activity.name, value); } }
        public Color color { get { return _activity.color; } set { SetProperty(ref _activity.color, value); } }
        public bool isNoise
        {
            get { return _activity.isNoise; }
            set
            {
                SetProperty(ref _activity.isNoise, value);
                if (_activity.isNoise)
                {
                    this.isIgnored = false;
                    this.color = Colors.DarkGoldenrod;
                }
            }
        }

        public bool isIgnored
        {
            get { return _activity.isIgnored; }
            set
            {
                SetProperty(ref _activity.isIgnored, value);
                if (_activity.isIgnored)
                {
                    this.isNoise = false;
                    this.color = Colors.DimGray;
                }
            }
        }

        private bool _isVisible = true;
        public bool isVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        public ActivityViewModel()
        {
            this._activity = new Activity();
        }

        public ActivityViewModel(Activity activity)
        {
            this._activity = activity;
        }

        public void SetColor(string colorHex)
        {
            if (colorHex.Length == 9)
            {
                byte r = byte.Parse(colorHex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                byte g = byte.Parse(colorHex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                byte b = byte.Parse(colorHex.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
                this.color = Color.FromArgb(255, r, g, b);
            }
        }
    }
}
