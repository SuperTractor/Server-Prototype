using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;


namespace DataBase
{
    /// <summary>
    /// 管理一个表单的类
    /// </summary>
    public class Table/*:IDisposable*/
    {
        // 表名
        string m_name;
        public string name
        {
            get { return m_name; }
        }

        // 表的位置
        string m_path;

        // 表单
        XDocument m_doc;

        // 存储数据项名和类型
        List<NamedVariable> m_variables;

        // 数据库初始化
        public Table(string tableName)
        {
            m_name = tableName;

            m_variables = new List<NamedVariable>();

            // 在根目录下找表文件
            m_path = string.Format(".\\{0}.xml", tableName);
            // 如果找不到表文件
            if (!File.Exists(m_path))
            {
                // 创建表文件
                //using (XmlWriter xw = XmlWriter.Create(m_path))
                //{
                //    xw.WriteStartDocument();
                //    xw.WriteEndDocument();
                //}
                m_doc = new XDocument(
                    new XDeclaration("1.0", "utf-8", "yes"),
                    new XElement("root", new XElement("colomns"), new XElement("entries"))
                    );
                m_doc.Save(m_path);
            }
            //else
            //{
            //    m_doc = XDocument.Load(m_path);
            //}
            // 此时应该有表文件了
            // 重新加载表单
            m_doc = XDocument.Load(m_path);
            // 读取表单的数据项名
            List<XElement> colomns = new List<XElement>(m_doc.Root.Element("colomns").Elements());
            for (int i = 0; i < colomns.Count; i++)
            {
                m_variables.Add(new NamedVariable(colomns[i].Element("name").Value, colomns[i].Element("type").Value));
            }
            // 保存表单
            m_doc.Save(m_path);
        }

        public string GetType(string name)
        {
            return m_variables.Find(var => var.name == name).type;
        }

        //public void Dispose()
        //{
        //    m_doc.Save(m_path);
        //}
        // obsolete
        public void Insert(List<string> names, List<object> values)
        {
            //if (names.Count != values.Count)
            //{
            //    throw new Exception("插入失败，列名和数据项个数不一致");
            //}

            // 重新加载表单
            m_doc = XDocument.Load(m_path);

            // 创建数据项
            XElement entry = new XElement("entry");

            for (int i = 0; i < names.Count; i++)
            {
                // 向数据项添加元素
                XElement element = new XElement(names[i], values[i]);
                entry.Add(element);
            }
            // 将数据项添加到根目录
            m_doc.Root.Element("entries").Add(entry);
            // 保存表单
            m_doc.Save(m_path);
        }

        public void Insert(DataObject dataObj)
        {
            // 重新加载表单
            m_doc = XDocument.Load(m_path);

            // 创建数据项
            XElement entry = new XElement("entry");

            for (int i = 0; i < dataObj.variables.Count; i++)
            {
                // 向数据项添加元素
                // 如果这项数据是最后更新时间
                if (dataObj.variables[i].name == "lastUpdatedTime")
                {
                    // 盖上时间戳
                    dataObj.variables[i].data = DateTime.Now;
                }

                XElement element = new XElement(dataObj.variables[i].name, dataObj.variables[i].data);
                entry.Add(element);
            }
            // 将数据项添加到根目录
            m_doc.Root.Element("entries").Add(entry);

            // 保存表单
            m_doc.Save(m_path);
        }

        // 从该表单中，检查是否存在指定用户
        public bool IsExist(string username)
        {
            // 重新加载表单
            m_doc = XDocument.Load(m_path);
            // 获取所有 entry
            List<XElement> entries = new List<XElement>(m_doc.Root.Element("entries").Elements());
            // 找同名的记录
            int idx = entries.FindIndex(entry => entry.Element("username").Value == username);

            // 保存表单
            m_doc.Save(m_path);
            // 返回查找结果
            return idx >= 0;
        }

