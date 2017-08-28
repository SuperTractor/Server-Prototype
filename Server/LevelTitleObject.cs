using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseUtility;

namespace Server
{
    public class LevelTitleObject : DataObject
    {
        public int level
        {
            get
            {
                return (int)Get("level");
            }
            set
            {
                Set("level", value);
            }
        }
        public string title
        {
            get
            {
                return (string)Get("title");
            }
            set
            {
                Set("title", value);
            }
        }

        public LevelTitleObject()
        {
            variables.Add(new NamedVariable("level", "int"));
            variables.Add(new NamedVariable("title", "string"));

            level = 1;
            title = "";
        }

        public LevelTitleObject(DataObject dataObj)
        {
            //variables = dataObj.variables;
            variables = new List<NamedVariable>(dataObj.variables);

        }
    }
}
