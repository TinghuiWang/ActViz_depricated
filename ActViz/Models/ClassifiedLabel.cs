using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActViz.Models
{
    public class ClassifiedLabel
    {
        public string ActivityLabel { get; set; }
        public bool IsError { get; set; }
        public bool IsGlitch { get; set; }
    }
}
