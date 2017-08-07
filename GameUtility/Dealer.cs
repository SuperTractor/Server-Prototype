using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtility
{
    public class cardComb
    {
        public
          bool valid;
        public
          bool thrown;
        public
          int mainNumber, mainColor; // 花色编号01234对应方块 黑桃 梅花 红桃 无主 mainNumber对应CardList编码方式
        public
          int thisColor; // 有主：花色编号0123对应方块 黑桃 梅花 红桃 其他主牌与主花色牌编在一起 无主：4对应主牌
        public
          int thisSame; // 记录几同
        public
          int thisType; // 0 拖拉机 1 单张 2 对子 3 三同 4 四同
        public
          int Count; // 记录总牌数
        public
          List<int> data; // 记录内容
        public
          CardList raw; // 原始数据
        public
          int TransFromCardList(int a)
        {
            return a % 13 == 12 ? 1 : a % 13 + 2;
        }
        public
          cardComb(CardList init, int mn, int mc)
        {
            Count = 0;
            raw = init;
            valid = true;
            thrown = false;
            mainNumber = mn;
            mainColor = mc; // 初始化主级数与主花色
            data = new List<int>();

            // 判断花色在同一区间 合法性来源
            bool mainOccur = false;
            int count = 0;
            if (mainColor == 4)
            {

                for (int i = 0; i < 4; i++)
                    for (int j = i * 13; j < (i + 1) * 13; j++)
                    {
                        if (init.data[j] > 0)
                        {
                            if (j - i * 13 != mainNumber)
                            {
                                count++;
                                thisColor = i;
                                break;
                            }
                            else
                            {
                                mainOccur = true;
                                thisColor = 4;
                            }
                        }
                    }

                if (init.data[52] > 0 || init.data[53] > 0)
                {
                    mainOccur = true;
                    thisColor = 4;
                }
                if ((mainOccur && count > 0) || (!mainOccur && count >= 2))
                {
                    valid = false;
                    return;
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                    for (int j = i * 13; j < (i + 1) * 13; j++)
                        if (init.data[j] > 0)
                        {
                            if (i == mainColor)
                            {
                                mainOccur = true;
                                thisColor = mainColor;
                            }
                            else
                            {
                                if (j - i * 13 != mainNumber)
                                {
                                    count++;
                                    thisColor = i;
                                    break;
                                }
                                else
                                {
                                    mainOccur = true;
                                    thisColor = mainColor;
                                }
                            }
                        }

                if (init.data[52] > 0 || init.data[53] > 0)
                {
                    mainOccur = true;
                    thisColor = mainColor;
                }
                if ((mainOccur && count > 0) || (!mainOccur && count >= 2))
                {
                    valid = false;
                    return;
                }
            }

            // 整齐
            count = 0;
            bool[] oc = { false, false, false, false, false };
            for (int i = 0; i < 54; i++)
                if (init.data[i] > 0)
                {
                    for (int j = 0; j < init.data[i]; j++)
                        Count++; // 计算总牌数
                    if (oc[init.data[i]])
                    {
                        if (init.data[i] == 1)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                    }
                    else
                    {
                        oc[init.data[i]] = true;
                        count++;
                        if (count == 2)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                    }
                }
            if (oc[1])
            {
                thisSame = 1;
                return;
            }
            for (int i = 2; i <= 4; i++)
                if (oc[i])
                    thisSame = i;

            // 相连
            bool flag = false;
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (init.data[i * 13 + mainNumber] > 0)
                        {
                            data.Add(i * 13 + mainNumber);
                            flag = true;
                        }
                    // 大小王
                    if (flag && init.data[52] == 0 && init.data[53] > 0)
                    {
                        thrown = true;
                        goto isThrown;
                    }
                    if (init.data[52] > 0)
                        data.Add(52);
                    if (init.data[53] > 0)
                        data.Add(53);
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (init.data[i] > 0)
                            {
                                int temp = i - 1 - thisColor * 13;
                                if (temp == mainNumber)
                                    temp--;
                                if (temp > 0 && init.data[temp] == 0 && flag)
                                {
                                    thrown = true;
                                    goto isThrown;
                                }
                                data.Add(i);
                                flag = true;
                            }
                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (init.data[i] > 0)
                            {
                                int temp = i - 1 - thisColor * 13;
                                if (temp == mainNumber)
                                    temp--;
                                if (temp > 0 && init.data[temp] == 0 && flag)
                                {
                                    thrown = true;
                                    goto isThrown;
                                }
                                data.Add(i);
                                flag = true;
                            }
                    // 副主级牌
                    int count1 = 0;
                    bool viceOccurFlag = false;
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && init.data[i * 13 + mainNumber] > 0)
                        {
                            viceOccurFlag = true;
                            int temp = init.data[init.data.Count() - 1] % 13;
                            if (flag && ((temp != 11 && mainNumber == 12) || (temp != 12 && mainNumber != 12)))
                            {
                                thrown = true;
                                goto isThrown;
                            }
                            data.Add(i * 13 + mainNumber);
                            count1++;
                            if (count1 > 1)
                            {
                                thrown = true;
                                goto isThrown;
                            }
                            flag = true;
                        }
                    // 主级牌
                    bool mainOccurFlag = false;
                    if (init.data[mainColor * 13 + mainNumber] > 0)
                    {
                        mainOccurFlag = true;
                        if (flag && !viceOccurFlag)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                        data.Add(mainColor * 13 + mainNumber);
                        flag = true;
                    }
                    // 小王
                    bool blackJokerOccurFlag = false;
                    if (init.data[52] > 0)
                    {
                        blackJokerOccurFlag = true;
                        if (flag && !mainOccurFlag)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                        data.Add(52);
                        flag = true;
                    };
                    // 大王
                    if (init.data[53] > 0)
                    {
                        if (flag && !blackJokerOccurFlag)
                        {
                            thrown = true;
                            goto isThrown;
                        }
                        data.Add(53);
                        flag = true;
                    };
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (init.data[i] > 0)
                            {
                                int temp = i - 1 - thisColor * 13;
                                if (temp == mainNumber)
                                    temp--;
                                if (temp > 0 && init.data[temp] == 0 && flag)
                                {
                                    thrown = true;
                                    goto isThrown;
                                }
                                data.Add(i);
                                flag = true;
                            }
                }
            }

            //确定类型 拖拉机 单张 对子 三同 四同
            if (thisSame == 2 && data.Count > 1)
                thisType = 0;
            else
                thisType = thisSame;
            return;

            // 甩牌 记录最长拖拉机 TODO
            isThrown:
            {
            }
            // 重新初始化
        }
    }

    public class RulePlayer
    {
        public CardList cardInHand;
        public RulePlayer()
        {
            cardInHand = new CardList();
        }
    }

    public class CardList
    {
        public
          int[] data; // 编号按照0~11->2~K, 12->A
        public
          CardList()
        {
            data = new int[54];
        }
    };

    public class Dealer
    {
        public int mainNumber, mainColor;

        // 全牌: 全部 4 副牌
        private Card[] m_totalCard;
        // 8 张底牌
        private Card[] m_bottom;
        // 分发给玩家的手牌
        private Card[] m_playerCard;
        // 玩家当前的手牌
        public List<Card>[] playersHandCard { get; set; }

        // 玩家的级数，由服务器主程序同步进来
        public int[] playerLevels { get; set; }

        // 底牌接口
        public Card[] bottom
        {
            get { return m_bottom; }
            set { m_bottom = value; }
        }
        // 4 个玩家的手牌暂存地（发牌阶段用）
        public Card[] playerCard
        {
            get { return m_playerCard; }
        }
        // 出牌要求牌数
        public int dealRequiredLength { get; set; }
        // 当前首家 ID，即第一个出牌玩家 ID
        public int firstHomePlayerId { get; set; }

        // 玩家总数
        public const int playerNumber = 4;

        // 当前玩家ID
        private int m_currentPlayerId;
        public int currentPlayerId { get { return m_currentPlayerId; } }

        // 当前台上方玩家ID
        private int[] m_upperPlayersId;
        public int[] upperPlayersId { get { return m_upperPlayersId; } }

        // 一共多少副牌
        public const int packNumber = 4;
        // 底牌的张数
        public const int bottomCardNumber = 8;
        // 初始时玩家拥有的手牌数
        public const int cardInHandInitialNumber = (packNumber * Card.cardNumberOfOnePack - bottomCardNumber) / 4;
        // 每个玩家手牌的最大数目
        public const int cardInHandMaxNumber = cardInHandInitialNumber + bottomCardNumber;

        // 抢底阶段，当前所有玩家的亮牌
        public List<Card>[] currentBidCards;
        // 每个玩家当前的合法亮牌花色
        //public List<Card.Suit>[] currentLegalBidColors;

        // 抢到底牌的玩家 ID
        public int gotBottomPlayerId { get; set; }
        // 抢到底牌的玩家的亮牌花色
        public Card.Suit gotBottomSuit { get; set; }

        // 当前炒底玩家
        private int m_currentFryPlayerId;
        public int currentFryPlayerId
        {
            get
            {
                return m_currentFryPlayerId;
            }
            set
            {
                m_currentFryPlayerId = value % playerNumber;
            }
        }
        // 炒底亮牌数下界，下一玩家必须亮牌后总数必须比这个数多
        public int fryCardLowerBound { get; set; }
        // 超出 5 个亮牌数就算最大
        private int m_fryCardLimit = 5;
        // 累计已经有多少玩家跳过炒底；一旦有 3 个玩家选择不跟，则炒底结束；一旦有 1 个玩家跟，重置此计数器
        public int skipFryCount { get; set; }
        // 目前各玩家亮出的筹码牌
        public List<Card>[] showCards;
        // 庄家的 id
        public List<int> bankerPlayerId;

        // 庄家是否单打
        public bool bankerIsFightAlone;
        // 信号牌
        public Card signCard;

        // 这一轮已经出牌的玩家个数
        private int m_handOutPlayerCount;
        public int handOutPlayerCount
        {
            get
            {
                return m_handOutPlayerCount;
            }
            set
            {
                m_handOutPlayerCount = value % 4;
            }
        }
        // 当前轮数，4 个玩家都出 1 次牌为一轮
        private int m_circle = 1;
        public int circle
        {
            get { return m_circle; }
            set { m_circle = value; }
        }
        // 4 个玩家本轮的出牌
        public List<Card>[] handOutCards;

        // 当前盘数，出完手牌为 1 盘
        private int m_round = 1;
        public int round
        {
            get { return m_round; }
            set { m_round = value; }
        }

        /// <summary>
        /// 构造函数, 初始化荷官
        /// 获取 4 副牌
        /// </summary>
        public Dealer()
        {
            m_totalCard = new Card[Card.cardNumberOfOnePack * packNumber];
            for (int i = 0; i < packNumber; i++)
            {
                Card.GetCardSet().CopyTo(m_totalCard, i * Card.cardNumberOfOnePack);
            }
            m_playerCard = new Card[cardInHandInitialNumber * 4];
            m_bottom = new Card[bottomCardNumber];

            playerLevels = new int[playerNumber];

            playersHandCard = new List<Card>[playerNumber];

            currentBidCards = new List<Card>[playerNumber];
            //currentLegalBidColors = new List<Card.Suit>[playerNumber];
            //Shuffle();
            //Cut();
            // 为炒底阶段玩家亮牌分配内存
            showCards = new List<Card>[playerNumber];
            // 为玩家对战阶段出牌分配内存
            handOutCards = new List<Card>[playerNumber];

            for (int i = 0; i < playerNumber; i++)
            {
                playersHandCard[i] = new List<Card>();
                showCards[i] = new List<Card>();
                handOutCards[i] = new List<Card>();
                currentBidCards[i] = new List<Card>();
            }
            bankerPlayerId = new List<int>();
        }
        // 洗牌
        public void Shuffle()
        {
            Random rnd = new Random();
            int rand_idx;
            Card temp;
            for (int i = 0; i < m_totalCard.Length; i++)
            {
                rand_idx = rnd.Next(m_totalCard.Length);
                temp = m_totalCard[i];
                m_totalCard[i] = m_totalCard[rand_idx];
                m_totalCard[rand_idx] = temp;
            }
        }
        // 多次洗牌
        public void Shuffle(int times)
        {
            for (int i = 0; i < times; i++)
            {
                Shuffle();
            }
        }

        // 分牌, 发牌的前一步
        // 把底牌抽出来再把剩下的牌分发给玩家
        public void Cut()
        {
            Array.Copy(m_totalCard, bottomCardNumber, m_playerCard, 0, cardInHandInitialNumber * 4);
            Array.Copy(m_totalCard, m_bottom, bottomCardNumber);
        }

        // 抢底阶段：给出此玩家当前可以亮的花色，已知当前摸牌的数目
        public bool[] GetLegalBidColors(int playerId/*, int touchCardNumber*/)
        {
            bool[] legalBidColors = new bool[4];
            // 构造当前该玩家已经摸到的牌
            //Card[] currentTouchCards = new Card[touchCardNumber];
            //playersHandCard[playerId].CopyTo(0, currentTouchCards, 0, touchCardNumber);
            // 检查该玩家是否为台上方玩家
            bool isUpperPlayer = Array.IndexOf(m_upperPlayersId, playerId) >= 0;
            // 只有台上方玩家才能抢底
            if (isUpperPlayer)
            {
                List<Card> levelCards;
                // 如果该玩家还没有亮牌
                if (currentBidCards[playerId].Count == 0)
                {
                    // 获取目前该玩家的摸牌当中，所有的级牌
                    levelCards = playersHandCard[playerId].FindAll(card => card.points + 1 == playerLevels[playerId]);

                    for (int i = 0; i < levelCards.Count; i++)
                    {
                        // 姑且认为可以亮出此花色的牌
                        int k = (int)levelCards[i].suit;
                        if (!legalBidColors[k])
                        {
                            legalBidColors[k] = true;
                        }
                    }
                }
                else// 如果该玩家已经亮了牌
                {
                    // 获取该玩家目前所有的级牌，而且花色要和他已经亮过的牌一致
                    levelCards = playersHandCard[playerId].FindAll(card => card.points + 1 == playerLevels[playerId] && card.suit == currentBidCards[playerId][0].suit);

                    // 姑且认为可以亮出此花色的牌
                    if (levelCards.Count > 0)
                    {
                        int k = (int)levelCards[0].suit;
                        if (!legalBidColors[k])
                        {
                            legalBidColors[k] = true;
                        }
                    }
                }
                // 根据其他玩家已经亮的牌决定此玩家可以亮的花色
                for (int i = 0; i < legalBidColors.Length; i++)
                {
                    // 如果先前认定这花色可以出
                    if (legalBidColors[i])
                    {
                        // 计算这花色的摸牌，加上已经亮的牌，总共多少张
                        // 只有当总数大于当前亮牌数最大的玩家，才可以出这个花色的牌
                        int totalNum = levelCards.Count(card => card.suit == (Card.Suit)i) + currentBidCards[playerId].Count;
                        legalBidColors[i] = totalNum > currentBidCards.Max(cards => cards.Count);
                    }
                }
            }
            else
            {
            }
            return legalBidColors;
        }
        // 亮牌需要增加的数目
        //public int BidNeedNumber(int playerId)
        //{
        //    return currentBidCards.Max(cards => cards.Count) + 1 - currentBidCards[playerId].Count;
        //}
        // 抢底阶段：亮牌帮助函数
        public void BidHelper(int playerId, int currentTouchNumber, Card.Suit suit)
        {
            int bidNeedNumber = currentBidCards.Max(cards => cards.Count) + 1 - currentBidCards[playerId].Count;
            // 构造一张级牌
            Card levelCard = new Card(suit, playerLevels[playerId] - 1);
            for (int i = 0; i < bidNeedNumber; i++)
            {
                // 找到级牌所在位置
                //int idx = playersHandCard[playerId].FindIndex(card => card.suit == suit && card.points + 1 == playerLevels[playerId]);
                // 去除一张级牌
                if (!playersHandCard[playerId].Remove(levelCard))
                {
                    Console.WriteLine("抢底：判断级牌数是否足够，出错");
                }
                // 加入亮牌当中
                currentBidCards[playerId].Add(levelCard);
            }
            // 更新当前抢到底牌的玩家 ID
            gotBottomPlayerId = playerId;
        }

        // 生成信号牌
        // 随机抽一张非硬主的牌
        public void GenerateSignCard()
        {
            // 获取一副牌
            List<Card> cardSet = new List<Card>(Card.GetCardSet());
            // 首先不能抽大小王
            cardSet.Remove(new Card(Card.Suit.Joker0, 13));
            cardSet.Remove(new Card(Card.Suit.Joker1, 13));
            // 其次，不能抽点数是台上方玩家级数的牌
            for (int i = 0; i < m_upperPlayersId.Length; i++)
            {
                cardSet.RemoveAll(card => card.points == playerLevels[m_upperPlayersId[i]]);
            }
            // 然后随机抽
            Random rdn = new Random();
            int idx = rdn.Next() % cardSet.Count;
            signCard = cardSet[idx];
        }

        // 判断炒底时增加的筹码牌是否合法
        public Judgement IsLegalShow(Card[] addFryCards, int playerId)
        {
            // 测试：总亮牌数比前一个炒底玩家的多就行
            bool isValid;
            isValid = addFryCards.Length + showCards[playerId].Count > fryCardLowerBound;
            string message;
            if (isValid)
            {
                message = "合法亮牌";
            }
            else
            {
                message = "筹码不够大";
            }
            return new Judgement(message, isValid);
        }

        // 判断埋底的合法性（炒底阶段）
        // 测试：只要埋下的牌数等于 8 就算合法
        public Judgement IsLegalBury(Card[] cards)
        {
            bool isValid = cards.Length == bottomCardNumber;
            string message;
            if (isValid)
            {
                message = "合法埋底";
            }
            else
            {
                message = "必须埋 8 张底牌";
            }
            return new Judgement(message, isValid);
        }

        // Player到RulePlayer的转换
        RulePlayer[] PlayerToRulePlayer(PlayerInfo[] res)
        {
            RulePlayer[] tmp = new RulePlayer[4];
            for(int i = 0; i < 4; i++)
            {
                tmp[i] = new RulePlayer();
            }
            for (int i = 0; i < 4; i++)
                foreach (Card j in res[i].cardInHand)
                    if (j != null)
                        tmp[i].cardInHand.data[j.CardToIndex()]++;
            return tmp;
        }

        // CardArray到CardList的转换
        CardList CardArrayToCardList(Card[] res)
        {
            CardList tmp = new CardList();
            foreach (Card j in res)
                if (j != null)
                    tmp.data[j.CardToIndex()]++;
            return tmp;
        }

        // CardArray到CardList的转换
        CardList CardListToCardList(List<Card> res)
        {
            CardList tmp = new CardList();
            foreach (Card j in res)
                if (j != null)
                    tmp.data[j.CardToIndex()]++;
            return tmp;
        }

        /// <summary>
        /// 判断两张牌的大小
        /// </summary>
        /// <param name="a">牌1，用54个数字表示</param>
        /// <param name="b">牌2，用54个数字表示</param>
        /// <param name="mainColor"></param>
        /// <param name="mainNumber"></param>
        /// <returns>a>b返回true，否则返回false</returns>
        bool biggerThan(int a, int b, int mainColor, int mainNumber)
        {
            if (a == -1)
                return false;
            if (b == -1)
                return true;
            if (a == b)
                return false;
            if (mainColor == 4)
            {
                // 大小王
                if (a == 53 || (a == 52 && b != 53))
                    return true;
                if (b == 53 || (b == 52 && a != 53))
                    return false;
                // 主级牌
                if (a % 13 == mainNumber && b % 13 == mainNumber)
                    return false;
                if (a % 13 == mainNumber && b % 13 != mainNumber)
                    return true;
                if (a % 13 != mainNumber && b % 13 == mainNumber)
                    return false;
                // 副牌
                if (a % 13 > b % 13)
                    return true;
                else
                    return false;
            }
            else // 有主
            {
                // 大小王
                if (a == 53 || (a == 52 && b != 53))
                    return true;
                if (b == 53 || (b == 52 && a != 53))
                    return false;
                // 主级牌
                if (b % 13 == mainNumber && b / 13 == mainColor)
                    return false;
                if (a % 13 == mainNumber && a / 13 == mainColor)
                    return true;
                // 副主级牌
                if (b % 13 == mainNumber && b / 13 != mainColor)
                    return false;
                if (a % 13 == mainNumber && a / 13 != mainColor)
                    return true;
                //主花色牌
                if (a / 13 == mainColor)
                    if (b / 13 == mainColor)
                    {
                        if (a % 13 > b % 13)
                            return true;
                        else
                            return false;
                    }
                    else
                        return true;
                // 副牌
                if (a % 13 > b % 13)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 找出长度为k的最大的拖拉机
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        List<int> findtractor(CardList temp, int k, int thisColor, int mainColor, int mainNumber)
        {
            List<int> maxtractor = new List<int>();
            List<int> currenttractor = new List<int>();
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (temp.data[i * 13 + mainNumber] > 1)
                        {
                            currenttractor.Add(i);
                        }
                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            //如果拖拉机长度大于等于k且最大牌较大,则将最大的k张牌赋值到maxtractor中
                            if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                            {
                                maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                            }

                        }

                    }
                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                    {
                        maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                    }
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                                }

                            }
                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                    {
                        maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                    }

                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                                }

                            }
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && temp.data[i * 13 + mainNumber] > 0)
                        {
                            int j = i * 13 + mainNumber;
                            if (temp.data[j] > 1)
                            {
                                currenttractor.Add(j);

                            }
                            else
                            {

                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                                }

                            }
                        }
                    // 主级牌
                    if (temp.data[mainColor * 13 + mainNumber] > 1)
                    {
                        currenttractor.Add(mainColor * 13 + mainNumber);

                    }
                    else
                    {
                        if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                        {
                            maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                        }
                    }



                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                            {
                                maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                            }

                        }

                    }
                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                    {
                        maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                    }
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                                {
                                    maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                                }

                            }
                    if (currenttractor.Count() >= k && biggerThan(currenttractor.Last(), maxtractor.Last(), mainColor, mainNumber))
                    {
                        maxtractor = (List<int>)currenttractor.Skip(Math.Max(0, currenttractor.Count() - k)).Take(k);
                    }

                }
            }

            return maxtractor;

        }
        /// <summary>
        /// 找出最长的拖拉机
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        List<int> longtractor(CardList temp, int thisColor, int mainColor, int mainNumber)
        {
            List<int> maxtractor = new List<int>();
            List<int> currenttractor = new List<int>();
            if (mainColor == 4)
            {
                if (thisColor == 4)
                {
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (temp.data[i * 13 + mainNumber] > 1)
                        {
                            currenttractor.Add(i);
                        }
                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                            currenttractor.Clear();

                        }

                    }
                    maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                    currenttractor.Clear();
                }
                else
                {
                    // 四种花色副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                                currenttractor.Clear();

                            }
                    maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                    currenttractor.Clear();

                }
            }
            else
            {
                if (thisColor == mainColor)
                {
                    // 主花色牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                                currenttractor.Clear();

                            }
                    // 副主级牌
                    for (int i = 0; i < 4; i++)
                        if (i != mainColor && temp.data[i * 13 + mainNumber] > 0)
                        {
                            int j = i * 13 + mainNumber;
                            if (temp.data[j] > 1)
                            {
                                currenttractor.Add(j);

                            }
                            else
                            {

                                maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                                currenttractor.Clear();

                            }
                        }
                    // 主级牌
                    if (temp.data[mainColor * 13 + mainNumber] > 1)
                    {
                        currenttractor.Add(mainColor * 13 + mainNumber);

                    }
                    else
                    {
                        maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                        currenttractor.Clear();
                    }



                    // 大小王
                    for (int i = 52; i < 54; i++)
                    {
                        if (temp.data[i] > 1)
                        {
                            currenttractor.Add(i);

                        }
                        else
                        {
                            maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                            currenttractor.Clear();

                        }

                    }
                    maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                    currenttractor.Clear();
                }
                else
                {
                    // 副牌
                    for (int i = thisColor * 13; i < (thisColor + 1) * 13; i++)
                        if (i - thisColor * 13 != mainNumber)
                            if (temp.data[i] > 1)
                            {
                                currenttractor.Add(i);

                            }
                            else
                            {
                                maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                                currenttractor.Clear();

                            }
                    maxtractor = currenttractor.Count() > maxtractor.Count() ? currenttractor : maxtractor;
                    currenttractor.Clear();

                }
            }
            return maxtractor;
        }

        /// <summary>
        /// 判断首位甩牌的合法性
        /// </summary>
        /// <param name="firstCard">首位出牌</param>
        /// <param name="player">四位玩家</param>
        /// <param name="thisplayer">当前玩家</param>
        /// <returns>false不合法，true合法</returns>
        bool canThrow(CardList firstCard, RulePlayer[] player, int thisplayer)
        {
            List<int> maxtractor = new List<int>();
            List<int> currenttractor = new List<int>();
            List<int>[] maxother = new List<int>[4];
            cardComb fc = new cardComb(firstCard, mainNumber, mainColor);

            if (!fc.valid)
                return false;

            //找出所有拖拉机
            while (true)
            {
                maxtractor.Clear();
                currenttractor.Clear();
                //找出最长拖拉机
                maxtractor = longtractor(firstCard, fc.thisColor, mainColor, mainNumber);
                int length = maxtractor.Count();
                //若为拖拉机，先比较合法性
                if (length > 1)
                {
                    int p = 0;
                    for (p = 0; p < 4; p++)
                    {
                        if (p != thisplayer)
                        {
                            maxother[p].Clear();
                            maxother[p] = findtractor(player[p].cardInHand, length, fc.thisColor, mainColor, mainNumber);
                            if (biggerThan(maxother[p].Last(), maxtractor.Last(), mainColor, mainNumber))
                            {
                                return false;
                            }

                        }
                    }
                }

                //没有拖拉机则结束循环
                else
                {
                    break;

                }
                //合法则将比较过的拖拉机删去后继续循环
                foreach (int j in maxtractor)
                {
                    firstCard.data[j] -= 2;
                }
                for (int p = 0; p < 4; p++)
                {
                    foreach (int j in maxother[p])
                    {
                        player[p].cardInHand.data[j] -= 2;
                    }

                }
            }

            int minf = -1;//最小牌minf
                          //四同 三同 对子 单个 找最小的牌，都要比其他玩家的所有牌大
            for (int k = 4; k > 0; k--)
            {
                minf = -1;
                for (int i = 0; i < 54; i++)
                {
                    if (firstCard.data[i] == k)
                        if (minf == -1) minf = i;
                        else
                            minf = biggerThan(minf, i, mainColor, mainNumber) ? i : minf;
                    //与其他玩家的牌进行比较
                    for (int p = 0; p < 4; i++)
                        if (p != thisplayer)
                            for (int j = 0; j < 54; j++)
                                if (player[p].cardInHand.data[j] == k && biggerThan(j, minf, mainColor, mainNumber))
                                    return false;
                }

            }
            return true;
        }

        /// <summary>
        /// 判断出牌是否有效
        /// </summary>
        /// <param name="currentPlayer">当前玩家</param>
        /// <param name="winner">当前圈出牌最大玩家</param>
        /// <param name="player">四位玩家</param>
        /// <param name="firstCard">当前圈首位出牌牌面</param>
        /// <param name="maxCard">当前圈最大牌面</param>
        /// <param name="playCard">出的牌组</param>
        /// <returns></returns>
        public
          Judgement canPlay(CardList firstCard, RulePlayer[] player, int thisplayer)
        {
            cardComb fc = new cardComb(firstCard, mainNumber, mainColor);

            if (!fc.valid)
            {
                return new Judgement("出牌不合法", false);
            }
            if (!fc.thrown)
                return new Judgement("正常出牌", true);
            else //甩牌咯
            {
                if (canThrow(firstCard, player, thisplayer))
                    return new Judgement("可以甩牌", true);
                else
                    return new Judgement("甩牌失败", false);
            }
        }

        /// <summary>
        /// 牌大小比较
        /// </summary>
        /// <param name="firstCard">首家出牌</param>
        /// <param name="playCard">当前玩家出牌</param>
        /// <param name="handCard">当前玩家手牌</param>
        /// <returns>0 代表不可出，1 代表可出不压制，2 代表可出压制</returns>
        public
          int orderCompare(CardList firstCard, CardList playCard, CardList handCard)
        {
            cardComb fc = new cardComb(firstCard, mainNumber, mainColor),
                pc = new cardComb(playCard, mainNumber, mainColor);

            if (fc.Count != pc.Count) // 总牌数不同肯定拒绝
                return 0;
            int state = 2;
            int firstColor = fc.thisColor;
            int firstSame = fc.thisSame;
            int firstType = fc.thisType;

            //牌数不够（不是没有）该花色牌全部得出，且必输？？？？？？？？？？

            int hcount = 0;
            int pcount = 0;
            if (mainColor == 4)
            {
                if (firstColor == 4)
                {
                    int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                    for (int i = 0; i < 6; i++)
                    {
                        hcount += handCard.data[l[i]];
                        pcount += playCard.data[l[i]];
                    }
                }
                else
                {
                    for (int i = 0; i < 13; i++)
                        if (i != mainNumber)
                        {
                            hcount += handCard.data[i + firstColor * 13];
                            pcount += playCard.data[i + firstColor * 13];
                        }

                }
            }
            else
            {
                if (firstColor == mainColor)
                {
                    int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                    for (int i = 0; i < 6; i++)
                    {
                        hcount += handCard.data[l[i]];
                        pcount += playCard.data[l[i]];
                    }
                    for (int i = 0; i < 13; i++)
                    {
                        if (i != mainNumber)
                        {
                            hcount += handCard.data[i + mainColor * 13];
                            pcount += playCard.data[i + mainColor * 13];
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 13; i++)
                        if (i != mainNumber)
                        {
                            hcount += handCard.data[i + firstColor * 13];
                            pcount += playCard.data[i + firstColor * 13];
                        }
                }
            }
            if (hcount < fc.Count)
            {
                if (hcount != pcount)
                    return 0;
                else
                    return 1;
            }

            if (fc.thrown)
            {
                //找出所有最长拖拉机
                List<int> maxtractor = new List<int>(); //firstCard中的拖拉机
                List<int> htractor = new List<int>();   //handCard中的对应拖拉机
                List<int> ptractor = new List<int>();   //playCard中的对应拖拉机
                int maxlength;  //handCard中最大但不大于firstCard的拖拉机长度
                int maxorder;   //maxorder同

                //找出firstCard所有拖拉机
                while (true)
                {
                    maxtractor.Clear();
                    htractor.Clear();
                    ptractor.Clear();
                    maxlength = -1;

                    //找出firstCard中最长拖拉机
                    maxtractor = longtractor(firstCard, fc.thisColor, mainColor, mainNumber);
                    int length = maxtractor.Count();
                    //若为拖拉机，则找handCard和playCard中的对应拖拉机
                    if (length > 1)
                    {
                        //首位出牌是主花色或当前出牌不是主花色时才需要考虑强制出牌
                        if (pc.thisColor != mainColor || fc.thisColor == mainColor)
                        {
                            //找出handCard中的playCard花色的最长拖拉机
                            maxlength = length;
                            maxorder = 4;
                            while (true)
                            {
                                htractor = findtractor(handCard, maxlength, pc.thisColor, mainColor, mainNumber);
                                //htractor为空，继续找拖拉机
                                if (!htractor.Any() && maxlength > 1)
                                    maxlength--;
                                //htractor为空，找maxorder同
                                else if (!htractor.Any() && maxlength == 1 && maxorder > 0)
                                {
                                    bool maxflag = false; //记录是否有maxorder同

                                    if (mainColor == 4)  //无主
                                    {
                                        if (pc.thisColor == mainColor)
                                        {
                                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                            for (int i = 0; i < 6; i++)
                                                if (handCard.data[l[i]] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[l[i]] -= maxorder;
                                                    break;
                                                }
                                        }
                                        else
                                        {
                                            for (int i = 0; i < 13; i++)
                                                if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                    break;
                                                }
                                        }
                                    }
                                    else//有主
                                    {
                                        if (pc.thisColor == mainColor)
                                        {
                                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                            for (int i = 0; i < 6; i++)
                                                if (handCard.data[l[i]] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[l[i]] -= maxorder;
                                                    break;
                                                }
                                            for (int i = 0; i < 13; i++)
                                                if (i != mainNumber && handCard.data[i + mainColor * 13] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[i + mainColor * 13] -= maxorder;
                                                    break;
                                                }
                                        }
                                        else
                                        {
                                            for (int i = 0; i < 13; i++)
                                                if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                                {
                                                    maxflag = true;
                                                    handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                    break;
                                                }
                                        }
                                    }
                                    //handCard中有maxorder同则跳出
                                    if (maxflag == true)
                                        break;
                                    maxorder--;
                                }

                                else
                                    break;
                            }
                            //handCard中存在拖拉机
                            if (maxlength > 1)
                            {
                                ptractor = findtractor(playCard, maxlength, pc.thisColor, mainColor, mainNumber);
                                //没有做到强制出牌，不合法
                                if (!ptractor.Any())
                                    return 0;
                            }
                            //handCard中没有拖拉机，找maxorder同
                            else
                            {
                                bool pflag = false; //用于记录playCard中是否有maxorder同
                                                    //playCard中找该花色的最大同
                                for (int i = 0; i < 54; i++)
                                {
                                    if (playCard.data[i] >= maxorder)
                                    {
                                        pflag = true;
                                        playCard.data[i] -= maxorder;
                                        break;
                                    }
                                }
                                //不满足强制性
                                if (!pflag)
                                    return 0;
                            }

                        }
                        //主牌压制，不需要考虑强制性，一旦整齐则压制
                        else
                        {
                            ptractor = findtractor(playCard, length, pc.thisColor, mainColor, mainNumber);
                            //不能匹配到同样长度的拖拉机，不整齐
                            if (!ptractor.Any())
                                return 1;
                        }
                    }
                    //没有拖拉机则结束循环
                    else
                    {
                        break;

                    }
                    //合法则去掉最长拖拉机后继续循环
                    foreach (int j in maxtractor)
                        firstCard.data[j] -= 2;
                    foreach (int j in ptractor)
                        playCard.data[j] -= 2;
                    foreach (int j in htractor)
                        handCard.data[j] -= 2;

                }



                //四同 三同 对子 单个 

                maxorder = -1;//handCard中playCard花色的最大同
                for (int k = 4; k > 0; k--)
                {
                    bool flag;
                    while (true)
                    {
                        flag = false;
                        //找firstCard中有没有该牌
                        for (int i = 0; i < 54; i++)
                            if (firstCard.data[i] == k)
                            {
                                flag = true;
                                firstCard.data[i] -= k;
                                break;
                            }
                        if (flag == false)
                            break;

                        //考虑强制性
                        if (pc.thisColor != mainColor || fc.thisColor == mainColor)
                        {
                            //handCard中playCard花色的最大同
                            maxorder = k;
                            for (maxorder = k; maxorder > 0; maxorder--)
                            {
                                bool maxflag = false; //记录是否有maxorder同
                                                      //无主
                                if (mainColor == 4)
                                {
                                    if (pc.thisColor == mainColor)
                                    {
                                        int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                        for (int i = 0; i < 6; i++)
                                            if (handCard.data[l[i]] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[l[i]] -= maxorder;
                                                break;
                                            }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < 13; i++)
                                            if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                break;
                                            }
                                    }
                                }
                                else//有主
                                {
                                    if (pc.thisColor == mainColor)
                                    {
                                        int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 };
                                        for (int i = 0; i < 6; i++)
                                            if (handCard.data[l[i]] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[l[i]] -= maxorder;
                                                break;
                                            }
                                        for (int i = 0; i < 13; i++)
                                            if (i != mainNumber && handCard.data[i + mainColor * 13] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[i + mainColor * 13] -= maxorder;
                                                break;
                                            }
                                    }
                                    else
                                    {
                                        for (int i = 0; i < 13; i++)
                                            if (i != mainNumber && handCard.data[i + pc.thisColor * 13] >= maxorder)
                                            {
                                                maxflag = true;
                                                handCard.data[i + pc.thisColor * 13] -= maxorder;
                                                break;
                                            }
                                    }
                                }
                                //handCard中有maxorder同则跳出
                                if (maxflag == true)
                                    break;

                            }

                            bool pflag = false; //用于记录playCard中是否有maxorder同
                                                //playCard中找该花色的最大同
                            for (int i = 0; i < 54; i++)
                            {
                                if (playCard.data[i] >= maxorder)
                                {
                                    pflag = true;
                                    playCard.data[i] -= maxorder;
                                    break;
                                }
                            }
                            //不满足强制性
                            if (!pflag)
                                return 0;

                        }
                        else//不需要考虑强制性，不整齐就不压制
                        {
                            bool pflag = false;
                            for (int i = 0; i < 54; i++)
                            {
                                if (playCard.data[i] == k)
                                {
                                    pflag = true;
                                    playCard.data[i] -= k;
                                    break;
                                }
                            }
                            if (pflag == false)
                                return 1;
                        }
                    }
                    //说明firstCard中没有k同
                    if (flag == false)
                        continue;
                    //考虑强制性，能走到这一步说明合理，但不压制

                }
                //考虑强制性，能走到这一步说明合法，但不压制
                if (pc.thisColor != mainColor || fc.thisColor == mainColor)
                    return 1;
                else//不考虑强制性，能走到这一步说明整齐，压制
                    return 2;
            }

            else // 首家非甩牌
            {
                // 四同 三同 对子 单张
                if (firstType <= 4 && firstType >= 1)
                {
                    int maxFirst = -1, maxPlay = -1;

                    for (int i = 0; i < 54; i++)
                        if (playCard.data[i] == firstSame && biggerThan(i, maxPlay, mainColor, mainNumber))
                            maxPlay = i;

                    //强制出牌但未出者return0；出主牌压制者retur 2；手牌中没有与firstcard完全整齐牌state=1；更新firstcard中的最大牌
                    for (int i = 0; i < 54; i++)
                    {
                        if (firstCard.data[i] == firstSame)
                        {
                            // 在playCard与handCard中寻找匹配
                            if (mainColor == 4)
                            {
                                if (firstColor == 4)
                                {
                                    int sameLevel = -1;//手牌中能匹配的最大同
                                                       // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                    {
                                        if (sameLevel == -1)
                                            for (int k = 0; k < 4; k++)
                                                if (handCard.data[k * 13 + mainNumber] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k * 13 + mainNumber] -= j;
                                                    break;
                                                }
                                        if (sameLevel == -1)
                                            for (int k = 52; k <= 53; k++)
                                                if (handCard.data[k] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                    }
                                    // 若handCard中没有相应
                                    if (sameLevel == -1)
                                    {
                                        state = 1;
                                        continue;
                                    }

                                    // 在playCard中寻找同样匹配级别的，有最大同存在必须出，否则非法
                                    bool flag = false;
                                    for (int k = 0; k < 4; k++)
                                        if (playCard.data[k * 13 + mainNumber] >= sameLevel)
                                        {
                                            playCard.data[k * 13 + mainNumber] -= sameLevel;
                                            flag = true;
                                            break;
                                        }
                                    if (!flag)
                                        for (int k = 52; k <= 53; k++)
                                            if (playCard.data[k] >= sameLevel)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                flag = true;
                                                break;
                                            }
                                    if (flag)
                                    {
                                        if (sameLevel != firstSame)
                                            state = 1;
                                    }
                                    else
                                        return 0;
                                }
                                else // 首家出副牌
                                {
                                    int sameLevel = -1;
                                    // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                        if (sameLevel == -1)
                                            for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                                if (handCard.data[k] >= j && k != mainNumber)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                    // 若handCard中没有相应 寻求主牌压制
                                    if (sameLevel == -1)
                                    {
                                        if (!pc.thrown && pc.thisColor == mainColor && pc.thisType == fc.thisType)
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    else // 在playCard中寻找同样匹配级别的
                                    {
                                        bool flag = false;
                                        for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                            if (playCard.data[k] >= sameLevel && k != mainNumber)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                flag = true;
                                                break;
                                            }
                                        if (flag)
                                        {
                                            if (sameLevel != firstSame)
                                                state = 1;
                                        }
                                        else
                                            return 0;
                                    }
                                }
                            }
                            else // 打有主
                            {
                                if (firstColor == mainColor)
                                {
                                    int sameLevel = -1;
                                    // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                    {
                                        if (sameLevel == -1)
                                            for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                                if (handCard.data[k] >= j && k != mainNumber)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                        if (sameLevel == -1)
                                            for (int k = 0; k < 4; k++)
                                                if (handCard.data[k * 13 + mainNumber] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k * 13 + mainNumber] -= j;
                                                    break;
                                                }
                                        if (sameLevel == -1)
                                            for (int k = 52; k <= 53; k++)
                                                if (handCard.data[k] >= j)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }
                                    }

                                    // 若handCard中没有相应
                                    if (sameLevel == -1)
                                    {
                                        state = 1;
                                        continue;
                                    }

                                    // 在playCard中寻找同样匹配级别的
                                    bool flag = false;
                                    for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                        if (playCard.data[k] >= sameLevel && k != mainNumber)
                                        {
                                            playCard.data[k] -= sameLevel;
                                            flag = true;
                                            break;
                                        }
                                    if (!flag)
                                        for (int k = 0; k < 4; k++)
                                            if (playCard.data[k * 13 + mainNumber] >= sameLevel)
                                            {
                                                playCard.data[k * 13 + mainNumber] -= sameLevel;
                                                flag = true;
                                                break;
                                            }
                                    if (!flag)
                                        for (int k = 52; k <= 53; k++)
                                            if (playCard.data[k] >= sameLevel)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                flag = true;
                                                break;
                                            }
                                    if (flag)
                                    {
                                        if (sameLevel != firstSame)
                                            state = 1;
                                    }
                                    else
                                        return 0;
                                }
                                else // 首家出副牌
                                {
                                    int sameLevel = -1;
                                    // 先看handCard 确定匹配级别
                                    for (int j = firstSame; j >= 1; j--) // 降阶
                                        if (sameLevel == -1)
                                            for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                                if (handCard.data[k] >= j && k != mainNumber)
                                                {
                                                    sameLevel = j;
                                                    handCard.data[k] -= j;
                                                    break;
                                                }

                                    // 若handCard中没有相应 寻求主牌压制   
                                    if (sameLevel == -1)
                                    {
                                        if (!pc.thrown && pc.thisColor == mainColor && pc.thisType == fc.thisType)
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    else // 在playCard中寻找同样匹配级别的
                                    {
                                        bool flag = false;
                                        for (int k = firstColor * 13; k < (firstColor + 1) * 13; k++)
                                            if (playCard.data[k] >= sameLevel && k != mainNumber)
                                            {
                                                playCard.data[k] -= sameLevel;
                                                flag = true;
                                                break;
                                            }
                                        if (flag)
                                        {
                                            if (sameLevel != firstSame)
                                                state = 1;
                                        }
                                        else
                                            return 0;
                                    }
                                }
                            }

                            // 更新最大
                            if (biggerThan(i, maxFirst, mainColor, mainNumber))
                                maxFirst = i;
                        }
                    }

                    //playcard最大牌比firstcard最大牌小，state=1
                    if (biggerThan(maxFirst, maxPlay, mainColor, mainNumber))
                    {
                        state = 1;
                    }
                }

                // 拖拉机
                if (firstType == 0)
                {
                    int firstLength = fc.data.Count;
                    int maxh = 0, maxhmax = -1; // Hand 手中最长拖拉机长度maxh与起始点maxhmax
                    int maxp = 0, maxpmax = -1; // Play 打出的最长拖拉机长度maxp与起始点maxpmax
                    int maxFirst = -1;

                    for (int i = 0; i < 54; i++)
                        if (firstCard.data[i] >= 2 && biggerThan(i, maxFirst, mainColor, mainNumber))
                            maxFirst = i;

                    // 在手牌中寻找
                    if (mainColor == 4)//无主
                    {
                        if (firstColor == mainColor)//主牌
                        {
                            // 在handCard中寻找最大匹配长度
                            int ctn = 0;                                                                                        // ctn连续计数
                            int[] l = new int[] { mainNumber, 13 + mainNumber, 2 * 13 + mainNumber, 3 * 13 + mainNumber, 52, 53 }; // 要考虑的牌
                            for (int i = 0; i < 4; i++)
                            {
                                if (handCard.data[l[i]] >= 2)
                                    ctn++;
                                if (ctn > maxh)
                                {
                                    maxh = ctn;
                                    maxhmax = l[i];
                                }
                            }
                            for (int i = 4; i < l.Length; i++)
                            {
                                if (handCard.data[l[i]] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxh)
                                {
                                    maxh = ctn;
                                    maxhmax = l[i];
                                }
                            }

                            // 若现有拖拉机长度超过首家出牌长度
                            if (maxh > fc.Count / 2)
                                maxh = fc.Count / 2;

                            // 在playCard寻找相应匹配
                            ctn = 0; // ctn连续计数
                            for (int i = 0; i < 4; i++)
                            {
                                if (playCard.data[l[i]] >= 2)
                                    ctn++;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = l[i];
                                }
                            }
                            for (int i = 4; i < l.Length; i++)
                            {
                                if (playCard.data[l[i]] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = l[i];
                                }
                            }

                            //强制出牌
                            if (maxp == maxh) // 拖拉机配足了长度
                            {
                                // 不需垫牌
                                if (maxp == fc.Count / 2)
                                {
                                    if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                        return 2;
                                    else
                                        return 1;
                                }
                                // 需垫牌，判断垫牌是否合法
                                int tmp1 = 0, tmp2 = 0;
                                for (int i = 0; i < l.Length; i++)
                                {
                                    tmp1 += handCard.data[l[i]];
                                    tmp2 += playCard.data[l[i]];
                                }
                                if (tmp1 > fc.Count)
                                    tmp1 = fc.Count;
                                if (tmp1 == tmp2)
                                    return 1;
                                else
                                    return 0;
                            }
                            else
                                return 0; // 拖拉机没配足长度
                        }
                        else // 首家出副牌
                        {
                            // 检查手牌的副牌段
                            int ctn = 0;
                            bool viceOccur = false;
                            for (int i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (handCard.data[i + firstColor * 13] >= 2)
                                    {
                                        if (handCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        ctn++;
                                    }
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = i + firstColor * 13;
                                    }
                                }

                            if (viceOccur) // 手牌有副牌
                            {
                                if (maxh >= fc.Count / 2)
                                    maxh = fc.Count / 2;

                                // 检查出牌的副牌段
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (playCard.data[i + firstColor * 13] >= 2)
                                        {
                                            if (playCard.data[i + firstColor * 13] > 0)
                                                viceOccur = true;
                                            ctn++;
                                        }
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i + firstColor * 13;
                                        }
                                    }

                                if (maxp == maxh) // 是否配足拖拉机
                                {
                                    if (maxp == fc.Count / 2) // 不用垫牌
                                    {
                                        if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    // 垫牌是否垫足
                                    int tmp1 = 0, tmp2 = 0;
                                    for (int i = 0; i < 13; i++)
                                        if (i != mainNumber)
                                        {
                                            tmp1 += handCard.data[i];
                                            tmp2 += playCard.data[i];
                                        }
                                    if (tmp1 >= fc.Count)
                                        tmp1 = fc.Count;
                                    if (tmp1 == tmp2)
                                        return 1;
                                    else
                                        return 0;
                                }
                                else
                                    return 0;
                            }
                            else // 手牌没有副牌，看是否用主牌压制，否则返回1
                            {
                                ctn = 0; // ctn连续计数
                                for (int i = 0; i < 4; i++)
                                {
                                    if (playCard.data[i * 13 + mainNumber] >= 2)
                                        ctn++;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i * 13 + mainNumber;
                                    }
                                }
                                for (int i = 52; i < 53; i++)
                                {
                                    if (playCard.data[i] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i;
                                    }
                                }

                                if (maxp == fc.Count / 2)
                                    return 2;
                                else
                                    return 1;
                            }
                        }
                    }
                    else // 打有主
                    {
                        if (firstColor == mainColor)
                        {
                            // 在handCard中寻找最大匹配长度
                            int i;
                            int ctn = 0; // ctn连续计数
                            for (i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (handCard.data[i + mainColor * 13] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = i + mainColor * 13;
                                    }
                                }
                            bool flag = true;
                            for (i = 0; i < 4; i++)
                                if (handCard.data[i * mainColor + mainNumber] >= 2)
                                {
                                    ctn++;
                                    flag = false;
                                    break;
                                }
                            if (flag)
                                ctn = 0;
                            if (ctn > maxh)
                            {
                                maxh = ctn;
                                maxhmax = i * mainColor + mainNumber;
                            }
                            for (i = 52; i < 54; i++)
                            {
                                if (handCard.data[i] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxh)
                                {
                                    maxh = ctn;
                                    maxhmax = i;
                                }
                            }

                            // 在playCard寻找相应匹配
                            ctn = 0; // ctn连续计数
                            for (i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (playCard.data[i + mainColor * 13] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i + mainColor * 13;
                                    }
                                }
                            flag = true;
                            for (i = 0; i < 4; i++)
                                if (playCard.data[i * mainColor + mainNumber] >= 2)
                                {
                                    ctn++;
                                    flag = false;
                                    break;
                                }
                            if (flag)
                                ctn = 0;
                            if (ctn > maxp)
                            {
                                maxp = ctn;
                                maxpmax = i * mainColor + mainNumber;
                            }
                            for (i = 52; i < 54; i++)
                            {
                                if (playCard.data[i] >= 2)
                                    ctn++;
                                else
                                    ctn = 0;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = i;
                                }
                            }

                            // 若现有拖拉机长度超过首家出牌长度
                            if (maxh > fc.Count / 2)
                                maxh = fc.Count / 2;

                            if (maxp == maxh) // 拖拉机配足了长度
                            {
                                // 不需垫牌
                                if (maxp == fc.Count / 2)
                                {
                                    if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                        return 2;
                                    else
                                        return 1;
                                }
                                // 判断垫牌是否合法
                                int tmp1 = 0, tmp2 = 0;
                                for (i = 0; i < 13; i++)
                                {
                                    tmp1 += handCard.data[i + mainColor * 13];
                                    tmp2 += playCard.data[i + mainColor * 13];
                                }
                                for (i = 0; i < 4; i++)
                                {
                                    tmp1 += handCard.data[i * mainColor + mainNumber];
                                    tmp2 += playCard.data[i * mainColor + mainNumber];
                                }
                                for (i = 52; i < 54; i++)
                                {
                                    tmp1 += handCard.data[i];
                                    tmp2 += playCard.data[i];
                                }
                                if (tmp1 > fc.Count)
                                    tmp1 = fc.Count;
                                if (tmp1 == tmp2)
                                    return 1;
                                else
                                    return 0;
                            }
                            else
                                return 0; // 拖拉机没配足长度
                        }
                        else // 首家出副牌
                        {
                            // 检查手牌的副牌段
                            int ctn = 0;
                            bool viceOccur = false;
                            for (int i = 0; i < 13; i++)
                                if (i != mainNumber)
                                {
                                    if (handCard.data[i + firstColor * 13] >= 2)
                                    {
                                        if (handCard.data[i + firstColor * 13] > 0)
                                            viceOccur = true;
                                        ctn++;
                                    }
                                    else
                                        ctn = 0;
                                    if (ctn > maxh)
                                    {
                                        maxh = ctn;
                                        maxhmax = i + firstColor * 13;
                                    }
                                }

                            if (viceOccur) // 手牌有副牌
                            {
                                if (maxh >= fc.Count / 2)
                                    maxh = fc.Count / 2;

                                // 检查出牌的副牌段
                                for (int i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (playCard.data[i + firstColor * 13] >= 2)
                                        {
                                            if (playCard.data[i + firstColor * 13] > 0)
                                                viceOccur = true;
                                            ctn++;
                                        }
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i + firstColor * 13;
                                        }
                                    }

                                if (maxp == maxh) // 是否配足拖拉机
                                {
                                    if (maxp == fc.Count / 2) // 不用垫牌
                                    {
                                        if (biggerThan(maxpmax, maxFirst, mainColor, mainNumber))
                                            return 2;
                                        else
                                            return 1;
                                    }
                                    // 垫牌是否垫足
                                    int tmp1 = 0, tmp2 = 0;
                                    for (int i = 0; i < 13; i++)
                                        if (i != mainNumber)
                                        {
                                            tmp1 += handCard.data[i];
                                            tmp2 += playCard.data[i];
                                        }
                                    if (tmp1 >= fc.Count)
                                        tmp1 = fc.Count;
                                    if (tmp1 == tmp2)
                                        return 1;
                                    else
                                        return 0;
                                }
                                else
                                    return 0;
                            }
                            else // 手牌没有副牌，看是否用主牌压制，否则返回1
                            {
                                ctn = 0; // ctn连续计数
                                int i;
                                for (i = 0; i < 13; i++)
                                    if (i != mainNumber)
                                    {
                                        if (playCard.data[i + mainColor * 13] >= 2)
                                            ctn++;
                                        else
                                            ctn = 0;
                                        if (ctn > maxp)
                                        {
                                            maxp = ctn;
                                            maxpmax = i;
                                        }
                                    }
                                bool flag = true;
                                for (i = 0; i < 4; i++)
                                    if (playCard.data[i * mainColor + mainNumber] >= 2)
                                    {
                                        ctn++;
                                        flag = false;
                                        break;
                                    }
                                if (flag)
                                    ctn = 0;
                                if (ctn > maxp)
                                {
                                    maxp = ctn;
                                    maxpmax = i;
                                }
                                for (i = 52; i < 54; i++)
                                {
                                    if (playCard.data[i] >= 2)
                                        ctn++;
                                    else
                                        ctn = 0;
                                    if (ctn > maxp)
                                    {
                                        maxp = ctn;
                                        maxpmax = i;
                                    }
                                }

                                if (maxp == fc.Count / 2)
                                    return 2;
                                else
                                    return 1;
                            }
                        }
                    }
                }
                // 只有当跟牌长度相等 且最大牌比首家大 state维持为2 否则state为1
                return state;

            }

        }

        /// <summary>
        /// 判断跟牌是否有效
        /// </summary>
        /// <param name="currentPlayer">当前玩家</param>
        /// <param name="winner">当前圈出牌最大玩家</param>
        /// <param name="player">四位玩家</param>
        /// <param name="firstCard">当前圈首位出牌牌面</param>
        /// <param name="maxCard">当前圈最大牌面</param>
        /// <param name="playCard">出的牌组</param>
        /// <returns></returns>
        public
           Judgement canPlay(CardList firstCard, CardList playCard, CardList handCard)
        {
            switch (orderCompare(firstCard, playCard, handCard))
            {
                case 0:
                    return new Judgement("有得出不出", false);
                case 1:
                    return new Judgement("不毙", true);
                case 2:
                    return new Judgement("毙", true);
            }
            return new Judgement("占位", true);
        }

        // 判断出牌合法性(对战阶段)
        public Judgement IsLegalDeal(PlayerInfo[] m_player, Card[] cards)
        {
            // 规则组：出2张牌会卡住，可能是死循环
            //RulePlayer[] tmp = PlayerToRulePlayer(m_player);
            //CardList playCard = CardArrayToCardList(cards);
            //CardList firstCard = CardListToCardList(handOutCards[firstHomePlayerId]);
            //if (currentPlayerId == firstHomePlayerId)
            //    return canPlay(playCard, tmp, currentPlayerId);
            //else
            //    return canPlay(firstCard, playCard, tmp[currentPlayerId].cardInHand);

            // 暂且无规则
            if (dealRequiredLength <= 0)
            {
                return new Judgement("", cards.Length > 0);
            }
            else
            {
                return new Judgement("", cards.Length == dealRequiredLength);
            }


        }

        public void SetCurrentPlayerId(int id)
        {
            m_currentPlayerId = id;
        }

        public void ClearShowCards()
        {
            for (int i = 0; i < playerNumber; i++)
            {
                showCards[i].Clear();
            }
        }

        public void ClearHandOutCards()
        {
            for (int i = 0; i < playerNumber; i++)
            {
                handOutCards[i].Clear();
            }
        }

        // TODO：
        //public void IncrementCurrentPlayerId()
        //{
        //    m_currentPlayerId = (m_currentPlayerId + 1) % playerNumber;
        //}
        public void UpdateNextPlayer()
        {
            // 如果所有玩家都出过牌
            if (handOutPlayerCount == 0)
            {
                // 设置下一出牌玩家为首家
                m_currentPlayerId = firstHomePlayerId;
            }
            else// 如果还有玩家没有出牌
            {
                // 下一玩家出牌
                m_currentPlayerId = (m_currentPlayerId + 1) % playerNumber;
            }
        }

        // 判断是否所有玩家的手牌为空
        public bool AllPlayersHandEmpty()
        {
            // 对于每一个玩家
            for (int i = 0; i < playersHandCard.Length; i++)
            {
                // 如果他还有手牌
                if (playersHandCard[i].Count > 0)
                {
                    return false;
                }
            }
            return true;
        }

        // 判断是否不可能有更高出价者(抢底阶段)
        public bool NoHigherBid()
        {
            bool hasHigerBid = false;
            for (int i = 0; i < playerNumber; i++)
            {
                bool[] legalBidColors = GetLegalBidColors(i);
                for (int j = 0; j < legalBidColors.Length; j++)
                {
                    hasHigerBid |= legalBidColors[j];
                }
            }
            // 当没有更高筹码，或者只有一个台上方玩家时，认为不需要再进行抢底
            return !hasHigerBid || upperPlayersId.Length == 1;
        }

        // 炒底阶段：判断是否不可能有更高筹码者
        //public bool NoHigerFry()
        //{
        //    // 测试：当要求亮牌不比最大亮牌数小时，判定已经没有更高筹码了
        //    return fryCardLowerBound >= m_fryCardLimit;
        //}

        //// 判断是否不可能有更高出价者(炒底阶段)
        //public bool NoHigherFry()
        //{
        //    return fryCardLowerBound >= m_fryCardLimit;
        //}
        //// 是否所有玩家跳过炒底阶段
        //public bool AllSkipFry()
        //{
        //    return true;
        //}
        // 判断是否结束炒底
        public bool FryEnd()
        {
            // 测试：当要求亮牌不比最大亮牌数小时，判定已经没有更高筹码了
            bool noHigherFry = fryCardLowerBound >= m_fryCardLimit;
            bool allSkipFry = skipFryCount >= playerNumber;
            if (noHigherFry)
            {
                Console.WriteLine("不可能有更高出价者, 炒底结束");
            }
            if (allSkipFry)
            {
                Console.WriteLine("所有玩家跳过炒牌, 炒底结束");
            }
            return noHigherFry || allSkipFry;
        }

        // 更新台上方玩家
        public void UpdateUpperPlayers()
        {
            if (m_round == 1)    // 如果是首盘
            {
                // 所有玩家都是台上方
                m_upperPlayersId = new int[4] { 0, 1, 2, 3 };
            }
            else// 如果不是首盘
            {

            }
        }

        // 更新首家
        public void UpdateFirstHome()
        {
            firstHomePlayerId = 0;
        }

        // 炒底阶段，帮指定玩家代理亮牌
        public Card[] AutoAddShowCard(int playerId)
        {
            // 随意选择合法的亮牌
            // 测试：牌数比最低筹码大即为合法亮牌
            int n = fryCardLowerBound + 1 - showCards[playerId].Count;
            Card[] addShowCards = new Card[n];
            // 从手牌中选取
            Array.Copy(playersHandCard[playerId].ToArray(), addShowCards, n);
            return addShowCards;
        }
        // 炒底阶段，帮指定玩家代理埋底
        // 测试：埋 8 张牌就是合法的
        public Card[] AutoBuryCard(int playerId)
        {
            // 随意选择合法的埋牌
            Card[] buryCards = new Card[bottomCardNumber];
            // 从手牌中选取
            Array.Copy(playersHandCard[playerId].ToArray(), buryCards, bottomCardNumber);
            return buryCards;
        }

        // 对战阶段，帮指定玩家代理出牌
        public Card[] AutoHandOut(int playerId)
        {
            Card[] handOutCards;
            if (playersHandCard[playerId].Count > 0)
            {
                // 测试：选择指定长度的牌
                if (dealRequiredLength <= 0)
                {
                    handOutCards = new Card[1];
                    Array.Copy(playersHandCard[playerId].ToArray(), handOutCards, 1);
                }
                else
                {
                    handOutCards = new Card[dealRequiredLength];
                    Array.Copy(playersHandCard[playerId].ToArray(), handOutCards, dealRequiredLength);
                }
                return handOutCards;
            }
            else
            {
                return new Card[0];
            }
        }

    }

}
