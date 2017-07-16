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
        static string m_ipStr = "192.168.88.101";

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

        // 当前应该显示牌数
        static int m_handCardShowNumber;
        // 抢底阶段显示手牌的延迟(毫秒)
        static int m_touchCardDelay = 1000;
        // 用于辅助模拟摸牌效果的计时器
        static Stopwatch m_touchCardStopwatch;

        // 炒底亮牌数组
        static Card[] m_addFryCards;

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
            // 新建延迟清理桌面计时器
            m_clearPlayCardStopwatch = new Stopwatch();
            // 新建出牌计时器
            m_handOutStopwatch = new Stopwatch();
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
                m_players[i].playerInfo.cardInHand = new List<Card>(cards);

                // 通过网络将手牌发送给客户端
                // 用 int 数组格式发送
                Console.WriteLine("向 " + m_players[i].playerInfo.name + " 发送手牌");
                ComServer.Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
            }
        }

        static void Deal2Bid()
        {
            // 开始计时
            m_touchCardStopwatch.Start();

            // 向客户端发送当前台上方的数目
            // 如果只有 1 个台上方, 则跳过摸牌效果; 否则, 才需要模拟摸牌
            for (int i = 0; i < Dealer.playerNumber; i++)
            {
                ComServer.Respond(m_players[i].socket, m_dealer.upperPlayersId);
            }
            m_handCardShowNumber = 0;
        }

        // 处理抢底流程
        // 返回是否结束摸牌
        static bool Bid()
        {
            // 是否所有服务器都已经显示当前要显示的那一张牌，即它们是否已经跟上节奏
            bool isFollowed = true;

            // 向 4 个客户端发送当前应该显示的牌数
            for (int i = 0; i < m_players.Length; i++)
            {
                ComServer.Respond(m_players[i].socket, m_handCardShowNumber);
                isFollowed &= (bool)ComServer.Respond(m_players[i].socket, "收到");
            }

            // 获取当前应该显示的手牌数
            //int handCardShowNumber = (int)m_touchCardStopwatch.ElapsedMilliseconds / m_touchCardDelay;
            // 如果计时器显示已经超过显示手牌延时，而且所有客户端都表示已经跟上步伐

            if (m_touchCardStopwatch.ElapsedMilliseconds > m_touchCardDelay && isFollowed)
            {
                // 准备显示下一张牌
                m_handCardShowNumber++;
                // 重启计时器
                //m_touchCardStopwatch.Reset();
                //m_touchCardStopwatch.Start();
                m_touchCardStopwatch.Restart();
            }

            // 如果摸牌数达到手牌数, 说明摸牌已经结束
            return m_handCardShowNumber >= Dealer.cardInHandInitialNumber;
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
            // 设置当前炒底玩家为抢到底牌的人
            // 抢到底牌的玩家首先炒底
            m_dealer.currentFryPlayerId = m_dealer.gotBottomPlayerId;
            // 清空荷官存储的炒底阶段亮牌
            m_dealer.ClearShowCards();
        }

        // 处理炒底流程
        // 测试：最多只能出 5 张牌；只能出比别人多；可以选择不跟

        static void Fry()
        {
            // 向所有玩家发送当前炒底玩家 ID
            for (int j = 0; j < Dealer.playerNumber; j++)
            {
                ComServer.Respond(m_players[j].socket, m_dealer.currentFryPlayerId);
            }
            // 向当前炒底玩家发送底牌
            ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(m_dealer.bottom));
            // 将底牌加入到炒底玩家的手牌当中去
            m_players[m_dealer.currentFryPlayerId].playerInfo.cardInHand.AddRange(m_dealer.bottom);
            // 接收当前炒底玩家的新增亮牌
            m_addFryCards = Card.ToCard((int[])ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到出牌"));
            // 如果玩家有出牌
            if (m_dealCards.Length > 0)
            {
                // 交给荷官检查
                Judgement judgement = m_dealer.IsLegalFry(m_addFryCards,m_dealer.currentFryPlayerId);
                // 先返回判断结果
                ComServer.Respond(m_players[m_dealer.currentFryPlayerId].socket, judgement);
                // 如果合法
                if (judgement.isValid)
                {
                    // 将亮牌加入筹码
                    m_dealer.showCards[m_dealer.currentFryPlayerId].AddRange(m_addFryCards);
                    // 将亮牌从玩家手牌中去除
                    for(int i = 0; i < m_addFryCards.Length; i++)
                    {
                        m_players[m_dealer.currentFryPlayerId].playerInfo.cardInHand.Remove(m_addFryCards[i]);
                    }
                }
                else// 如果不合法
                {

                }
            }
            else// 如果玩家还没有出牌
            {

            }

        }

        static void Fry2Fight()
        {
            // 首次出牌对牌数没有限制
            m_dealer.dealRequiredLength = -1;
            // 重置轮数为 1 
            m_dealer.circle = 1;
            // 标志可以开始计时
            m_isOkCountDown = true;
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
                // 玩家是否开启代理
                bool isAutoPlay = (bool)ComServer.Respond(m_players[m_dealer.currentPlayerId].socket,"收到");
                // 如果玩家选择代理
                if(isAutoPlay)
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
                    if (!isTimeOut&&!isAutoPlay)
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
            // 更新荷官掌握的玩家手牌
            for (int i = 0; i < m_dealer.playersHandCard.Length; i++)
            {
                m_dealer.playersHandCard[i] = m_players[i].playerInfo.cardInHand;
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
                    case GameStateMachine.State.Bid:
                        // 如果不可能有更高出价者
                        if (m_dealer.NoHigherBid())
                        {
                            Console.WriteLine("不可能有更高出价者, 抢底结束");
                            m_gameStateMachine.Update(GameStateMachine.Signal.NoHigherBid);
                            break;
                        }
                        // 进行抢底
                        // 如果摸牌结束
                        if (Bid())
                        {
                            // 暂且直接进入炒底阶段
                            m_gameStateMachine.Update(GameStateMachine.Signal.NoHigherBid);
                            // 测试：设抢到底牌者是 id=0 者
                            m_dealer.gotBottomPlayerId = 0;
                        }
                        break;
                    case GameStateMachine.State.Bid2Fry:
                        Bid2Fry();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneBid2Fry);
                        Console.WriteLine("开始炒底");

                        break;
                    case GameStateMachine.State.Fry:
                        // 如果炒底结束
                        //if (m_dealer.FryEnd())
                        // 测试
                        if(true)
                        {
                            m_gameStateMachine.Update(GameStateMachine.Signal.FryEnd);
                            break;
                        }
                        Fry();
                        break;
                    case GameStateMachine.State.Fry2Fight:
                        Fry2Fight();
                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneFry2Fight);
                        Console.WriteLine("开始对战");
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
