using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameUtility
{
    public class Dealer
    {
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
        public void GenerateSignCard()
        {
            // 测试：我先随便编一个
            signCard = new Card(Card.Suit.Club, 0);
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

        // 判断出牌合法性(对战阶段)
        public Judgement IsLegalDeal(Card[] cards)
        {
            string message;
            bool isValid;
            // 如果出牌要求牌数为负
            if (dealRequiredLength < 0)
            {
                // 则按规定, 表明是第一次出牌, 对出牌长度没有限制，只要求一定要出牌
                isValid = cards.Length > 0;
                message = "请出牌";
            }
            // 如果出牌长度不符合要求
            else if (cards.Length != dealRequiredLength)
            {
                // 非法出牌
                isValid = false;
                message = "出牌长度不符合要求";
            }
            else
            {
                // 测试：其他的都是合法出牌
                isValid = true;
                message = "合法出牌";
            }
            return new Judgement(message, isValid);
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
