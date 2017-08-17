using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseUtility;
using GameUtility;

namespace server_0._0._1
{
    /// <summary>
    /// 对应 stat 表单一个记录
    /// </summary>
    /// 
    //[Serializable]
    public class StatObject : DataObject
    {
        //public int score
        //{
        //    get
        //    {
        //        return (int)Get("score");
        //    }
        //    set
        //    {
        //        Set("score", value);
        //    }
        //}


        // 统计数据部分
        // 最高得分
        public int highScore
        {
            get
            {
                return (int)Get("highScore");
            }
            set
            {
                Set("highScore", value);
            }
        }
        // 累计得分
        public int totalScore
        {
            get
            {
                return (int)Get("totalScore");
            }
            set
            {
                Set("totalScore", value);
            }
        }
        // 最高级数
        public int highLevel
        {
            get
            {
                return (int)Get("highLevel");
            }
            set
            {
                Set("highLevel", value);
            }
        }
        // 累计级数
        public int totalLevel
        {
            get
            {
                return (int)Get("totalLevel");
            }
            set
            {
                Set("totalLevel", value);
            }
        }


        public StatObject()
        {
            //variables.Add(new NamedVariable("score", "int"));
            variables.Add(new NamedVariable("highScore", "int"));
            variables.Add(new NamedVariable("totalScore", "int"));
            variables.Add(new NamedVariable("highLevel", "int"));
            variables.Add(new NamedVariable("totalLevel", "int"));

            highScore = 0;
            totalScore = 0;
            highLevel = 0;
            totalLevel = 0;
        }

        public StatObject(DataObject dataObj)
        {
            //variables = dataObj.variables;
            variables = new List<NamedVariable>(dataObj.variables);

        }

        public void CopyTo(PlayerInfo info)
        {
            info.highScore = highScore;
            info.totalScore = totalScore;
            info.highLevel = highLevel;
            info.totalLevel = totalLevel;
        }

        public void CopyFrom(PlayerInfo info)
        {
            highScore = info.highScore;
            totalScore = info.totalScore;
            highLevel = info.highLevel;
            totalLevel = info.totalLevel;
        }
    }
}
