using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtility
{
    /// <summary>
    /// 专门用来给牌排序的类
    /// </summary>
    /// 
    [Serializable]
    public class CardSorter
    {
        // 台上方 ID
        int[] upperPlayersId;
        // 台上方级数
        int[] playerLevels;
        // 主级数
        int mainNumber;
        // 主花色
        int mainColor;
        // 第几局
        int m_round;

        // 深构造函数
        public CardSorter(int[] upperPlayersId, int[] playerLevels, int mainNumber, int mainColor, int round)
        {
            this.upperPlayersId = new int[upperPlayersId.Length];
            Array.Copy(upperPlayersId, this.upperPlayersId, upperPlayersId.Length);
            this.playerLevels = new int[playerLevels.Length];
            Array.Copy(playerLevels, this.playerLevels, playerLevels.Length);
            this.mainNumber = mainNumber;
            this.mainColor = mainColor;
            m_round = round;
        }

        // 用荷官来构造排序家
        public CardSorter(Dealer dealer)
        {
            upperPlayersId = new int[dealer.upperPlayersId.Length];
            Array.Copy(dealer.upperPlayersId, upperPlayersId, upperPlayersId.Length);
            playerLevels = new int[dealer.playerLevels.Length];
            Array.Copy(dealer.playerLevels, playerLevels, playerLevels.Length);
            mainNumber = dealer.mainNumber;
            mainColor = dealer.mainColor;
            m_round = dealer.round;
        }

        // 深构造函数：用另外一个排序家来构造
        // 用荷官来构造排序家
        public CardSorter(CardSorter other)
        {
            upperPlayersId = new int[other.upperPlayersId.Length];
            Array.Copy(other.upperPlayersId, upperPlayersId, upperPlayersId.Length);
            playerLevels = new int[other.playerLevels.Length];
            Array.Copy(other.playerLevels, playerLevels, playerLevels.Length);
            mainNumber = other.mainNumber;
            mainColor = other.mainColor;
            m_round = other.m_round;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="cards"></param>
        void Sort(ref List<Card> cards, int mode)
        {
            // 不对空的手牌进行排序
            if (cards.Count <= 0)
            {
                return;
            }
            List<Card> temp = new List<Card>();

            switch (mode)
            {
                // 假设要排序的牌组里面没有大小鬼；将各种花色的牌分开，个数多的在前；同花色中，按照点数A、K、Q、J、10、9、8、7、6、5、4、3、2的顺序排列
                case 0:
                    List<Card>[] suitCards = new List<Card>[4];
                    // 先将不同花色的牌分开一下
                    for (int i = 0; i < suitCards.Length; i++)
                    {
                        suitCards[i] = new List<Card>(cards.FindAll(card => card.suit == (Card.Suit)i));
                    }
                    // 获取各组的个数
                    int[] suitCounts = Array.ConvertAll(suitCards, cardList => cardList.Count);
                    // 然后根据各组的大小，从多到少插入到牌组里去
                    for (int j = 0; j < suitCounts.Length; j++)
                    {
                        int idx = Array.FindIndex(suitCounts, number => number == suitCounts.Max());
                        // 置为负数，免得影响后面求最值
                        suitCounts[idx] = -1;
                        // 将这组牌按照要求的点数顺序排好
                        // 先拿出所有的 A
                        int count = suitCards[idx].RemoveAll(card => card.points == 0);
                        // 放到temp里面去
                        for (int i = 0; i < count; i++)
                        {
                            temp.Add(new Card((Card.Suit)idx, 0));
                        }
                        // 然后倒序
                        for (int i = 12; i >= 1; i--)
                        {
                            // 拿出所有的该点数的牌
                            count = suitCards[idx].RemoveAll(card => card.points == i);
                            // 放到temp里面去
                            for (int k = 0; k < count; k++)
                            {
                                temp.Add(new Card((Card.Suit)idx, i));
                            }
                        }
                    }
                    break;
                // 假设要排序的牌组都是同点数的，要按照同花色个数从多到少排列
                case 1:
                    // 获取点数
                    int points = cards[0].points;
                    int[] suitNumbers = new int[4];
                    // 来统计一下各种花色有多少张
                    suitNumbers[(int)Card.Suit.Club] = cards.RemoveAll(card => card.suit == Card.Suit.Club);
                    suitNumbers[(int)Card.Suit.Diamond] = cards.RemoveAll(card => card.suit == Card.Suit.Diamond);
                    suitNumbers[(int)Card.Suit.Heart] = cards.RemoveAll(card => card.suit == Card.Suit.Heart);
                    suitNumbers[(int)Card.Suit.Spade] = cards.RemoveAll(card => card.suit == Card.Suit.Spade);
                    // 然后按照个数，从大到小依次加入到序列
                    for (int j = 0; j < suitNumbers.Length; j++)
                    {
                        int idx = Array.FindIndex(suitNumbers, number => number == suitNumbers.Max());
                        // 获取花色
                        Card.Suit suit = (Card.Suit)idx;
                        // 获取数目
                        int count = suitNumbers[idx];
                        // 置为负数，免得影响后面求最值
                        suitNumbers[idx] = -1;
                        // 加入牌组
                        for (int k = 0; k < count; k++)
                        {
                            temp.Add(new Card(suit, points));
                        }
                    }
                    break;
                // 假设要排序的牌组里面没有大小鬼；将各种花色的牌按黑桃-红桃-梅花-方块排好；同花色中，按照点数A、K、Q、J、10、9、8、7、6、5、4、3、2的顺序排列
                case 2:
                    suitCards = new List<Card>[4];
                    // 先将不同花色的牌分开一下
                    for (int i = 0; i < suitCards.Length; i++)
                    {
                        suitCards[i] = new List<Card>(cards.FindAll(card => card.suit == (Card.Suit)i));
                    }
                    // 按黑桃-红桃-梅花-方块排好
                    int[] suitCodes = new int[4] { (int)Card.Suit.Spade, (int)Card.Suit.Heart, (int)Card.Suit.Club, (int)Card.Suit.Diamond };

                    for (int j = 0; j < suitCodes.Length; j++)
                    {
                        // 将这组牌按照要求的点数顺序排好
                        // 先拿出所有的 A
                        int count = suitCards[suitCodes[j]].RemoveAll(card => card.points == 0);
                        // 放到temp里面去
                        for (int i = 0; i < count; i++)
                        {
                            temp.Add(new Card((Card.Suit)suitCodes[j], 0));
                        }
                        // 然后倒序
                        for (int i = 12; i >= 1; i--)
                        {
                            // 拿出所有的该点数的牌
                            count = suitCards[suitCodes[j]].RemoveAll(card => card.points == i);
                            // 放到temp里面去
                            for (int k = 0; k < count; k++)
                            {
                                temp.Add(new Card((Card.Suit)suitCodes[j], i));
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
            // 返回排序的结果
            cards = new List<Card>(temp);
        }

        // 规则组的主级数编码到网络的Card编码
        int MainNumber2Points(int n)
        {
            return (n + 1) % 13;
        }

        // 规则组的主级数编码到网络的Card编码
        Card.Suit MainColor2Suit(int n)
        {
            switch (n)
            {
                case 0:
                    return Card.Suit.Diamond;
                case 1:
                    return Card.Suit.Spade;
                case 2:
                    return Card.Suit.Club;
                case 3:
                    return Card.Suit.Heart;
                // 无将的情况就用小鬼表示
                default:
                    return Card.Suit.Joker0;
            }
        }

        /// <summary>
        /// 给特定玩家的牌排好序，方便他出牌
        /// 如果是出牌阶段调用，要求已经确定好台上方，玩家级数
        /// </summary>
        /// <param name="cards">要排序的牌组</param>
        /// <param name="playerId">玩家 ID</param>
        public void Sort(ref List<Card> cards, int playerId, /*GameStateMachine.State state*/int mode)
        {
            /// <summary>
            /// 检查某玩家是否为台上方玩家
            /// </summary>
            /// <param name="id">要检查的玩家 ID</param>
            /// <returns></returns>
            bool IsUpperPlayer(int id)
            {
                return Array.IndexOf(upperPlayersId, id) >= 0;
            }

            // 不对空的；或者只有 1 张手牌进行排序
            if (cards.Count <= 1)
            {
                return;
            }
            List<Card> temp = new List<Card>();
            // 先找大王
            int numberOfBigJokers = cards.RemoveAll(card => card.suit == Card.Suit.Joker1);
            for (int i = 0; i < numberOfBigJokers; i++)
            {
                temp.Add(new Card(Card.Suit.Joker1, 13));
            }
            // 再找小鬼
            int numberOfLittleJokers = cards.RemoveAll(card => card.suit == Card.Suit.Joker0);
            for (int i = 0; i < numberOfLittleJokers; i++)
            {
                temp.Add(new Card(Card.Suit.Joker0, 13));
            }
            switch (mode)
            {
                // 发牌阶段；没有完全确定主花色和主级数
                case /*GameStateMachine.State.Deal*/0:
                    // 然后是主牌：即点数是台上方级数的牌，对台下方，级数大的在前，小的在后；对台上方，自己的级数在前，其他级数从大到小；同级数里面，数量多的花色在前，少的在后，数量一样的，随便
                    // 先来排一下级数
                    int[] tempLevels = new int[upperPlayersId.Length];
                    for (int i = 0; i < upperPlayersId.Length; i++)
                    {
                        tempLevels[i] = playerLevels[upperPlayersId[i]];
                    }
                    // 将重复的级数去掉
                    tempLevels = tempLevels.Distinct().ToArray();
                    // 如果这玩家是台上方的
                    if (IsUpperPlayer(playerId))
                    {
                        // 先把他自己级数的牌抽出来
                        List<int> tempLevelsList = tempLevels.ToList();
                        tempLevelsList.RemoveAll(level => level == playerLevels[playerId]);
                        tempLevels = tempLevelsList.ToArray();

                        List<Card> levelCards = cards.FindAll(card => card.points == (playerLevels[playerId] - 1) % 13);
                        cards.RemoveAll(card => card.points == (playerLevels[playerId] - 1) % 13);
                        Sort(ref levelCards, 1);
                        temp.AddRange(levelCards);

                        // 然后才把其他台上方级数的牌排一下序
                        // 从大到小排序
                        Array.Sort(tempLevels, (level1, level2) => level2.CompareTo(level1));
                        // 对每一个级数
                        for (int i = 0; i < tempLevels.Length; i++)
                        {
                            levelCards = cards.FindAll(card => card.points == (tempLevels[i] - 1) % 13);
                            cards.RemoveAll(card => card.points == (tempLevels[i] - 1) % 13);
                            Sort(ref levelCards, 1);
                            temp.AddRange(levelCards);
                        }
                    }
                    // 否则，如果这玩家是台下方的
                    else
                    {
                        // 从大到小排序
                        Array.Sort(tempLevels, (level1, level2) => level2.CompareTo(level1));
                        // 对每一个级数
                        for (int i = 0; i < tempLevels.Length; i++)
                        {
                            List<Card> levelCards = cards.FindAll(card => card.points == (tempLevels[i] - 1) % 13);
                            cards.RemoveAll(card => card.points == (tempLevels[i] - 1) % 13);
                            Sort(ref levelCards, 1);
                            temp.AddRange(levelCards);
                        }
                    }
                    // 接下来排一下副牌，按同花色的数目从多到少排列；而且一个花色里面，点数按 A、K、Q、J、10、9、8、7、6、5、4、3、2 顺序排列
                    Sort(ref cards, 2);
                    temp.AddRange(cards);
                    break;
                // 其他阶段，假定已经确定主级数和主花色
                case 1:
                    //// 如果是首盘
                    //if (m_round == 1)
                    //{
                    //    // 先拿出来所有的 A
                    //    List<Card> levelCards = cards.FindAll(card => card.points == 0);
                    //    cards.RemoveAll(card => card.points == 0);
                    //    Sort(ref levelCards, 1);
                    //    temp.AddRange(levelCards);
                    //    // 然后再排剩下的牌
                    //    Sort(ref cards, 2);
                    //    temp.AddRange(cards);
                    //}
                    //// 如果不是首盘
                    //else
                    //{
                        int points = MainNumber2Points(mainNumber);
                        Card.Suit suit = MainColor2Suit(mainColor);
                        // 如果不是无将
                        if (mainColor != 4)
                        {
                            // 先排出来点数是主级数，花色是主花色的牌
                            List<Card> mainCards = cards.FindAll(card => card.points == points && card.suit == suit);
                            cards.RemoveAll(card => card.points == points && card.suit == suit);
                            temp.AddRange(mainCards);

                            // 然后排出来点数是主级数，其他花色的牌
                            mainCards = cards.FindAll(card => card.points == points);
                            cards.RemoveAll(card => card.points == points);
                            Sort(ref mainCards, 1);
                            temp.AddRange(mainCards);

                            // 然后是其他点数，但花色是主花色的牌
                            mainCards = cards.FindAll(card => card.suit == suit);
                            cards.RemoveAll(card => card.suit == suit);
                            Sort(ref mainCards, 2);
                            temp.AddRange(mainCards);

                            // 最后是其他牌
                            mainCards = new List<Card>(cards);
                            Sort(ref mainCards, 2);
                            temp.AddRange(mainCards);
                        }
                        // 如果打的是无将
                        else
                        {
                            // 先排出来点数是主级数的牌，花色按个数从大到小排列
                            List<Card> mainCards = cards.FindAll(card => card.points == points);
                            cards.RemoveAll(card => card.points == points);
                            Sort(ref mainCards, 1);
                            temp.AddRange(mainCards);

                            // 然后排出来其他点数的牌
                            mainCards = new List<Card>(cards);
                            Sort(ref mainCards, 2);
                            temp.AddRange(mainCards);
                        }
                    //}
                    break;
            }
            // 完成排序，将数组放回传进来的牌组里
            cards = new List<Card>(temp);
        }

    }
}
