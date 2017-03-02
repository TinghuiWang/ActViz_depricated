using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace ActViz.Models
{
    public class Sensor
    {
        private const string nameKey = "name";
        private const string typeKey = "type";
        private const string locXKey = "locX";
        private const string locYKey = "locY";
        private const string sizeXKey = "sizeX";
        private const string sizeYKey = "sizeY";
        private const string descriptionKey = "description";
        private string _name;
        private string _type;
        private double _locX;
        private double _locY;
        private double _sizeX;
        private double _sizeY;
        private string _description;

        public double fontSize { get; set; }

        public Sensor(string name = "M001", string type = "Motion",
            double locX = 0.0, double locY = 0.0, double sizeX = 0.0, double sizeY = 0.0,
            string description = "")
        {
            this._name = name;
            this._type = type;
            this._locX = locX;
            this._locY = locY;
            this._sizeX = sizeX;
            this._sizeY = sizeY;
            this._description = description;
        }

        public Sensor(JsonObject jsonObject)
        {
            _name = jsonObject.GetNamedString(nameKey);
            _type = jsonObject.GetNamedString(typeKey);
            _locX = jsonObject.GetNamedNumber(locXKey);
            _locY = jsonObject.GetNamedNumber(locYKey);
            _sizeX = jsonObject.GetNamedNumber(sizeXKey);
            _sizeY = jsonObject.GetNamedNumber(sizeYKey);
            if (jsonObject.Keys.Contains(descriptionKey))
            {
                _description = jsonObject.GetNamedString(descriptionKey);
            }
            else
            {
                _description = "";
            }
        }

        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public string type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
            }
        }

        public double locX
        {
            get
            {
                return _locX;
            }
            set
            {
                _locX = value;
            }
        }

        public double locY
        {
            get
            {
                return _locY;
            }
            set
            {
                _locY = value;
            }
        }

        public double sizeX
        {
            get
            {
                return _sizeX;
            }
            set
            {
                _sizeX = value;
            }
        }

        public double sizeY
        {
            get
            {
                return _sizeY;
            }
            set
            {
                _sizeY = value;
            }
        }

        public string description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }

        public JsonObject ToJsonObject()
        {
            JsonObject sensorObject = new JsonObject();
            sensorObject.SetNamedValue(nameKey, JsonValue.CreateStringValue(_name));
            sensorObject.SetNamedValue(typeKey, JsonValue.CreateStringValue(_type));
            sensorObject.SetNamedValue(locXKey, JsonValue.CreateNumberValue(_locX));
            sensorObject.SetNamedValue(locYKey, JsonValue.CreateNumberValue(_locY));
            sensorObject.SetNamedValue(sizeXKey, JsonValue.CreateNumberValue(_sizeX));
            sensorObject.SetNamedValue(sizeYKey, JsonValue.CreateNumberValue(_sizeY));
            sensorObject.SetNamedValue(descriptionKey, JsonValue.CreateStringValue(_description));
            return sensorObject;
        }

        public static int CompareByName(Sensor sensor1, Sensor sensor2)
        {
            return string.Compare(sensor1.name, sensor2.name);
        }

    }
}
