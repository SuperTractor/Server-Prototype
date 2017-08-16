using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase
{
    /// <summary>
    /// 对应 stat 表单一个记录
    /// </summary>
    public class StatObject : DataObject
    {
        public int score
        {
            get
            {
                return (int)Get("score");
            }
            set
            {
                Set("score", value);
            }
        }

        public StatObject()
        {
            variables.Add(new NamedVariable("score", "int"));
        }

        public StatObject(DataObject dataObj)
        {
            variables = dataObj.variables;
        }
    }
}
