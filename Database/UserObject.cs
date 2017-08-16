using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    /// <summary>
    /// 对应 user 表单一个记录
    /// </summary>
    /// 
    [Serializable]
    public class UserObject : DataObject
    {
        public string password
        {
            get
            {
                return (string)Get("password");
            }
            set
            {
                Set("password", value);
            }
        }

        public UserObject()
        {
            variables.Add(new NamedVariable("password", "string"));
        }

        public UserObject(DataObject dataObj)
        {
            variables = dataObj.variables;
        }
    }
}
