using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class RecentDatasets
    {
        public string Name { get; set; }
        public string Path { get; set; }

        public RecentDatasets(string Name, string Path)
        {
            this.Name = Name;
            this.Path = Path;
        }

        public RecentDatasets()
        {
            this.Name = "";
            this.Path = "";
        }
    }
}
