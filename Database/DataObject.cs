using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    // 表单记录的基类
    [Serializable]
    public class DataObject
    {
        // 一系列命名变量
        public List<NamedVariable> variables;
        //public List<NamedVariable> variables
        //{
        //    get { return m_variables; }
        //}
        // 存取接口
        public object Get(string name)
        {
            return variables.Find(namedVar => namedVar.name == name).data;
        }

        public void Set(string name, object value)
        {
            variables.Find(namedVar => namedVar.name == name).data = value;
        }

        public string username
        {
            get
            {
                return (string)Get("username");
            }
            set
            {
                Set("username", value);
            }
        }

        //public int id { get; set; }
        public DateTime lastUpdatedTime
        {
            get
            {
                return (DateTime)Get("lastUpdatedTime");
            }
            set
            {
                Set("lastUpdatedTime", value);
            }
        }

        // 构造函数
        public DataObject()
        {
            // 创建数据模板
            variables = new List<NamedVariable>();
            variables.Add(new NamedVariable("username", "string"));
            variables.Add(new NamedVariable("lastUpdatedTime", "DateTime"));
        }




    }
}
