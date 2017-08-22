using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtility
{
    [Serializable]
    public class PlayerInfo
    {
        public int id { get; set; }
        // 玩家的名字
        public string name { get; set; }
        //// 玩家的等级
        //public int level { get; set; }
        //// 玩家的分数
        //public int score { get; set; }
        //// 手牌
        //// 手牌空的位置用 null 表示
        //public List<Card> cardInHand /*{ get; set; }*/;
        //public Card[] cardInHand { get; set; }

        // 统计数据部分
        // 最高得分
        public int highScore { get; set; }
        // 累计得分
        public int totalScore { get; set; }
        // 最高级数
        public int highLevel { get; set; }
        // 累计级数
        public int totalLevel { get; set; }

        // 总抢底次数
        public int totalBidTimes { get; set; }
        // 总炒底次数
        public int totalFryTimes { get; set; }
        // 总埋底分数
        public int totalBuryScores { get; set; }
        // 游戏总局数
        public int totalRoundTimes { get; set; }
        // 总单打次数
        public int totalSingleTimes { get; set; }
        // 总做庄次数
        public int totalBankerTimes { get; set; }
        // 总抄底成功数
        public int totalBottomSuccessTimes { get; set; }
        // 总抄底分数
        public int totalBottomScores { get; set; }
        // 最高抄底分数
        public int highBottomScores { get; set; }
        // 总逃跑次数
        public int totalRunTimes { get; set; }




        public PlayerInfo(string name)
        {
            this.name = name;
            //cardInHand = new List<Card>();
        }
        public PlayerInfo(string name, int id)
        {
            this.name = name;
            this.id = id;
            //cardInHand = new List<Card>();
        }

        // 更新玩家统计信息
        public void UpdateStat()
        {
            //totalLevel += level;
            //totalScore += score;
            //highLevel = Math.Max(highLevel, level);
            //highScore = Math.Max(highScore, score);
        }


    }
}
