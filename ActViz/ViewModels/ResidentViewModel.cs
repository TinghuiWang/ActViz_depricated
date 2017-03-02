using ActViz.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ActViz.ViewModels
{
    public class ResidentViewModel : NotificationBase
    {
        private Resident _resident;
        public Resident resident { get { return _resident; } }

        public string name { get { return _resident.name; } set { SetProperty(ref _resident.name, value); } }
        public int id { get { return _resident.id; } set { SetProperty(ref _resident.id, value); } }
        public Color color { get { return _resident.color; } set { SetProperty(ref _resident.color, value); } }
        public bool isNoise
        {
            get { return _resident.isNoise; }
            set
            {
                SetProperty(ref _resident.isNoise, value);
                if (_resident.isNoise)
                {
                    this.isIgnored = false;
                    this.color = Colors.DarkGoldenrod;
                }
            }
        }

        public bool isIgnored
        {
            get { return _resident.isIgnored; }
            set
            {
                SetProperty(ref _resident.isIgnored, value);
                if (_resident.isIgnored)
                {
                    this.isNoise = false;
                    this.color = Colors.DimGray;
                }
            }
        }

        public ResidentViewModel(int id)
        {
            this._resident = new Resident(id);
        }

        public ResidentViewModel(Resident resident)
        {
            this._resident = resident;
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
