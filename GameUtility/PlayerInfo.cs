using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtility
{
    [Serializable]
    public class PlayerInfo
    {
        // 用户信息部分
        public int id { get; set; }
        // 用户名
        public string username { get; set; }
        // 昵称
        public string nickname { get; set; }
        // 头像名称
        public string headImageName { get; set; }
        // 积分；经验值；从stat.xml中的grades同步过来
        public int experience { get; set; }
        // 头衔；对应于stat.xml中的level
        public string title { get; set; }
        // 是否在线标记
        public bool isOnline { get; set; }

        // 游戏统计数据部分

        //// 玩家的等级
        //public int level { get; set; }
        //// 玩家的分数
        //public int score { get; set; }
        //// 手牌
        //// 手牌空的位置用 null 表示
        //public List<Card> cardInHand /*{ get; set; }*/;
        //public Card[] cardInHand { get; set; }

        // 统计数据部分

        // 玩家的积分；计算方法见 UpdateStat
        // 升 1 级加 5 分；做 1 次台上方加 2 分；逃跑一次扣 3 分；逃跑的惩罚在program中实现
        public int grades { get; set; }

        // 玩家的等级；目前因为未解锁头像只有 6 个，所以一共是 7 级；暂定最高 7 级
        // 按照分数段来确定级别（按照 grades 来确定）
        // 100*(i-1)^2 ~ 100*i^2 为第 i 级分数段
        // 比如 3600~4900 为第 7 级
        public int level { get; set; }

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
        // 总埋底次数
        public int totalBuryTimes { get; set; }
        // 平均埋底分数
        public float aveBuryScores { get; set; }
        // 游戏总局数
        public int totalRoundTimes { get; set; }
        // 总单打次数
        public int totalSingleTimes { get; set; }
        // 总找朋友次数
        public int totalFindFriendTimes { get; set; }
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

        // 总台上次数
        public int totalUpperTimes { get; set; }
        // 胜率；平均台上方次数
        public float upperRate { get; set; }


        public PlayerInfo()
        {
            username = "";
            nickname = "";
            headImageName = "";
            title = "";
        }

        public PlayerInfo(string name)
        {
            this.username = name;
            //cardInHand = new List<Card>();
        }
        public PlayerInfo(string name, int id)
        {
            this.username = name;
            this.id = id;
            //cardInHand = new List<Card>();
        }

        public PlayerInfo(PlayerInfo other)
        {
            id = other.id;
            // 用户名
            username = other.username;
            // 昵称
            nickname = other.nickname;
            // 头像名称
            headImageName = other.headImageName;
            // 统计数据部分
            // 最高得分
            highScore = other.highScore;
            // 累计得分
            totalScore = other.totalScore;
            // 最高级数
            highLevel = other.highLevel;
            // 累计级数
            totalLevel = other.totalLevel;

            // 总抢底次数
            totalBidTimes = other.totalBidTimes;
            // 总炒底次数
            totalFryTimes = other.totalFryTimes;
            // 总埋底分数
            totalBuryScores = other.totalBuryScores;
            // 游戏总局数
            totalRoundTimes = other.totalRoundTimes;
            // 总单打次数
            totalSingleTimes = other.totalSingleTimes;
            // 总找朋友次数
            totalFindFriendTimes = other.totalFindFriendTimes;
            // 总做庄次数
            totalBankerTimes = other.totalBankerTimes;
            // 总抄底成功数
            totalBottomSuccessTimes = other.totalBottomSuccessTimes;
            // 总抄底分数
            totalBottomScores = other.totalBottomScores;
            // 最高抄底分数
            highBottomScores = other.highBottomScores;
            // 总逃跑次数
            totalRunTimes = other.totalRunTimes;

            totalBuryTimes = other.totalBuryTimes;

            aveBuryScores = other.aveBuryScores;
            totalUpperTimes = other.totalUpperTimes;
            upperRate = other.upperRate;
            level = other.level;
            grades = other.grades;
            experience = other.experience;
            title = other.title;
            isOnline = other.isOnline;
        }

        public void CopyBasicInfoFrom(PlayerInfo other)
        {
            username = other.username;
            nickname = other.nickname;
            headImageName = other.headImageName;
            experience = other.experience;
            isOnline = other.isOnline;
            title = other.title;
        }

        // 更新玩家统计信息
        public void UpdateStat()
        {
            //totalLevel += level;
            //totalScore += score;
            //highLevel = Math.Max(highLevel, level);
            //highScore = Math.Max(highScore, score);
        }

        // 更新玩家统计信息
        public void UpdateStat(
            int level/*,int score*/, int addLevel, int bidtimes, int frytimes, int burytimes, int buryscore, int singletimes, int findFriendTimes, bool isbanker,
            bool isbottomsuccess, int bottomsuccessscore, bool isUpper)
        {
            //最高级数
            highLevel = Math.Max(highLevel, level);
            //累计级数
            totalLevel += addLevel;
            // 升 1 级加 5 分
            grades += addLevel * 5;

            //总抢底次数
            totalBidTimes += bidtimes;
            //总炒底次数
            totalFryTimes += frytimes;

            // 更新总埋底分数
            totalBuryScores += buryscore;
            //总埋底次数
            totalBuryTimes += burytimes;
            //平均埋底分数
            if (totalBuryTimes == 0)
            {
                aveBuryScores = 0.0f;
            }
            else
            {
                aveBuryScores = (float)totalBuryScores / totalBuryTimes;
            }
            //游戏总局数
            totalRoundTimes++;
            //总单打次数
            totalSingleTimes += singletimes;
            // 总找朋友次数
            totalFindFriendTimes += findFriendTimes;
            //总做庄次数
            if (isbanker) totalBankerTimes++;
            //总抄底成功数            
            //总抄底分数
            //最高抄底分数
            if (isbottomsuccess)
            {
                totalBottomSuccessTimes++;
                totalBottomScores += bottomsuccessscore;
                highBottomScores = Math.Max(bottomsuccessscore, highBottomScores);
            }

            //总逃跑次数

            // 如果做了台上方
            if (isUpper)
            {
                totalUpperTimes++;
                upperRate = (float)totalUpperTimes / totalRoundTimes;
                // 做 1 次台上方，加 2 分
                grades += 2;
            }

            //totalScore += score;
            //highScore = Math.Max(highScore, score);

            // 更新级数
            if (grades < 0)
            {
                this.level = 1;
            }
            else
            {
                this.level = (int)(Math.Sqrt(grades / 100.0f)) + 1;
            }
        }

        public static int GetLevel(int experience)
        {
            // 更新级数
            if (experience < 0)
            {
               return 1;
            }
            else
            {
                return (int)(Math.Sqrt(experience / 100.0f)) + 1;
            }
        }

    }
}
