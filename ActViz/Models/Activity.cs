using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI;

namespace ActViz.Models
{
    public class Activity
    {
        private const string nameKey = "name";
        private const string colorKey = "color";
        private const string noiseKey = "is_noise";
        private const string ignoredKey = "is_ignored";
        public string name ;
        public Color color;
        public bool isNoise;
        public bool isIgnored;

        #region Constructor
        public Activity(JsonObject jsonObject)
        {
            this.name = jsonObject.GetNamedString(nameKey);
            this.SetColor(jsonObject.GetNamedString(colorKey));
            this.isNoise = jsonObject.GetNamedBoolean(noiseKey);
            this.isIgnored = jsonObject.GetNamedBoolean(ignoredKey);
        }

        public Activity()
        {
            this.name = "";
            this.color = Colors.DimGray;
            this.isNoise = false;
            this.isIgnored = false;
        }

        public Activity(string name, Color color, bool isNoise, bool isIgnored)
        {
            this.name = name;
            this.color = color;
            this.isNoise = isNoise;
            this.isIgnored = isIgnored;
        }

        public Activity(string name, string colorHex, bool isNoise, bool isIgnored)
        {
            this.name = name;
            this.SetColor(colorHex);
            this.isNoise = isNoise;
            this.isIgnored = isIgnored;
        }
        #endregion

        public JsonObject ToJsonObject()
        {
            JsonObject jsonObject = new JsonObject();
            jsonObject.SetNamedValue(nameKey, JsonValue.CreateStringValue(this.name));
            jsonObject.SetNamedValue(colorKey, JsonValue.CreateStringValue(this.color.ToString()));
            jsonObject.SetNamedValue(noiseKey, JsonValue.CreateBooleanValue(this.isIgnored));
            jsonObject.SetNamedValue(ignoredKey, JsonValue.CreateBooleanValue(this.isNoise));
            return jsonObject;
        }

        public Color ColorForIgnored
        {
            get { return Colors.DimGray; }
        }

        public Color ColorForNoise
        {
            get { return Colors.LightGoldenrodYellow; }
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
