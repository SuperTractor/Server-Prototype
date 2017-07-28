using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GameUtility;
using Networking;
using System.Diagnostics;

namespace server_0._0._1
{
    class Program
    {
        // 服务的端口
        static int m_port = 8885;
        // 服务器用来监听连接的 socket
        static Socket m_socket;
        // 服务器的 IP 地址
        static IPAddress m_ip;
        //static string m_ipStr = "120.24.217.239";
        static string m_ipStr = "172.21.186.34";

        // 开始游戏要求的玩家个数
        // 当前在线的玩家数目
        static int m_playersCount = 0;
        // 玩家列表
        static Player[] m_players;
        // 荷官
        static Dealer m_dealer;
        // 出牌要求的牌数
        // 游戏状态机
        static GameStateMachine m_gameStateMachine;

        // 用于辅助模拟摸牌效果，暂且放置手牌
        static List<Card>[] m_tempHandCards;

        // 当前应该显示牌数
        static int m_handCardShowNumber;
        // 抢底阶段显示手牌的延迟(毫秒)
        static int m_touchCardDelay = 10;
        // 用于辅助模拟摸牌效果的计时器
        static Stopwatch m_touchCardStopwatch;
        // 用于最后抢底阶段的计时器
        static Stopwatch m_lastBidStopwatch;
        // 最后抢底阶段有的思考时间（毫秒）
        static int m_lastBidDelay = 5000;
        // 抢底阶段：庄家埋底思考时间
        static int m_bidBuryBottomDelay = 30000;
        // 庄家埋底计时器
        static Stopwatch m_bidBuryStopwatch;

        // 炒底新增亮牌数组
        static Card[] m_addFryCards;
        // 亮牌思考时间（毫秒）
        static int m_showCardDelay = 10000;
        // 埋底思考时间（毫秒）
        static int m_buryBottomDelay = 30000;
        // 炒底阶段，玩家爱选择埋下的底牌
        static Card[] m_buryCards;
        // 炒底阶段，玩家是否跟牌
        static bool m_isFollow;
        // 亮牌的计时器
        static Stopwatch m_showCardStopwatch;
        // 埋底的计时器
        static Stopwatch m_buryCardStopwatch;

        // 寻友阶段计时器
        static Stopwatch m_findFriendStopwatch;
        // 寻友阶段庄家思考时间（毫秒）
        static int m_findFriendDelay = 10000;
        // 寻友结束后，如果庄家有找朋友，则停留几秒让所有玩家看清楚信号牌
        static int m_findFriendLingerDelay = 2000;
        // 停留计时器
        static Stopwatch m_findFriendLingerStopwatch;

        // 用于实现最后一个出牌后延迟清空牌桌的计时器
        static Stopwatch m_clearPlayCardStopwatch;
        // 4 个玩家都出牌后的延迟（毫秒）
        static int m_clearPlayCardDelay = 1500;
        // 标志已经完成一次延时
        static bool m_doneClearPlayCardDelay;

        // 出牌数组
        static Card[] m_dealCards;

        // 出牌计时器
        static Stopwatch m_handOutStopwatch;
        // 玩家拥有的出牌思考时间(毫秒)
        static int m_handOutTimeLimit = 30000;
        // 上一次检查时的出牌玩家 id
        //static int m_lastCheckHandOutPlayerId;
        // 标志可以开始出牌思考计时
        static bool m_isOkCountDown;

        static void Initialize()
        {
            // 设置命令行的编码为 utf8
            Console.OutputEncoding = Encoding.Unicode;

            m_players = new Player[Dealer.playerNumber];
            m_ip = IPAddress.Parse(m_ipStr);
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Bind(new IPEndPoint(m_ip, m_port));
            m_socket.Listen(Dealer.playerNumber);
            Console.WriteLine("启动监听{0}成功", m_socket.LocalEndPoint.ToString());
            //主线程等待客户端连接
            WaitClient();

            m_dealer = new Dealer();
            m_gameStateMachine = new GameStateMachine();
            m_gameStateMachine.Update(GameStateMachine.State.Deal);
            // 新建摸牌效果辅助计时器
            m_touchCardStopwatch = new Stopwatch();
            // 新建炒底阶段亮牌计时器
            m_showCardStopwatch = new Stopwatch();
            // 新建炒底阶段埋底计时器
            m_buryCardStopwatch = new Stopwatch();
            // 新建延迟清理桌面计时器
            m_clearPlayCardStopwatch = new Stopwatch();
            // 新建出牌计时器
            m_handOutStopwatch = new Stopwatch();
            // 新建最后抢底阶段的计时器
            m_lastBidStopwatch = new Stopwatch();
            // 新建庄家埋底的计时器
            m_bidBuryStopwatch = new Stopwatch();
            // 新建庄家寻友计时器
            m_findFriendStopwatch = new Stopwatch();
            // 新建寻友计时器
            m_findFriendLingerStopwatch = new Stopwatch();
            // 新建暂存手牌数组
            m_tempHandCards = new List<Card>[Dealer.playerNumber];
            //for(int i = 0; i < Dealer.playerNumber; i++)
            //{
            //    m_tempHandCards[i] = new List<Card>();
            //}
        }
        //接收一个连接并为其分配一个线程服务。
        static void WaitClient()
        {
            // 如果还没有满人
            while (m_playersCount < Dealer.playerNumber)
            {
                // 接受来自客户端的连接请求(如果有的话)
                // 此处阻塞, 直到有客户端的连接请求
                // 一旦连接成功, 将创建一个新的 socket 代表此连接
                // 服务器和该客户端将可以通过此 socket 进行交流
                Socket socket = m_socket.Accept();
                // 服务器从客户端读取用户信息
                // 此处阻塞, 直到有客户端发送消息
                // 接受玩家发送过来的姓名
                // 发送当前的玩家计数给该玩家作为 id

                string name = (string)ComServer.Respond(socket, m_playersCount);
                PlayerInfo playerInfo = new PlayerInfo(name, m_playersCount);

                // 记录客户端和服务器之间连接所用的 socket, 以及对应的用户信息
                // 即记录连接信息
                Player player = new Player(playerInfo, socket);

                m_players[m_playersCount++] = player;

                Console.WriteLine(player.playerInfo.name + " 连接到服务器");
            }
            // 向所有玩家返回响应
            for (int i = 0; i < Dealer.playerNumber; i++)
            {
                ComServer.Respond(m_players[i].socket, "房间满人，游戏开始");
            }
        }

