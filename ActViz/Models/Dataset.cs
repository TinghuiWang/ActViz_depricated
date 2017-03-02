using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using ActViz.Models;

namespace ActViz.Models
{
    public class Dataset
    {
        private const string nameKey = "name";
        private const string floorPlanKey = "floorplan";
        private const string sensorKey = "sensors";
        private const string residentKey = "residents";
        private const string activityKey = "activities";
        private const string jsonFileName = "dataset.json";
        private string _pathToFloorPlan;
        private List<Sensor> _sensorList;
        private List<Resident> _residentList;
        private List<Activity> _activityList;

        public string Name { get; set; }
        public string Path { get; set; }
        public StorageFolder Folder { get; set; }
        public List<Sensor> SensorList { get { return _sensorList; } }
        public List<Resident> ResidentList { get { return _residentList; } }
        public List<Activity> ActivityList { get { return _activityList; } }

        public Dataset()
        {
            this._sensorList = new List<Sensor>();
            _residentList = new List<Resident>();
            _activityList = new List<Activity>();
        }

        public async Task SaveToFolder()
        {
            StorageFile datasetJson = await this.Folder.CreateFileAsync("dataset.json", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(datasetJson, this.Stringify());
        }

        public async Task FromStorageFolder(StorageFolder folder)
        {
            this.Folder = folder;
            this.Path = folder.Path;
            StorageFile datasetJson = await folder.GetFileAsync("dataset.json");
            string strDatasetJson = await FileIO.ReadTextAsync(datasetJson);
            FromJsonObject(JsonObject.Parse(strDatasetJson));
        }

        public void FromJsonObject(JsonObject homeObject)
        {
            Name = homeObject.GetNamedString(nameKey);
            _pathToFloorPlan = homeObject.GetNamedString(floorPlanKey);
            _sensorList = new List<Sensor>();
            _residentList = new List<Resident>();
            _activityList = new List<Activity>();
            try
            {
                JsonArray jsonSensorArray = homeObject.GetNamedArray(sensorKey);
                foreach (IJsonValue sensorValue in jsonSensorArray)
                {
                    Sensor sensor = new Sensor(sensorValue.GetObject());
                    _sensorList.Add(sensor);
                }
            }
            catch (Exception) { }
            try
            {
                JsonArray jsonResidentArray = homeObject.GetNamedArray(residentKey);
                foreach (IJsonValue residentValue in jsonResidentArray)
                {
                    Resident resident = new Resident(residentValue.GetObject());
                    _residentList.Add(resident);
                }
            }
            catch (Exception) { }
            try
            {
                JsonArray jsonActivityArray = homeObject.GetNamedArray(activityKey);
                foreach (IJsonValue activityValue in jsonActivityArray)
                {
                    Activity activity = new Activity(activityValue.GetObject());
                    _activityList.Add(activity);
                }
            }
            catch (Exception) { }
        }

        public JsonObject ToJsonObject()
        {
            JsonObject homeObject = new JsonObject();
            homeObject.SetNamedValue(nameKey, JsonValue.CreateStringValue(Name));
            homeObject.SetNamedValue(floorPlanKey, JsonValue.CreateStringValue(_pathToFloorPlan));
            // Sensor Array
            JsonArray jsonSensorArray = new JsonArray();
            foreach (Sensor sensor in _sensorList)
            {
                jsonSensorArray.Add(sensor.ToJsonObject());
            }
            homeObject.SetNamedValue(sensorKey, jsonSensorArray);
            // Activity Array
            JsonArray jsonActivityArray = new JsonArray();
            foreach (Activity activity in _activityList)
            {
                jsonActivityArray.Add(activity.ToJsonObject());
            }
            homeObject.SetNamedValue(activityKey, jsonActivityArray);
            // Resident Array
            JsonArray jsonResidentArray = new JsonArray();
            foreach(Resident resident in _residentList)
            {
                jsonResidentArray.Add(resident.ToJsonObject());
            }
            homeObject.SetNamedValue(residentKey, jsonResidentArray);
            return homeObject;
        }

        public string Stringify()
        {
            return ToJsonObject().Stringify();
        }

        public string pathToFloorPlan
        {
            get
            {
                return _pathToFloorPlan;
            }
            set
            {
                _pathToFloorPlan = value;
            }
        }

        public Sensor AddSensor(string sensorName)
        {
            string sensorType = SensorType.GuessSensorTypeFromName(sensorName);
            Sensor newSensor = new Sensor(sensorName, sensorType, 0, 0, 0.01, 0.01);
            _sensorList.Add(newSensor);
            return newSensor;
        }

        public Sensor AddSensor(string sensorName, string sensorType,
            double locX = 0.0, double locY = 0.0, double sizeX = 0.0, double sizeY = 0.0)
        {
            Sensor newSensor = new Sensor(sensorName, sensorType, locX, locY, sizeX, sizeY);
            _sensorList.Add(newSensor);
            return newSensor;
        }

        #region Activity
        public void AddActivity(Activity activity)
        {
            _activityList.Add(activity);
        }

        public void RemoveActivity(Activity activity)
        {
            _activityList.Remove(activity);
        }
        #endregion

        #region Resident
        public void AddResident(Resident resident)
        {
            _residentList.Add(resident);
        }

        public void RemoveResident(Resident resident)
        {
            _residentList.Remove(resident);
        }
        #endregion

        public List<Sensor> GetSensors()
        {
            return _sensorList;
        }

        public void RemoveSensor(Sensor sensor)
        {
            _sensorList.Remove(sensor);
        }
    }
}
