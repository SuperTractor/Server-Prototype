using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    /// <summary>
    /// 为了避免表单一直放在内存，所有操作前，都应该重新加载表单，完成操作后，保存表单
    /// 
    /// </summary>
    public class DBManager
    {
        // 存放要用到的表
        List<Table> m_tables; 

        // 初始化函数
        public DBManager()
        {
            // 新建表单
            m_tables = new List<Table>();
            // 用户基本信息表单
            m_tables.Add(new Table("user"));
            // 用户游戏统计数据表单
            m_tables.Add(new Table("stat"));
        }

        //// 清理函数
        //~DBManager()
        //{
        //    for(int i = 0; i < m_tables.Count; i++)
        //    {
        //        m_tables[i].Dispose();
        //    }
        //}

        // 向指定表单插入一条记录
        // OBSOLETE
        public bool Insert(string tableName,object data)
        {
            //if (itemNames.Count != itemValues.Count)
            //{
            //    // 名称和数据项个数不匹配
            //    return false;
            //}
            int idx = m_tables.FindIndex(table => table.name == tableName);
            if (idx < 0)
            {
                // 找不到指定表单
                return false;
            }

            List<string> itemNames = new List<string>();
            List<object> itemValues = new List<object>();

            if (tableName == "stat")
            {
            }
            else if(tableName == "user")
            {

            }
            else
            {
                // 出错了
                return false;
            }

            // 向表单插入数据
            m_tables[idx].Insert(itemNames, itemValues);

            return true;
        }

        public bool Insert(string tableName,DataObject dataObj)
        {
            int idx = m_tables.FindIndex(table => table.name == tableName);
            if (idx < 0)
            {
                // 找不到指定表单
                return false;
            }

            m_tables[idx].Insert(dataObj);

            return true;
        }

        // 更新现有数据，如果没有，则自动创建
        // OBSOLETE
        public bool Update(string tableName, object data)
        {
            return true;
        }

        public bool Update(string tableName,DataObject dataObj)
        {
            int idx = m_tables.FindIndex(table => table.name == tableName);
            if (idx < 0)
            {
                // 找不到指定表单
                return false;
            }

            m_tables[idx].Update(dataObj);

            return true;
        }

        // 检查指定用户是否存在于指定表单
        public bool IsExist(string tableName,string userName)
        {
            int idx = m_tables.FindIndex(table => table.name == tableName);
            if (idx < 0)
            {
                // 找不到指定表单
                return false;
            }
            // 调用 Table 方法
            return m_tables[idx].IsExist(userName);
        }

        //// 在指定表单查找并返回指定用户的数据（数据要用对应的类封装好）
        //public object Find(string tableName,string userName)
        //{
        //    int idx = m_tables.FindIndex(table => table.name == tableName);
        //    if (idx < 0)
        //    {
        //        // 找不到指定表单
        //        return null;
        //    }

        //    List<string> names = new List<string>();
        //    List<object> values = new List<object>();
        //    // 调用 Table 方法，获取记录
        //    bool isFound = m_tables[idx].Find(userName, names, values);

        //    if (!isFound)
        //    {
        //        // 找不到指定用户
        //        return null;
        //    }

        //    if (tableName == "stat")
        //    {
        //        StatObject statObject = new StatObject();
        //        for(int i = 0; i < names.Count; i++)
        //        {
        //            switch (names[i])
        //            {
        //                case "username":
        //                    statObject.username = (string)values[i];
        //                    break;
        //                case "lastUpdatedTime":
        //                    statObject.lastUpdatedTime = (DateTime)values[i];
        //                    break;
        //                case "score":
        //                    statObject.score = (int)values[i];
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        return statObject;
        //    }
        //    else if (tableName == "user")
        //    {
        //        UserObject statObject = new UserObject();
        //        for (int i = 0; i < names.Count; i++)
        //        {
        //            switch (names[i])
        //            {
        //                case "username":
        //                    statObject.username = (string)values[i];
        //                    break;
        //                case "lastUpdatedTime":
        //                    statObject.lastUpdatedTime = (DateTime)values[i];
        //                    break;
        //                case "password":
        //                    statObject.password = (string)values[i];
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        return statObject;
        //    }
        //    // 出错了
        //    return null;
        //}

        public DataObject Find(string tableName,string userName)
        {
            int idx = m_tables.FindIndex(table => table.name == tableName);
            if (idx < 0)
            {
                // 找不到指定表单
                return null;
            }
            // 调用 Table 方法，获取记录
            return m_tables[idx].Find(userName);
        }

    }
}