        static void DealCards()
        {
            // 洗牌
            m_dealer.Shuffle();
            // 分牌
            m_dealer.Cut();
            Card[] cards = new Card[Dealer.cardInHandInitialNumber];
            for (int i = 0; i < Dealer.playerNumber; i++)
            {
                // 给玩家分发手牌
                Array.Copy(m_dealer.playerCard, i * Dealer.cardInHandInitialNumber, cards, 0, Dealer.cardInHandInitialNumber);
                //m_players[i].playerInfo.cardInHand = new List<Card>(cards);
                m_tempHandCards[i] = new List<Card>(cards);
                // 通过网络将手牌发送给客户端
                // 用 int 数组格式发送
                //Console.WriteLine("向 " + m_players[i].playerInfo.name + " 发送手牌");
                //ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
            }
            // 如果当前是首盘
            if (m_dealer.round == 1)
            {
                // 设置所有玩家的级数为 1
                for (int i = 0; i < Dealer.playerNumber; i++)
                {
                    m_players[i].playerInfo.level = 1;
                }
            }
        }

        static void Deal2Bid()
        {
            // 开始计时
            m_touchCardStopwatch.Restart();

            for (int i = 0; i < Dealer.playerNumber; i++)
            {
                // 向客户端发送当前台上方的 ID
                // 如果只有 1 个台上方, 则跳过摸牌效果; 否则, 才需要模拟摸牌
                ComServer.Respond(m_players[i].socket, m_dealer.upperPlayersId);
                // 发送玩家当前的等级
                ComServer.Respond(m_players[i].socket, m_players[i].playerInfo.level);
            }
            m_handCardShowNumber = 0;
            for (int i = 0; i < m_dealer.currentBidCards.Length; i++)
            {
                m_dealer.currentBidCards[i].Clear();
            }
        }

