using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace ActViz.Models
{
    public class SensorType
    {
        private string _type;
        private string _strColor;
        private Color _color;
        private static ObservableCollection<SensorType> _allSensorTypes = null;

        public string type { get { return _type; } set { _type = value; } }
        public string strColor { get { return _strColor; } set { _strColor = value; } }
        public Color color { get { return _color; } set { _color = value; } }

        public SensorType(string type, Color color)
        {
            _type = type;
            _color = color;
            _strColor = color.ToString();
        }

        private static void InitAllSensorTypes()
        {
            if (_allSensorTypes == null)
            {
                _allSensorTypes = new ObservableCollection<SensorType>();
                _allSensorTypes.Add(new SensorType("Motion", Colors.Red));
                _allSensorTypes.Add(new SensorType("Door", Colors.Green));
                _allSensorTypes.Add(new SensorType("Temperature", Colors.DarkGoldenrod));
                _allSensorTypes.Add(new SensorType("Item", Colors.BlueViolet));
                _allSensorTypes.Add(new SensorType("Light", Colors.DarkOrange));
            }
        }

        public static string GuessSensorTypeFromName(string sensorName)
        {
            switch(sensorName[0])
            {
                case 'M':
                    return "Motion";
                case 'D':
                    return "Door";
                case 'T':
                    return "Temperature";
                case 'I':
                    return "Item";
                case 'L':
                    return "Light";
                case 'P':
                    return "Power";
                default:
                    return "Motion";
            }
        }

        public static Color GetColorFromSensorType(string type)
        {
            if (_allSensorTypes == null)
            {
                InitAllSensorTypes();
            }

            for (int i = 0; i < _allSensorTypes.Count; i++)
            {
                if (type == _allSensorTypes[i].type)
                {
                    return _allSensorTypes[i].color;
                }
            }
            return Colors.White;
        }

        public static ObservableCollection<SensorType> GetSensorTypes()
        {
            InitAllSensorTypes();
            return _allSensorTypes;
        }

        public static int GetSensorTypeIndex(string type)
        {
            int i;
            InitAllSensorTypes();
            for (i = 0; i < _allSensorTypes.Count; i++)
            {
                if (_allSensorTypes[i].type == type)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
