using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    public class NamedVariable
    {
        public string name { get; set; }
        public string type { get; set; }
        public object data { get; set; }

        public NamedVariable(string name,string type,object data = null)
        {
            this.name = name;
            this.type = type;
            this.data = data;
        }
    }
}
