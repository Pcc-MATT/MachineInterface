using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineInterface
{
    public class Part
    {
        public string uuid { get; set; }
        public string path { get; set; }
        public List<attributes> Attributes { get; set; }
        public class attributes
        {
            public int key { get; set; }
            public string value { get; set; }
        }
    }
}
