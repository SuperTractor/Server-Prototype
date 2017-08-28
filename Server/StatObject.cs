using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DatabaseUtility;
using GameUtility;

namespace Server
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

        // 总抢底次数
        public int totalBidTimes
        {
            get
            {
                return (int)Get("totalBidTimes");
            }
            set
            {
                Set("totalBidTimes", value);
            }
        }
        // 总炒底次数
        public int totalFryTimes
        {
            get
            {
                return (int)Get("totalFryTimes");
            }
            set
            {
                Set("totalFryTimes", value);
            }
        }
        // 总埋底分数
        public int totalBuryScores
        {
            get
            {
                return (int)Get("totalBuryScores");
            }
            set
            {
                Set("totalBuryScores", value);
            }
        }
        // 总埋底次数
        public int totalBuryTimes
        {
            get
            {
                return (int)Get("totalBuryTimes");
            }
            set
            {
                Set("totalBuryTimes", value);
            }
        }
        // 平均埋底分数
        public float aveBuryScores
        {
            get
            {
                return (float)Get("aveBuryScores");
            }
            set
            {
                Set("aveBuryScores", value);
            }
        }
        // 游戏总局数
        public int totalRoundTimes
        {
            get
            {
                return (int)Get("totalRoundTimes");
            }
            set
            {
                Set("totalRoundTimes", value);
            }
        }
        // 总单打次数
        public int totalSingleTimes
        {
            get
            {
                return (int)Get("totalSingleTimes");
            }
            set
            {
                Set("totalSingleTimes", value);
            }
        }
        // 总单打次数
        public int totalFindFriendTimes
        {
            get
            {
                return (int)Get("totalFindFriendTimes");
            }
            set
            {
                Set("totalFindFriendTimes", value);
            }
        }
        // 总做庄次数
        public int totalBankerTimes
        {
            get
            {
                return (int)Get("totalBankerTimes");
            }
            set
            {
                Set("totalBankerTimes", value);
            }
        }
        // 总抄底成功数
        public int totalBottomSuccessTimes
        {
            get
            {
                return (int)Get("totalBottomSuccessTimes");
            }
            set
            {
                Set("totalBottomSuccessTimes", value);
            }
        }
        // 总抄底分数
        public int totalBottomScores
        {
            get
            {
                return (int)Get("totalBottomScores");
            }
            set
            {
                Set("totalBottomScores", value);
            }
        }
        // 最高抄底分数
        public int highBottomScores
        {
            get
            {
                return (int)Get("highBottomScores");
            }
            set
            {
                Set("highBottomScores", value);
            }
        }
        // 总逃跑次数
        public int totalRunTimes
        {
            get
            {
                return (int)Get("totalRunTimes");
            }
            set
            {
                Set("totalRunTimes", value);
            }
        }

        // 总台上次数
        public int totalUpperTimes
        {
            get
            {
                return (int)Get("totalUpperTimes");
            }
            set
            {
                Set("totalUpperTimes", value);
            }
        }
        // 胜率；平均台上方次数
        public float upperRate
        {
            get
            {
                return (float)Get("upperRate");
            }
            set
            {
                Set("upperRate", value);
            }
        }
        // 积分；经验值
        public int grades
        {
            get
            {
                return (int)Get("grades");
            }
            set
            {
                Set("grades", value);
            }
        }
        // 等级
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

        public StatObject()
        {
            //variables.Add(new NamedVariable("score", "int"));
            variables.Add(new NamedVariable("highScore", "int"));
            variables.Add(new NamedVariable("totalScore", "int"));
            variables.Add(new NamedVariable("highLevel", "int"));
            variables.Add(new NamedVariable("totalLevel", "int"));

            variables.Add(new NamedVariable("totalBidTimes", "int"));
            variables.Add(new NamedVariable("totalFryTimes", "int"));
            variables.Add(new NamedVariable("totalBuryScores", "int"));
            variables.Add(new NamedVariable("totalRoundTimes", "int"));
            variables.Add(new NamedVariable("totalSingleTimes", "int"));
            variables.Add(new NamedVariable("totalFindFriendTimes", "int"));
            variables.Add(new NamedVariable("totalBankerTimes", "int"));
            variables.Add(new NamedVariable("totalBottomSuccessTimes", "int"));
            variables.Add(new NamedVariable("totalBottomScores", "int"));
            variables.Add(new NamedVariable("highBottomScores", "int"));
            variables.Add(new NamedVariable("totalRunTimes", "int"));
            variables.Add(new NamedVariable("totalBuryTimes", "int"));
            variables.Add(new NamedVariable("aveBuryScores", "float"));
            variables.Add(new NamedVariable("totalUpperTimes", "int"));
            variables.Add(new NamedVariable("upperRate", "float"));
            variables.Add(new NamedVariable("grades", "int"));
            variables.Add(new NamedVariable("level", "int"));


            highScore = 0;
            totalScore = 0;
            highLevel = 0;
            totalLevel = 0;

            totalBidTimes = 0;
            totalFryTimes = 0;
            totalBuryScores = 0;
            totalRoundTimes = 0;
            totalSingleTimes = 0;
            totalFindFriendTimes = 0;
            totalBankerTimes = 0;
            totalBottomSuccessTimes = 0;
            totalBottomScores = 0;
            highBottomScores = 0;
            totalRunTimes = 0;
            totalBuryTimes = 0;
            aveBuryScores = 0.0f;
            totalUpperTimes = 0;
            upperRate = 0.0f;
            grades = 0;
            level = 1;
        }

        public StatObject(DataObject dataObj)
        {
            //variables = dataObj.variables;
            variables = new List<NamedVariable>(dataObj.variables);

        }

        public void CopyTo(PlayerInfo info)
        {
            info.username = username;

            info.highScore = highScore;
            info.totalScore = totalScore;
            info.highLevel = highLevel;
            info.totalLevel = totalLevel;

            info.totalBidTimes = totalBidTimes;
            info.totalFryTimes = totalFryTimes;
            info.totalBuryScores = totalBuryScores;
            info.totalRoundTimes = totalRoundTimes;
            info.totalSingleTimes = totalSingleTimes;
            info.totalFindFriendTimes = totalFindFriendTimes;
            info.totalBankerTimes = totalBankerTimes;
            info.totalBottomSuccessTimes = totalBottomSuccessTimes;
            info.totalBottomScores = totalBottomScores;
            info.highBottomScores = highBottomScores;
            info.totalRunTimes = totalRunTimes;
            info.totalBuryTimes = totalBuryTimes;
            info.aveBuryScores = aveBuryScores;
            info.totalUpperTimes = totalUpperTimes;
            info.upperRate = upperRate;
            info.grades = grades;
            info.level = level;

        }

        public void CopyFrom(PlayerInfo info)
        {
            username = info.username;


            highScore = info.highScore;
            totalScore = info.totalScore;
            highLevel = info.highLevel;
            totalLevel = info.totalLevel;

            totalBidTimes = info.totalBidTimes;
            totalFryTimes = info.totalFryTimes;
            totalBuryScores = info.totalBuryScores;
            totalRoundTimes = info.totalRoundTimes;
            totalSingleTimes = info.totalSingleTimes;
            totalFindFriendTimes = info.totalFindFriendTimes;
            totalBankerTimes = info.totalBankerTimes;
            totalBottomSuccessTimes = info.totalBottomSuccessTimes;
            totalBottomScores = info.totalBottomScores;
            highBottomScores = info.highBottomScores;
            totalRunTimes = info.totalRunTimes;
            totalBuryTimes = info.totalBuryTimes;
            aveBuryScores = info.aveBuryScores;
            totalUpperTimes = info.totalUpperTimes;
            upperRate = info.upperRate;
            grades = info.grades;
            level = info.level;

        }
    }
}