        // obsolete
        public bool Find(string username, List<string> names, List<object> values)
        {
            // 重新加载表单
            m_doc = XDocument.Load(m_path);
            // 获取所有 entry
            List<XElement> entries = new List<XElement>(m_doc.Root.Element("entries").Elements());
            // 找同名的记录
            int idx = entries.FindIndex(entry => entry.Element("username").Value == username);

            if (idx < 0)
            {
                // 用户名不存在
                return false;
            }

            // 获取记录的所有数据项
            List<XElement> elements = new List<XElement>(entries[idx].Elements());
            //names = new List<string>();
            //values = new List<object>();
            for (int i = 0; i < elements.Count; i++)
            {
                names.Add(elements[i].Name.LocalName);
                values.Add(elements[i].Value);
            }

            // 保存表单
            m_doc.Save(m_path);
            return true;
        }

        public DataObject Find(string username)
        {
            // 重新加载表单
            m_doc = XDocument.Load(m_path);
            // 获取所有 entry
            List<XElement> entries = new List<XElement>(m_doc.Root.Element("entries").Elements());
            // 找同名的记录
            int idx = entries.FindIndex(entry => entry.Element("username").Value == username);

            if (idx < 0)
            {
                // 用户名不存在
                return null;
            }

            // 获取记录的所有数据项
            List<XElement> elements = new List<XElement>(entries[idx].Elements());
            // 复制数据项模板
            NamedVariable[] temp = new NamedVariable[m_variables.Count];
            m_variables.CopyTo(temp);

            DataObject dataObj = new DataObject();
            dataObj.variables = temp.ToList();

            //names = new List<string>();
            //values = new List<object>();
            for (int i = 0; i < elements.Count; i++)
            {
                string type = GetType(elements[i].Name.LocalName);
                if (type == "int")
                {
                    dataObj.Set(elements[i].Name.LocalName, int.Parse(elements[i].Value));
                }
                else if (type == "bool")
                {
                    dataObj.Set(elements[i].Name.LocalName, bool.Parse(elements[i].Value));
                }
                else if (type == "double")
                {
                    dataObj.Set(elements[i].Name.LocalName, bool.Parse(elements[i].Value));
                }
                else if (type == "float")
                {
                    dataObj.Set(elements[i].Name.LocalName, float.Parse(elements[i].Value));
                }
                else if (type == "double")
                {
                    dataObj.Set(elements[i].Name.LocalName, double.Parse(elements[i].Value));
                }
                else if (type == "DateTime")
                {
                    dataObj.Set(elements[i].Name.LocalName, DateTime.Parse(elements[i].Value));
                }
                else if (type == "string")
                {
                    dataObj.Set(elements[i].Name.LocalName, elements[i].Value);
                }
            }


            // 保存表单
            m_doc.Save(m_path);
            return dataObj;
        }

        // 更新数据项
        public void Update(DataObject dataObj)
        {
            // 如果不存在
            if (!IsExist(dataObj.username))
            {
                // 增加新记录
                Insert(dataObj);
            }
            // 如果已经存在该用户的记录
            else
            {
                // 重新加载表单
                m_doc = XDocument.Load(m_path);
                // 获取所有 entry
                List<XElement> entries = new List<XElement>(m_doc.Root.Element("entries").Elements());
                // 找同名的记录
                int idx = entries.FindIndex(entry => entry.Element("username").Value == dataObj.username);

                // 改数据
                for (int i = 0; i < dataObj.variables.Count; i++)
                {
                    // 如果这项数据是最后更新时间
                    if (dataObj.variables[i].name == "lastUpdatedTime")
                    {
                        // 盖上时间戳
                        dataObj.variables[i].data = DateTime.Now;
                    }
                    XElement element = entries[idx].Element(dataObj.variables[i].name);
                    // 如果有这个数据项
                    if (element != null)
                    {
                        element.SetValue(dataObj.variables[i].data);
                    }
                    // 如果没有
                    else
                    {
                        entries[idx].Add(new XElement(dataObj.variables[i].name, dataObj.variables[i].data));
                    }
                }

                // 保存表单
                m_doc.Save(m_path);
            }
        }
    }
}