        // 处理抢底流程
        // 返回是否结束摸牌
        static bool Touch()
        {
            // 如果台上方不止一个
            if (m_dealer.upperPlayersId.Length > 1)
            {
                // 是否所有服务器都已经显示当前要显示的那一张牌，即它们是否已经跟上节奏
                bool isFollowed = true;

                for (int i = 0; i < m_players.Length; i++)
                {
                    // 向 4 个客户端发送当前应该显示的牌数
                    //ComServer.Respond(m_players[i].socket, m_handCardShowNumber);
                    // 向 4 个客户端发送当前玩家的手牌
                    ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));

                    // 检查客户端是否跟上步伐
                    isFollowed &= (bool)ComServer.Respond(m_players[i].socket, "收到");
                    // 向该玩家发送当前可以亮牌的花色
                    ComServer.Respond(m_players[i].socket, m_dealer.GetLegalBidColors(i/*,m_handCardShowNumber*/));

                    // 接收每个玩家的亮牌决定
                    bool isClickClub = (bool)ComServer.Respond(m_players[i].socket, "收到");
                    bool isClickDiamond = (bool)ComServer.Respond(m_players[i].socket, "收到");
                    bool isClickHeart = (bool)ComServer.Respond(m_players[i].socket, "收到");
                    bool isClickSpade = (bool)ComServer.Respond(m_players[i].socket, "收到");

                    // 获取当前玩家需要增加的亮牌数
                    //int bidNeedNumber = m_dealer.BidNeedNumber(i);
                    // 根据现在的情况，更新该玩家的亮牌和手牌
                    if (isClickClub)
                    {
                        m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Club);
                    }
                    else if (isClickDiamond)
                    {
                        m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Diamond);
                    }
                    else if (isClickHeart)
                    {
                        m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Heart);
                    }
                    else if (isClickSpade)
                    {
                        m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Spade);
                    }
                    else// 如果玩家还没有决定亮牌
                    {

                    }
                    // 更新玩家的手牌
                    m_players[i].playerInfo.cardInHand = m_dealer.playersHandCard[i];
                    // 将该玩家的手牌发送到客户端
                    ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
                }
                for (int i = 0; i < m_players.Length; i++)
                {
                    // 将当前亮牌玩家的 ID 和亮牌发送到所有玩家
                    for (int j = 0; j < m_dealer.currentBidCards.Length; j++)
                    {
                        ComServer.Respond(m_players[i].socket, j);
                        ComServer.Respond(m_players[i].socket, Card.ToInt(m_dealer.currentBidCards[j]));
                    }
                }


                // 获取当前应该显示的手牌数
                //int handCardShowNumber = (int)m_touchCardStopwatch.ElapsedMilliseconds / m_touchCardDelay;
                // 如果计时器显示已经超过显示手牌延时，而且所有客户端都表示已经跟上步伐

                if (m_touchCardStopwatch.ElapsedMilliseconds > m_touchCardDelay && isFollowed)
                {
                    for (int i = 0; i < m_players.Length; i++)
                    {
                        // 准备显示下一张牌
                        m_players[i].playerInfo.cardInHand.Add(m_tempHandCards[i][0]);
                        m_tempHandCards[i].RemoveAt(0);
                    }
                    //m_handCardShowNumber++;
                    // 重启计时器
                    //m_touchCardStopwatch.Reset();
                    //m_touchCardStopwatch.Start();
                    m_touchCardStopwatch.Restart();
                }
            }
            else// 如果台上方只有一个
            {
                // 直接把手牌发送到各个玩家
                for (int i = 0; i < m_players.Length; i++)
                {
                    m_players[i].playerInfo.cardInHand = m_tempHandCards[i];
                    // 将该玩家的手牌发送到客户端
                    ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
                }
                // 并指定是此台上方玩家抢到底牌
                m_dealer.gotBottomPlayerId = m_dealer.upperPlayersId[0];
            }

            //// 如果摸牌数达到手牌数, 说明摸牌已经结束
            //return m_handCardShowNumber >= Dealer.cardInHandInitialNumber;
            // 如果暂存手牌数组均为空，则说明摸牌已经结束
            bool isOk = true;
            for (int i = 0; i < m_tempHandCards.Length; i++)
            {
                isOk &= m_tempHandCards[i].Count == 0;
            }
            return isOk;
        }

        // 处理摸牌到最后抢底过渡阶段
        static void Touch2LastBid()
        {
            // 重新开始计时
            m_lastBidStopwatch.Restart();
        }

        // 处理最后抢底阶段
        // 返回是否超时
        static bool LastBid()
        {
            for (int i = 0; i < m_players.Length; i++)
            {
                // 向 4 个客户端发送当前的剩余时间（秒）
                ComServer.Respond(m_players[i].socket, (m_lastBidDelay - (int)m_lastBidStopwatch.ElapsedMilliseconds) / 1000);
                // 向 4 个客户端发送当前玩家的手牌
                ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));

                // 向该玩家发送当前可以亮牌的花色
                ComServer.Respond(m_players[i].socket, m_dealer.GetLegalBidColors(i/*,m_handCardShowNumber*/));

                // 接收每个玩家的亮牌决定
                bool isClickClub = (bool)ComServer.Respond(m_players[i].socket, "收到");
                bool isClickDiamond = (bool)ComServer.Respond(m_players[i].socket, "收到");
                bool isClickHeart = (bool)ComServer.Respond(m_players[i].socket, "收到");
                bool isClickSpade = (bool)ComServer.Respond(m_players[i].socket, "收到");

                // 根据现在的情况，更新该玩家的亮牌和手牌
                if (isClickClub)
                {
                    m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Club);
                }
                else if (isClickDiamond)
                {
                    m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Diamond);
                }
                else if (isClickHeart)
                {
                    m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Heart);
                }
                else if (isClickSpade)
                {
                    m_dealer.BidHelper(i, m_handCardShowNumber, Card.Suit.Spade);
                }
                else// 如果玩家还没有决定亮牌
                {

                }
                // 更新玩家的手牌
                m_players[i].playerInfo.cardInHand = m_dealer.playersHandCard[i];
                // 将该玩家的手牌发送到客户端
                ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
            }
            for (int i = 0; i < m_players.Length; i++)
            {
                // 将当前亮牌玩家的 ID 和亮牌发送到所有玩家
                for (int j = 0; j < m_dealer.currentBidCards.Length; j++)
                {
                    ComServer.Respond(m_players[i].socket, j);
                    ComServer.Respond(m_players[i].socket, Card.ToInt(m_dealer.currentBidCards[j]));
                }
            }
            return m_lastBidDelay < m_lastBidStopwatch.ElapsedMilliseconds;
        }

        // 处理最后抢底到埋底的过渡阶段
        static void LastBid2BidBury()
        {
            // 在最后摸牌结束后，将玩家的亮牌重新放回到手牌当中
            for (int i = 0; i < m_players.Length; i++)
            {
                m_players[i].playerInfo.cardInHand.AddRange(m_dealer.currentBidCards[i]);
                // 将手牌发送到客户端
                // 向 4 个客户端发送当前玩家的手牌
                ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
                // 将庄家 ID 发送给所有玩家
                ComServer.Respond(m_players[i].socket, m_dealer.gotBottomPlayerId);

            }
            // 将底牌发给庄家
            ComServer.Respond(m_players[m_dealer.gotBottomPlayerId].socket, Card.ToInt(m_dealer.bottom));

            // 并将底牌加入到庄家手牌
            m_players[m_dealer.gotBottomPlayerId].playerInfo.cardInHand.AddRange(m_dealer.bottom);

            // 启动埋底计时器
            m_bidBuryStopwatch.Restart();
        }

        // 抢底阶段：处理庄家埋底
        static bool BidBury()
        {
            Judgement judgement;
            bool isOk;

            bool isTimeOut = m_bidBuryStopwatch.ElapsedMilliseconds > m_bidBuryBottomDelay;

            for (int i = 0; i < m_players.Length; i++)
            {
                // 发送庄家剩余时间
                ComServer.Respond(m_players[i].socket, (m_bidBuryBottomDelay - (int)m_bidBuryStopwatch.ElapsedMilliseconds) / 1000);
            }
            // 告知当前炒底玩家是否超时
            ComServer.Respond(m_players[m_dealer.gotBottomPlayerId].socket, isTimeOut);

            // 检查当前亮牌玩家是否需要提示
            bool isNeedTips = (bool)ComServer.Respond(m_players[m_dealer.gotBottomPlayerId].socket, "收到");
            // 检查是否开启代理
            bool isAutoPlay = (bool)ComServer.Respond(m_players[m_dealer.gotBottomPlayerId].socket, "收到");

            // 如果玩家需要埋底提示，或者开启代理，或者已经超时
            if (isNeedTips || isAutoPlay || isTimeOut)
            {
                // 从荷官处获取亮牌提示
                Card[] buryCardTips = m_dealer.AutoBuryCard(m_dealer.gotBottomPlayerId);
                // 将亮牌提示返回到客户端
                ComServer.Respond(m_players[m_dealer.gotBottomPlayerId].socket, Card.ToInt(buryCardTips));
            }

            // 接收当前炒底玩家要埋的底牌
            m_buryCards = Card.ToCard((int[])ComServer.Respond(m_players[m_dealer.gotBottomPlayerId].socket, "收到出牌"));
            // 如果玩家有埋底
            if (m_buryCards.Length > 0)
            {
                Console.WriteLine("玩家 id=" + m_dealer.gotBottomPlayerId + "正在埋牌");
                // 检验埋底的合法性
                judgement = m_dealer.IsLegalBury(m_buryCards);
                // 向客户端发送埋底合法性
                ComServer.Respond(m_players[m_dealer.gotBottomPlayerId].socket, judgement);
                // 如果玩家所埋的牌合法
                if (judgement.isValid)
                {
                    // 将埋牌从玩家手牌中去除
                    for (int i = 0; i < m_buryCards.Length; i++)
                    {
                        m_players[m_dealer.gotBottomPlayerId].playerInfo.cardInHand.Remove(m_buryCards[i]);
                    }
                    // 将埋牌放到底牌
                    m_dealer.bottom = m_buryCards;
                }
                else// 如果玩家埋的牌不合法
                {

                }
            }
            else// 如果玩家还没有埋底
            {
                judgement = new Judgement("", false);
            }
            // 只有当玩家埋了底，而且是合法的，埋底这一步才算结束
            isOk = judgement.isValid && m_buryCards.Length > 0;
            return isOk;
        }

        static void Bid2Fry()
        {
            // 暂停并重置计时器
            m_touchCardStopwatch.Stop();
            m_touchCardStopwatch.Reset();
            // 重置计数器
            m_handCardShowNumber = 0;
            // 重置炒底亮牌数下界
            m_dealer.fryCardLowerBound = 0;
            // 设置当前庄家的下一个玩家为第一个炒底的玩家
            // 因为庄家已经买过底了
            m_dealer.currentFryPlayerId = (m_dealer.gotBottomPlayerId + 1) % Dealer.playerNumber;
            // 清空荷官存储的炒底阶段亮牌
            m_dealer.ClearShowCards();
            // 重启炒底阶段亮牌计时器
            m_showCardStopwatch.Restart();

            // 设置庄家
            m_dealer.bankerPlayerId.Clear();
            m_dealer.bankerPlayerId.Add(m_dealer.gotBottomPlayerId);
            // 将庄家 ID 发送到客户端
            for (int i = 0; i < m_players.Length; i++)
            {
                // 发送庄家剩余时间
                ComServer.Respond(m_players[i].socket, m_dealer.bankerPlayerId.ToArray());
            }
        }

        // 处理炒底流程
        // 测试：最多只能出 5 张牌；只能出比别人多；可以选择不跟
        // 处理炒底阶段亮牌流程
        // 返回：亮牌阶段是否结束；玩家要么成功亮牌，要么不跟
        static bool FryShow()
        {
            // 亮牌合法性判决
            Judgement judgement;
            // 亮牌阶段是否结束
            bool isOk;

            // 当前玩家是否已经超时
            bool isTimeOut = m_showCardStopwatch.ElapsedMilliseconds > m_showCardDelay;

            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                // 向所有玩家发送当前炒底玩家 ID
                ComServer.Respond(m_players[j].socket, m_dealer.currentFryPlayerId);
                // 发送当前炒底玩家剩余思考时间
                ComServer.Respond(m_players[j].socket, (m_showCardDelay - (int)m_showCardStopwatch.ElapsedMilliseconds) / 1000);
            }
            // 告知当前炒底玩家是否超时
            ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, isTimeOut);

            // 如果超时了
            if (isTimeOut)
            {
                // 认为玩家不跟
                m_isFollow = false;
            }
            else
            {
                // 询问客户端玩家是否选择不跟
                m_isFollow = (bool)ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
            }

            // 如果玩家选择跟牌
            if (m_isFollow)
            {
                // 检查当前亮牌玩家是否需要提示
                bool isNeedTips = (bool)ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
                // 检查当前炒底玩家是否开启代理
                bool isAutoPlay = (bool)ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");

                // 如果玩家需要亮牌提示
                if (isNeedTips)
                {
                    // 从荷官处获取亮牌提示
                    Card[] showCardTips = m_dealer.AutoAddShowCard(m_dealer.currentFryPlayerId);
                    // 将亮牌提示返回到客户端
                    ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(showCardTips));
                }
                // 如果玩家开启了代理
                if (isAutoPlay)
                {
                    // 从荷官处获取亮牌提示
                    Card[] showCardTips = m_dealer.AutoAddShowCard(m_dealer.currentFryPlayerId);
                    // 将亮牌提示返回到客户端
                    ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(showCardTips));
                }

                // 接收当前炒底玩家的新增亮牌
                m_addFryCards = Card.ToCard((int[])ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到出牌"));
                // 如果玩家有出牌
                if (m_addFryCards.Length > 0)
                {
                    Console.WriteLine("玩家 id=" + m_dealer.currentFryPlayerId + ", 正在亮牌");
                    // 交给荷官检查
                    judgement = m_dealer.IsLegalShow(m_addFryCards, m_dealer.currentFryPlayerId);
                    // 先返回判断结果
                    ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, judgement);
                    // 如果合法
                    if (judgement.isValid)
                    {
                        Console.WriteLine("玩家 id=" + m_dealer.currentFryPlayerId + " 成功亮牌，准备埋底");

                        // 将亮牌加入筹码
                        m_dealer.showCards[m_dealer.currentFryPlayerId].AddRange(m_addFryCards);
                        // 将亮牌从玩家手牌中去除
                        for (int i = 0; i < m_addFryCards.Length; i++)
                        {
                            m_players[m_dealer.currentFryPlayerId].playerInfo.cardInHand.Remove(m_addFryCards[i]);
                        }
                        // 客户端会自行将亮牌从手牌中去除，不用从服务器发送过去了
                        // 更新炒底的筹码下界
                        m_dealer.fryCardLowerBound = m_dealer.showCards[m_dealer.currentFryPlayerId].Count;
                        // 重置不跟的玩家个数
                        m_dealer.skipFryCount = 0;
                    }
                    else// 如果不合法
                    {
                        Console.WriteLine("玩家 id=" + m_dealer.currentFryPlayerId + judgement.message);
                    }
                }
                else// 如果玩家还没有出牌
                {
                    judgement = new Judgement("还没亮牌", false);
                }
                isOk = m_addFryCards.Length > 0 && judgement.isValid;
                // 向所有客户端发送状态，告知是否要接收出牌
                for (int j = 0; j < Dealer.playerNumber; j++)
                {
                    ComServer.Respond(m_players[j].socket, isOk);
                }
                if (isOk)
                {
                    // 向客户端发送当前玩家最新的亮牌
                    for (int j = 0; j < Dealer.playerNumber; j++)
                    {
                        ComServer.Respond(m_players[j].socket, Card.ToInt(m_dealer.showCards[m_dealer.currentFryPlayerId].ToArray()));
                    }
                }
                else
                {

                }
            }
            else// 否则，如果玩家不跟
            {
                isOk = true;
                // 向所有客户端发送状态，告知不要接收出牌
                for (int j = 0; j < Dealer.playerNumber; j++)
                {
                    ComServer.Respond(m_players[j].socket, false);
                }
            }
            return isOk;
        }

        // 处理炒底阶段亮牌到埋底过渡阶段
        static void FryShow2Bury()
        {
            // 告知客户端当前炒底玩家没有跟牌
            ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, m_isFollow);

            // 如果玩家有跟牌
            if (m_isFollow)
            {
                // 将底牌发送给亮牌成功的玩家
                // 向当前炒底玩家发送底牌
                ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(m_dealer.bottom));
                // 将底牌加入到炒底玩家的手牌当中去
                m_players[m_dealer.currentFryPlayerId].playerInfo.cardInHand.AddRange(m_dealer.bottom);
            }
            else// 如果玩家不跟
            {
                // 增加不跟的玩家数
                m_dealer.skipFryCount++;
            }
            // 启动埋底计时器
            m_buryCardStopwatch.Restart();
        }

        // 处理炒底阶段埋底流程
        // 返回：玩家是否完成埋底
        static bool FryBury()
        {
            Judgement judgement;
            bool isOk;

            // 当前玩家是否已经超时
            bool isTimeOut = m_buryCardStopwatch.ElapsedMilliseconds > m_buryBottomDelay;

            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                // 发送当前炒底玩家剩余思考时间
                ComServer.Respond(m_players[j].socket, (m_buryBottomDelay - (int)m_buryCardStopwatch.ElapsedMilliseconds) / 1000);
            }
            // 告知当前炒底玩家是否超时
            ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, isTimeOut);

            // 检查当前亮牌玩家是否需要提示
            bool isNeedTips = (bool)ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
            // 检查是否开启代理
            bool isAutoPlay = (bool)ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");

            // 如果玩家需要埋底提示，或者开启代理，或者已经超时
            if (isNeedTips || isAutoPlay || isTimeOut)
            {
                // 从荷官处获取亮牌提示
                Card[] buryCardTips = m_dealer.AutoBuryCard(m_dealer.currentFryPlayerId);
                // 将亮牌提示返回到客户端
                ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(buryCardTips));
            }
            // 接收当前炒底玩家要埋的底牌
            m_buryCards = Card.ToCard((int[])ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到出牌"));
            // 如果玩家有埋底
            if (m_buryCards.Length > 0)
            {
                Console.WriteLine("玩家 id=" + m_dealer.currentFryPlayerId + "正在埋牌");
                // 检验埋底的合法性
                judgement = m_dealer.IsLegalBury(m_buryCards);
                // 向客户端发送埋底合法性
                ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, judgement);
                // 如果玩家所埋的牌合法
                if (judgement.isValid)
                {
                    // 将埋牌从玩家手牌中去除
                    for (int i = 0; i < m_buryCards.Length; i++)
                    {
                        m_players[m_dealer.currentFryPlayerId].playerInfo.cardInHand.Remove(m_buryCards[i]);
                    }
                    // 将埋牌放到底牌
                    m_dealer.bottom = m_buryCards;
                }
                else// 如果玩家埋的牌不合法
                {

                }
            }
            else// 如果玩家还没有埋底
            {
                judgement = new Judgement("", false);
            }
            // 只有当玩家埋了底，而且是合法的，埋底这一步才算结束
            isOk = judgement.isValid && m_buryCards.Length > 0;
            return isOk;
        }

        // 处理炒底阶段，从埋底重新回到亮牌的过渡流程
        static void FryBury2Show()
        {
            // 设置下一玩家亮牌
            m_dealer.currentFryPlayerId++;
            // 重启亮牌计时器
            m_showCardStopwatch.Restart();
        }

        static void Fry2FindFriend()
        {
            // 重启寻友计时器
            m_findFriendStopwatch.Restart();
        }

        static bool FindFriend()
        {
            // 是否完成寻友
            bool isOk;
            // 玩家是否已经超时
            bool isTimeOut = m_findFriendStopwatch.ElapsedMilliseconds > m_findFriendDelay;
            // 获取庄家 ID 
            int bankerId = m_dealer.bankerPlayerId[0];
            // 庄家是否有操作的标志
            bool hasOperation = false;
            // 向所有玩家
            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                // 发送庄家剩余思考时间
                ComServer.Respond(m_players[j].socket, (m_findFriendDelay - (int)m_findFriendStopwatch.ElapsedMilliseconds) / 1000);
            }
            // 告知庄家是否超时
            ComServer.Respond(m_players[bankerId].socket, isTimeOut);
            // 如果已经超时了
            if (isTimeOut)
            {
                // 认为庄家选择单打
                m_dealer.bankerIsFightAlone = true;
                m_dealer.signCard = null;
                isOk = true;
                hasOperation = false;
            }
            else
            {
                // 检查庄家是否选择寻友
                bool isFindFriend = (bool)ComServer.Respond(m_players[bankerId].socket, "收到");

                // 检查庄家是否选择单打
                bool isAlone = (bool)ComServer.Respond(m_players[bankerId].socket, "收到");
                
                isOk = isAlone || isFindFriend;
                hasOperation = isAlone || isFindFriend;
                // 如果庄家决定单打
                if (isAlone)
                {
                    m_dealer.bankerIsFightAlone = true;
                    m_dealer.signCard = null;
                }
                // 如果庄家决定寻友
                else if (isFindFriend)
                {
                    // 让荷官生成信号牌
                    m_dealer.GenerateSignCard();
                    m_dealer.bankerIsFightAlone = false;
                }
                // 如果庄家还没有操作
                else
                {
                }
            } 
            // 告诉所有玩家，庄家是否有操作
            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                ComServer.Respond(m_players[j].socket, hasOperation);
            }
            if (hasOperation)
            {
                // 告诉所有玩家，庄家是否选择单打
                for (int j = 0; j < Dealer.playerNumber; j++)
                {
                    ComServer.Respond(m_players[j].socket, m_dealer.bankerIsFightAlone);
                }
                // 如果庄家选择寻友
                if (!m_dealer.bankerIsFightAlone)
                {
                    // 向所有玩家发送信号牌
                    for (int j = 0; j < Dealer.playerNumber; j++)
                    {
                        ComServer.Respond(m_players[j].socket, m_dealer.signCard);
                    }
                }
            }
            return isOk;
        }

        static void FindFriend2Linger()
        {
            // 重启计时器
            m_findFriendLingerStopwatch.Restart();
        }

        static bool FindFriendLinger()
        {
            return m_findFriendLingerStopwatch.ElapsedMilliseconds>m_findFriendLingerDelay;
        }

        static void FindFriend2Fight()
        {
            // 首次出牌对牌数没有限制
            m_dealer.dealRequiredLength = -1;
            // 重置轮数为 1 
            m_dealer.circle = 1;
            // 标志可以开始计时
            m_isOkCountDown = true;

            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                // 将炒底的亮牌重新放到玩家的手牌里去
                m_players[j].playerInfo.cardInHand.AddRange(m_dealer.showCards[j]);
                // 向玩家发送手牌
                ComServer.Respond(m_players[j].socket, Card.ToInt(m_players[j].playerInfo.cardInHand/*.ToArray()*/));
            }
            // 清空亮牌
            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                m_dealer.showCards[j].Clear();
            }
        }

        static void Fight()
        {
            // 如果不是第 1 轮出牌，而且所有玩家都出过 1 次牌，而且还没完成 1 次延时
            // 则继续延迟；否则不延迟，正常出牌
            bool isClearDelay = m_dealer.circle > 1 && m_dealer.handOutPlayerCount == 0 && !m_doneClearPlayCardDelay;
            //bool isClearDelaying = isClearDelay && m_clearPlayCardStopwatch.ElapsedMilliseconds < m_clearPlayCardDelay;
            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                // 通知客户端是否需要延迟清理
                ComServer.Respond(m_players[j].socket, isClearDelay);
            }
            // 如果不需要延迟，则正常运行
            if (!isClearDelay)
            {
                // 如果可以开始为出牌思考计时
                if (m_isOkCountDown)
                {
                    // 启动计时器
                    m_handOutStopwatch.Start();
                    // 重置 flag
                    m_isOkCountDown = false;
                }
                // 玩家是否已经没有时间思考
                bool isTimeOut = m_handOutStopwatch.ElapsedMilliseconds > m_handOutTimeLimit;

                // 玩家轮流出牌
                //Console.WriteLine("轮到 {0} 出牌", m_players[m_dealer.currentPlayerId].playerInfo.name);
                for (int j = 0; j < Dealer.playerNumber; j++)
                {
                    // 通知所有玩家允许出牌者的 id
                    ComServer.Respond(m_players[j].socket, m_dealer.currentPlayerId);
                    // 通知所有玩家，首家 ID
                    ComServer.Respond(m_players[j].socket, m_dealer.firstHomePlayerId);
                    // 向所有客户端发送剩余思考时间（毫秒）
                    ComServer.Respond(m_players[j].socket, Math.Max(0, (m_handOutTimeLimit - (int)m_handOutStopwatch.ElapsedMilliseconds) / 1000));
                    // 向所有客户端发送当前出牌者是否已经超时思考
                    ComServer.Respond(m_players[j].socket, isTimeOut);

                }
                // 检查当前亮牌玩家是否需要提示
                bool isNeedTips = (bool)ComServer.Respond(m_players[m_dealer.currentPlayerId].socket, "收到");
                // 玩家是否开启代理
                bool isAutoPlay = (bool)ComServer.Respond(m_players[m_dealer.currentPlayerId].socket, "收到");

                // 如果玩家需要亮牌提示
                if (isNeedTips)
                {
                    // 从荷官处获取亮牌提示
                    Card[] dealCardTips = m_dealer.AutoHandOut(m_dealer.currentPlayerId);
                    // 将亮牌提示返回到客户端
                    ComServer.Respond(m_players[m_dealer.currentPlayerId].socket, Card.ToInt(dealCardTips));
                }

                // 如果玩家选择代理
                if (isAutoPlay)
                {
                    // 自动出牌
                    m_dealCards = m_dealer.AutoHandOut(m_dealer.currentPlayerId);
                }
                else// 如果玩家没有代理
                {
                    // 如果玩家已经没有剩余时间思考
                    if (isTimeOut)
                    {
                        // 自动出牌
                        m_dealCards = m_dealer.AutoHandOut(m_dealer.currentPlayerId);
                    }
                    else// 如果玩家还有剩余思考时间
                    {
                        // 接受他选择的出牌
                        //object temp = Serializer.Receive(m_players[m_dealer.currentPlayerId].socket);
                        //Card[] m_dealCards = Card.ToCard((int[])temp);

                        m_dealCards = Card.ToCard((int[])ComServer.Respond(m_players[m_dealer.currentPlayerId].socket, "收到出牌"));
                    }
                }

                // 如果有出牌
                if (m_dealCards.Length > 0)
                {

                    Console.WriteLine("{0} 的出牌", m_players[m_dealer.currentPlayerId].playerInfo.name);
                    Card.PrintDeck(m_dealCards);

                    // 用来判断出牌合法性的所有信息都包含在 Dealer 里头
                    Judgement judgement = m_dealer.IsLegalDeal(m_dealCards);

                    // 当玩家是自主行动，不是代理出牌
                    if (!isTimeOut && !isAutoPlay)
                    {
                        // 先把合法性判断返回客户端
                        // 一定要保证 Receive 和 Send 操作之间, 没有其他网络通信
                        ComServer.Respond(m_players[m_dealer.currentPlayerId].socket, judgement);
                    }

                    // 如果出牌合法
                    if (judgement.isValid)
                    {
                        // 将选牌从手牌中扣除
                        for (int j = 0; j < m_dealCards.Length; j++)
                        {
                            // 找到选牌在手牌中的位置
                            m_players[m_dealer.currentPlayerId].playerInfo.cardInHand.Remove(m_dealCards[j]);
                        }
                        // 如果这是首家
                        // 测试：直接假定 ID=0 的玩家是首家
                        // TODO: 要根据牌的大小确定下轮的首家
                        if (m_dealer.currentPlayerId == m_dealer.firstHomePlayerId)
                        {
                            // 设置出牌要求长度为首家出牌长度
                            m_dealer.dealRequiredLength = m_dealCards.Length;
                        }
                        // 重置思考计时器
                        m_handOutStopwatch.Reset();
                    }
                    // 向出牌玩家发送手牌
                    // 如果出牌合法，这手牌有所减少；否则，手牌没有改变
                    ComServer.Respond(m_players[m_dealer.currentPlayerId].socket, Card.ToInt(m_players[m_dealer.currentPlayerId].playerInfo.cardInHand));

                    for (int i = 0; i < m_players.Length; i++)
                    {
                        // 将出牌的处理结果发放给所有玩家
                        ComServer.Respond(m_players[i].socket, judgement.isValid);
                    }

                    if (judgement.isValid)
                    {
                        // 向所有玩家发送该玩家的出牌
                        for (int j = 0; j < Dealer.playerNumber; j++)
                        {
                            // 先发送 ID
                            ComServer.Respond(m_players[j].socket, m_players[m_dealer.currentPlayerId].playerInfo.id);

                            // 再发送出牌
                            ComServer.Respond(m_players[j].socket, Card.ToInt(m_dealCards));
                        }
                        // 存储出牌到荷官
                        m_dealer.handOutCards[m_dealer.currentPlayerId] = new List<Card>(m_dealCards);
                        // 更新首家
                        m_dealer.UpdateFirstHome();
                        // 下一玩家出牌
                        m_dealer.handOutPlayerCount++;
                        m_dealer.UpdateNextPlayer();
                        m_dealer.circle++;
                        // 如果最后一个玩家出牌
                        if (m_dealer.handOutPlayerCount == 0)
                        {
                            // 标志需要 1 次延时
                            m_doneClearPlayCardDelay = false;
                            // 清空荷官中存储的本轮玩家出牌
                            m_dealer.ClearHandOutCards();
                        }
                        // 标志可以开始为下一玩家出牌思考计时
                        m_isOkCountDown = true;
                    }
                    // 否则, 还是同一玩家出牌
                }
                else// 否则如果玩家还没有出牌
                {
                    for (int i = 0; i < m_players.Length; i++)
                    {
                        // 告知所有玩家，当前玩家还没有出牌
                        ComServer.Respond(m_players[i].socket, false);
                    }
                    // 继续计时
                    // 如果已经


                }
                // 还是同一玩家出牌
            }
            else// 如果需要延迟
            {
                // 如果计时器还没有启动
                if (!m_clearPlayCardStopwatch.IsRunning)
                {
                    // 启动计时器
                    m_clearPlayCardStopwatch.Start();
                }
                // 如果已经延迟足够了
                else if (m_clearPlayCardStopwatch.ElapsedMilliseconds > m_clearPlayCardDelay)
                {
                    // 重置计时器
                    m_clearPlayCardStopwatch.Reset();
                    // 标志已经完成 1 次延时
                    m_doneClearPlayCardDelay = true;
                }
            }

        }

        static void Fight2Score()
        {

        }

        // 处理计分流程
        static void Score()
        {

        }

        static void Score2Deal()
        {
            // 计分结束，盘数增加
            m_dealer.round++;
            //// 清空玩家手牌
            //for(int i = 0; i < m_players.Length; i++)
            //{
            //    m_players[i].playerInfo.cardInHand.Clear();
            //}
        }

        // 更新荷官需要掌握的信息
        static void UpdateDealer()
        {
            for (int i = 0; i < Dealer.playerNumber; i++)
            {
                // 更新荷官掌握的玩家手牌
                m_dealer.playersHandCard[i] = m_players[i].playerInfo.cardInHand;
                // 更新玩家的级数
                m_dealer.playerLevels[i] = m_players[i].playerInfo.level;
            }
            // 更新台上方玩家
            m_dealer.UpdateUpperPlayers();

            // 如果轮到首家出牌
            if (m_dealer.currentPlayerId == m_dealer.firstHomePlayerId)
            {
                // 重新设置出牌要求长度, 即没有要求
                m_dealer.dealRequiredLength = -1;
            }
        }

        // 游戏主循环
        static void GameLoop()
        {
            bool isOk;
            while (true)
            {
                // 向所有玩家发送最新的游戏状态
                for (int i = 0; i < Dealer.playerNumber; i++)
                {
                    ComServer.Respond(m_players[i].socket, m_gameStateMachine.state);
                }

                // 更新荷官掌握的信息
                UpdateDealer();

                switch (m_gameStateMachine.state)
                {
                    case GameStateMachine.State.Deal:
                        // 重新发牌
                        Console.WriteLine("正在发牌");
                        DealCards();
                        // 告知游戏状态机已经完成发牌
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneDeal);
                        break;
                    case GameStateMachine.State.Deal2Bid:
                        Deal2Bid();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneDeal2Bid);
                        Console.WriteLine("开始抢底");
                        break;
                    //case GameStateMachine.State.Bid:
                    case GameStateMachine.State.Touch:
                        bool isOkTouch = Touch();
                        bool noHigherBid = m_dealer.NoHigherBid();
                        // 如果摸牌结束，而且已经不可能有人要亮牌
                        if (isOkTouch && noHigherBid)
                        {
                            Console.WriteLine("不可能有更高出价者, 抢底结束");
                            m_gameStateMachine.Update(GameStateMachine.Signal.NoHigherBid);
                        }
                        // 如果摸牌结束，而且还有人可能要亮牌
                        else if (isOkTouch && !noHigherBid)
                        {
                            // 完成摸牌，准备进入最后抢底阶段
                            m_gameStateMachine.Update(GameStateMachine.Signal.DoneTouch);
                        }
                        //// 如果不可能有更高出价者
                        //if (m_dealer.NoHigherBid())
                        //{
                        //    // 完成摸牌
                        //    m_gameStateMachine.Update(GameStateMachine.Signal.DoneTouch);
                        //}
                        //// 进行抢底
                        //// 如果摸牌结束
                        //if (Touch())
                        //{
                        //    // 测试：暂且直接进入炒底阶段
                        //    //m_gameStateMachine.Update(GameStateMachine.Signal.NoHigherBid);
                        //    // 测试：设抢到底牌者是 id=0 者
                        //    m_dealer.gotBottomPlayerId = 0;
                        //    // 完成摸牌
                        //    m_gameStateMachine.Update(GameStateMachine.Signal.DoneTouch);
                        //}
                        break;
                    case GameStateMachine.State.Touch2LastBid:
                        Touch2LastBid();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneTouch2LastBid);
                        break;
                    case GameStateMachine.State.LastBid:
                        bool isOverTime = LastBid();
                        // 如果已经没有更高的出价，或者已经超时了
                        if (m_dealer.NoHigherBid() || isOverTime)
                        {
                            // 直接让庄家埋底
                            m_gameStateMachine.Update(GameStateMachine.Signal.EndLastBid);
                        }
                        break;
                    case GameStateMachine.State.LastBid2BidBury:
                        LastBid2BidBury();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneLastBid2BidBury);
                        break;
                    case GameStateMachine.State.BidBury:
                        isOk = BidBury();
                        if (isOk)
                        {
                            m_gameStateMachine.Update(GameStateMachine.Signal.DoneBidBury);
                        }
                        break;
                    case GameStateMachine.State.Bid2Fry:
                        Bid2Fry();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneBid2Fry);
                        Console.WriteLine("开始炒底");
                        break;
                    //case GameStateMachine.State.Fry:
                    //    // 如果炒底结束
                    //    //if (m_dealer.FryEnd())
                    //    // 测试
                    //    if(true)
                    //    {
                    //        m_gameStateMachine.Update(GameStateMachine.Signal.FryEnd);
                    //        break;
                    //    }
                    //    Fry();
                    //    break;
                    case GameStateMachine.State.FryShow:
                        // 如果亮牌阶段结束
                        if (FryShow())
                        {
                            // 该玩家得以继续埋底
                            m_gameStateMachine.Update(GameStateMachine.Signal.SuccessfulShow);
                        }
                        else// 否则继续亮牌
                        {

                        }
                        break;
                    case GameStateMachine.State.FryShow2Bury:
                        FryShow2Bury();
                        // 如果玩家有跟牌
                        if (m_isFollow)
                        {
                            // 继续埋底
                            m_gameStateMachine.Update(GameStateMachine.Signal.DoneFryShow2Bury);
                        }
                        else if (!m_dealer.FryEnd())// 否则，如果玩家没有跟牌，而且炒底阶段还没有结束
                        {
                            // 直接到下一玩家亮牌
                            m_gameStateMachine.Update(GameStateMachine.Signal.FryContinue);
                        }
                        else// 如果大家都不跟，则炒底阶段结束
                        {
                            m_gameStateMachine.Update(GameStateMachine.Signal.FryEnd);
                        }
                        break;
                    case GameStateMachine.State.FryBury:
                        isOk = FryBury();
                        // 如果炒底阶段结束
                        //if (m_dealer.NoHigerFry())
                        if (isOk && m_dealer.FryEnd())
                        {
                            // 结束炒底流程
                            m_gameStateMachine.Update(GameStateMachine.Signal.FryEnd);
                        }
                        else if (isOk)// 否则，如果完成埋底，继续亮牌
                        {
                            m_gameStateMachine.Update(GameStateMachine.Signal.FryContinue);
                        }
                        else// 如果玩家还没有埋底，继续等待玩家埋底
                        {

                        }
                        break;
                    case GameStateMachine.State.FryBury2Show:
                        FryBury2Show();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneFryBury2Show);
                        break;
                    //case GameStateMachine.State.Fry2Fight:
                    //    Fry2Fight();
                    //    m_gameStateMachine.Update(GameStateMachine.Signal.DoneFry2Fight);
                    //    Console.WriteLine("开始对战");
                    //    break;
                    case GameStateMachine.State.Fry2FindFriend:
                        Fry2FindFriend();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneFry2FindFriend);
                        break;
                    case GameStateMachine.State.FindFriend:
                        isOk = FindFriend();
                        if (isOk)
                        {
                            if (m_dealer.bankerIsFightAlone)
                            {
                                m_gameStateMachine.Update(GameStateMachine.Signal.IsFightAlone);
                            }
                            else
                            {
                                m_gameStateMachine.Update(GameStateMachine.Signal.HasFriend);
                            }
                        }
                        break;
                    case GameStateMachine.State.FindFriend2Linger:
                        FindFriend2Linger();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneFindFriend2Linger);
                        break;
                    case GameStateMachine.State.FindFriendLinger:
                        isOk = FindFriendLinger();
                        if (isOk)
                        {
                            m_gameStateMachine.Update(GameStateMachine.Signal.DoneFindFriendLinger);
                        }
                        break;
                    case GameStateMachine.State.FindFriend2Fight:
                        FindFriend2Fight();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneFindFriend2Fight);
                        break;
                    case GameStateMachine.State.Fight:
                        // 玩家轮流出牌
                        Fight();
                        // 如果所有玩家的手牌为空
                        if (m_dealer.AllPlayersHandEmpty())
                        {
                            Console.WriteLine("所有玩家手牌打空, 本局结束");
                            // 告知游戏状态机, 所有玩家手牌已空
                            m_gameStateMachine.Update(GameStateMachine.Signal.PlayerHandCardAllEmpty);
                            break;
                        }
                        break;
                    case GameStateMachine.State.Fight2Score:
                        Fight2Score();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneFight2Score);
                        break;
                    case GameStateMachine.State.Score:
                        Score();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneScore);
                        break;
                    case GameStateMachine.State.Score2Deal:
                        Score2Deal();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneScore2Deal);
                        break;
                }
            }
        }

        static void Main(string[] args)
        {
            // 初始化
            Initialize();
            // 开始游戏主循环
            GameLoop();
        }

    }
}
