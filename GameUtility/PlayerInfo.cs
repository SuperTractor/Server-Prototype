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
        // 玩家的等级
        public int level { get; set; }
        // 玩家的分数
        public int score { get; set; }
        // 手牌
        // 手牌空的位置用 null 表示
        public List<Card> cardInHand { get; set; }
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


        public PlayerInfo(string name)
        {
            this.name = name;
            cardInHand = new List<Card>();
        }
        public PlayerInfo(string name,int id)
        {
            this.name = name;
            this.id = id;
            cardInHand = new List<Card>();
        }



    }
}
