using ActViz.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz
{
    /// <summary>
    /// Making Logging as a singleton class 
    /// </summary>
    public class Logging : INotifyPropertyChanged
    {
        static readonly Logging _instance = new Logging();
        public static Logging Instance
        {
            get { return _instance; }
        }

        private string _log;
        public string Log { get { return _log; } }

        private Logging()
        {
            _log = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public void Warn(string sender, string message)
        {
            if (_log == null) _log = "";
            _log += string.Format("[{0}] Warn: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
            RaisePropertyChanged("Log");
        }

        public void Info(string sender, string message)
        {
            if (_log == null) _log = "";
            _log += string.Format("[{0}] Info: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
            RaisePropertyChanged("Log");
        }

        public void Error(string sender, string message)
        {
            if (_log == null) _log = "";
            _log += string.Format("[{0}] Error: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
            RaisePropertyChanged("Log");
        }

        public void Debug(string sender, string message)
        {
            if (_log == null) _log = "";
            _log += string.Format("[{0}] Debug: {1} : {2}\n", DateTime.Now.ToString(), sender, message);
            RaisePropertyChanged("Log");
        }
    }
}
