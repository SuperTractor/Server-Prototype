#define DATABASE
//#undef DATABASE

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
//using cn.bmob.exception;
//using cn.bmob.Extensions;
//using cn.bmob.http;
//using cn.bmob.io;
//using cn.bmob.json;
//using cn.bmob.response;
//using cn.bmob.tools;
//using cn.bmob.api;
//using cn.bmob.config;
using ConsoleUtility;
//using BmobInterface;
//using Database;
using DatabaseUtility;
using DBNetworking;

namespace Server
{
    public class RoomService
    {
        // 房间号
        string m_id;
        public string id
        {
            get { return m_id; }
        }

        // 是否完成房间服务的标志
        bool m_doneService = false;
        public bool doneService
        {
            get { return m_doneService; }
        }

        // 房间满人之后，标志可以开始循环
        bool m_isOk2Loop;

        object obj;

        // 房间最大可容纳人数
        int m_roomSize = 100000000;

        // 开始游戏要求的玩家个数
        // 当前在线的玩家数目
        int m_playersCount = 0;
        // 玩家等候列表
        List<PlayerInfo> m_playersWaitLine;

        // 玩家列表
        // Player[] m_players;
        List<PlayerInfo> m_playerInfos;

        // 玩家的网络通信接口
        List<Player> m_players;


        // 从 Bmob 提取的玩家统计数据
        // List<StatObject> m_userStats;
        // StatObject m_userStat;

        // 游戏准备好开始了
        bool m_isReady;

        // 玩家是否连接
        bool[] playersIsConnected;
        // 广播给所有在线玩家的消息
        string m_broadcastMessage;

        // 荷官
        Dealer m_dealer;
        // 牌的排序家
        CardSorter m_cardSorter;
        // 出牌要求的牌数
        // 游戏状态机
        GameStateMachine m_gameStateMachine;

        // 用于辅助模拟摸牌效果，暂且放置手牌
        List<Card>[] m_tempHandCards;

        // 当前应该显示牌数
        int m_handCardShowNumber;

        // 用于辅助模拟摸牌效果的计时器
        Stopwatch m_touchCardStopwatch;
        // 用于最后抢底阶段的计时器
        Stopwatch m_lastBidStopwatch;

        // 用于亮牌的计时器
        Stopwatch m_showBottomStopwatch;
#if (RULE)
                // 抢底阶段显示手牌的延迟(毫秒)
         int m_touchCardDelay = 1;
        // 最后抢底阶段有的思考时间（毫秒）
         int m_lastBidDelay = 10000;
                // 抢底阶段：庄家埋底思考时间
         int m_bidBuryBottomDelay = 3000000;
                // 亮牌思考时间（毫秒）
         int m_showCardDelay = 1000000;
               // 埋底思考时间（毫秒）
         int m_buryBottomDelay = 3000000;
                // 寻友阶段庄家思考时间（毫秒）
         int m_findFriendDelay = 10000;
        // 寻友结束后，如果庄家有找朋友，则停留几秒让所有玩家看清楚信号牌
         int m_findFriendLingerDelay = 2000;
                // 4 个玩家都出牌后的延迟（毫秒）
         int m_clearPlayCardDelay = 1500;
                // 玩家拥有的出牌思考时间(毫秒)
         int m_handOutTimeLimit = 100000000;
#else
        // 抢底阶段显示手牌的延迟(毫秒)
        int m_touchCardDelay = 1;
        // 最后抢底阶段有的思考时间（毫秒）
        int m_lastBidDelay = 10000;
        // 抢底阶段：庄家埋底思考时间
        int m_bidBuryBottomDelay = 30000;
        // 亮牌思考时间（毫秒）
        int m_showCardDelay = 10000;
        // 埋底思考时间（毫秒）
        int m_buryBottomDelay = 30000;
        // 寻友阶段庄家思考时间（毫秒）
        int m_findFriendDelay = 10000;
        // 寻友结束后，如果庄家有找朋友，则停留几秒让所有玩家看清楚信号牌
        int m_findFriendLingerDelay = 2000;
        // 4 个玩家都出牌后的延迟（毫秒）
        int m_clearPlayCardDelay = 1500;
        // 玩家拥有的出牌思考时间(毫秒)
        int m_handOutTimeLimit = 30000;
        // 最后亮底牌的延迟；毫秒
        int m_showBottomDelay = 5000;
#endif

        // 庄家埋底计时器
        Stopwatch m_bidBuryStopwatch;

        // 炒底新增亮牌数组
        Card[] m_addFryCards;

        // 炒底阶段，玩家爱选择埋下的底牌
        Card[] m_buryCards;
        // 炒底阶段，玩家是否跟牌
        bool m_isFollow;
        // 亮牌的计时器
        Stopwatch m_showCardStopwatch;
        // 埋底的计时器
        Stopwatch m_buryCardStopwatch;

        // 寻友阶段计时器
        Stopwatch m_findFriendStopwatch;

        // 停留计时器
        Stopwatch m_findFriendLingerStopwatch;

        // 用于实现最后一个出牌后延迟清空牌桌的计时器
        Stopwatch m_clearPlayCardStopwatch;

        // 标志已经完成一次延时
        bool m_doneClearPlayCardDelay;

        // 出牌数组
        Card[] m_dealCards;

        // 出牌计时器
        Stopwatch m_handOutStopwatch;

        // 上一次检查时的出牌玩家 id
        // int m_lastCheckHandOutPlayerId;
        // 标志可以开始出牌思考计时
        bool m_isOkCountDown;

        // 指示完成一次游戏主循环的事件
        private AutoResetEvent m_doneGameLoopEvent = new AutoResetEvent(false);
        public AutoResetEvent doneGameLoopEvent
        {
            get { return m_doneGameLoopEvent; }
        }

        // 表示接收到接待线程的 waitingCustomerEvent 事件
        private AutoResetEvent m_receivedEvent = new AutoResetEvent(false);
        public AutoResetEvent receivedEvent
        {
            get { return m_receivedEvent; }
        }


        // 房主的 ID；-1 表示房间里面还没有人，还没有房主
        int m_roomMasterId = -1;
        // 玩家的准备状态；房主默认是已经准备就绪的；m_playerReadyStates[2] = true 表示玩家 ID=2 准备就绪
        bool[] m_playerReadyStates = new bool[Dealer.playerNumber];

        // 统计信息表名
        string statTableName = "stat";
        // 等级-头衔对应表名
        string levelTitleTableName = "level_title";
        // 用户信息表名
        string userTableName = "user";

        const int defaultRound = 5;
        // 游戏的局数；默认是 5 局
        int m_gameRoundSetting = defaultRound;

        // 存储逃跑玩家的名字
        List<string> m_runPlayerNames = new List<string>();

        //static string m_statTableName = "stat";

        // 记录断线玩家的用户名；主线程会根据这个记录来修改逃跑次数
        List<string> abnormallyDisconnectedUserNames = new List<string>();
        // 分发给玩家的 ID 卡；玩家连接上服务器后，发他一个；如果他断线了，我们就收回来，待会发给其他人
        List<int> m_idCards;

        // 数据库客户端
        DBClient m_DBClient;

        // 检查数据库连接辅助计时器
        Stopwatch m_checkConnectStopwatch;
        // 检查数据库连接延时（毫秒）
        int m_checkConnectDelay = 3000;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="roomId">房间 ID</param>
        public RoomService(string roomId)
        {
            // 获取房间 ID
            m_id = roomId;
            // 初始化用户 ID 卡
            m_idCards = new List<int>(Enumerable.Range(0, Dealer.playerNumber));
            // 初始化客户端列表
            m_players = new List<Player>();
        }

        public void SetOk2Loop()
        {
            m_isOk2Loop = true;
        }

        /// <summary>
        /// 假定这房间还没有满人；接待处用来往这个房间塞人的接口
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(Player player)
        {
            if (m_players.Count >= Dealer.playerNumber)
            {
                throw new Exception(string.Format("房间 {0} 已经满人了，却还往里面塞人", m_id));
            }
            // 首先为该玩家分配房间内 ID
            player.id = DistributeId();
            // 然后将这个玩家加入到玩家列表
            m_players.Add(player);
            // 将玩家列表按照 ID 由小到大排序
            //m_players.Sort((player1, player2) => player1.id - player2.id);
            // 向该玩家发送分配的 ID
            player.Respond(player.id);
            // 向该玩家发送此房间 ID
            player.Respond(m_id);
            MyConsole.Log(string.Format("用户 {0} 进入房间 {1}；ID = {2}", player.name, m_id, player.id));
        }

        // 获取房间内人数
        public int GetPlayerNumber()
        {
            return m_players.Count;
        }

        /// <summary>
        /// 获知该房间是否已经满人
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return m_players.Count >= Dealer.playerNumber;
        }

        // 分配 ID 卡
        int DistributeId()
        {
            if (m_idCards.Count > 0)
            {
                int id = m_idCards.Min();
                m_idCards.Remove(id);
                return id;
            }
            else
            {
                throw new Exception("尝试发 ID 卡，然而早就发完了");
            }

        }

        // 回收 ID 卡
        void RecycleId(int id)
        {
            int idx = m_idCards.IndexOf(id);
            if (idx >= 0)
            {
                throw new Exception("尝试回收还没分发的 ID 卡");
            }
            else
            {
                m_idCards.Add(id);
            }
        }

        /// <summary>
        /// 服务器响应客户端
        /// </summary>
        /// <param name="playerId">要响应的玩家 ID</param>
        /// <param name="sth">响应的内容</param>
        /// <param name="channel">频道</param>
        /// <returns>从客户端收到的数据</returns>
        object Respond(int playerId, object sth = null, int channel = 0)
        {
            //MyConsole.Log(playerId.ToString());

            Player thisPlayer = m_players.Find(player => player.id == playerId);

            Message message;
            try
            {
                if (sth == null)
                {
                    message = thisPlayer.Respond(new Message("", channel));
                }
                else
                {
                    message = thisPlayer.Respond(new Message(sth, channel));
                }
            }
            // 可能是断线了
            catch (AggregateException ex)
            {
                //// Get stack trace for the exception with source file information
                //var st = new StackTrace(ex, true);

                //StackFrame[] frames = st.GetFrames();

                //for (int i = 0; i < frames.Length; i++)
                //{
                //    MyConsole.Log(frames[i].ToString());
                //}

                //MyConsole.Log(ex.Message);
                throw new Exception(string.Format("玩家 {0} 断线了", thisPlayer.name));
            }
            return message.data;
        }

        /// <summary>
        /// 向所有客户端广播；用 4 个线程并行的做
        /// </summary>
        /// <param name="sth">要广播的内容</param>
        /// <param name="channel">广播频道</param>
        void Broadcast(object sth, int channel = 0)
        {
            Task[] tasks = new Task[m_players.Count];
            for (int i = 0; i < m_players.Count; i++)
            {
                int id = m_players[i].id;
                tasks[i] = new Task(() => Respond(id, sth, channel));
                tasks[i].Start();
            }
            try
            {
                // 等待所有发送操作完成
                Task.WaitAll(tasks);
            }
            // 可能是其中一个客户端断线了；注意要用 AggregateException，捕获所有异常，可能有多个，只抓一个可能会导致崩溃
            catch (AggregateException ae)
            {

                throw ae.Flatten();
            }
            //for (int i = 0; i < tasks.Length; i++)
            //{
            //    try
            //    {
            //        tasks[i].Wait();
            //    }
            //    catch (AggregateException ae)
            //    {
            //        throw ae.Flatten();
            //    }
            //}

            //for (int i = 0; i < m_players.Count; i++)
            //{
            //    try
            //    {
            //      Respond(m_players[i].id, sth, channel);
            //    }
            //    // 出现断线
            //    catch
            //    {
            //        throw;
            //    }
            //}
        }

        /// <summary>
        /// 将不同消息分发给不同玩家；按照 ID 从小到大的顺序来分发
        /// </summary>
        /// <param name="things"></param>
        /// <param name="channel"></param>
        void Scatter(object[] things, int channel = 0)
        {
            Task[] tasks = new Task[m_players.Count];
            List<int> ids = m_players.ConvertAll(player => player.id);
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                object thisThing = things[i];
                tasks[i] = new Task(() => Respond(id, thisThing, channel));
                tasks[i].Start();
            }
            try
            {
                // 等待所有发送操作完成
                Task.WaitAll(tasks);
            }
            // 可能是其中一个客户端断线了
            catch (AggregateException ae)
            {
                throw ae.Flatten();

            }

            //for (int i = 0; i < m_players.Count; i++)
            //{
            //    try
            //    {
            //        Task.Run(() => Respond(m_players[i].id, things[i], channel));
            //    }
            //    // 出现断线
            //    catch
            //    {
            //        throw;
            //    }
            //}
        }

        /// <summary>
        /// 从不同玩家处接收不同消息；按照 ID 从小到大的顺序收集消息
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>各客户端的消息</returns>
        object[] Collect(int channel = 0)
        {
            Task<object>[] tasks = new Task<object>[m_players.Count];
            List<int> ids = m_players.ConvertAll(player => player.id);
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
            {
                int id = ids[i];
                tasks[i] = new Task<object>(() => Respond(id, new object(), channel));
                tasks[i].Start();
            }
            try
            {
                // 等待所有发送操作完成
                Task.WaitAll(tasks);
            }
            // 可能是其中一个客户端断线了
            catch (AggregateException ae)
            {
                throw ae.Flatten();

            }
            //object[] objs=Array.ConvertAll(tasks, task => task.Result);
            ////object[] objs = new object[m_players.Count];
            //for (int i = 0; i < m_players.Count; i++)
            //{
            //    try
            //    {
            //        objs[i] = Task.Run(() => Respond(m_players[i].id, "", channel)).Result;
            //    }
            //    // 出现断线
            //    catch
            //    {
            //        throw;
            //    }
            //}
            return Array.ConvertAll(tasks, task => task.Result);
        }

        /// <summary>
        /// 紧急广播；忽略断线的玩家；频道 1 是紧急频道
        /// </summary>
        /// <param name="sth">广播的内容</param>
        void EmergencyBroadcast(object sth)
        {
            Task[] tasks = new Task[m_players.Count];
            for (int i = 0; i < m_players.Count; i++)
            {
                int id = i;
                tasks[i] = new Task(() => Respond(id, sth, 1));
                tasks[i].Start();
            }

            try
            {
                // 等待所有发送操作完成
                Task.WaitAll(tasks);
            }
            catch (AggregateException ae)
            {
            }

            //for (int i = 0; i < m_players.Count; i++)
            //{
            //    try
            //    {
            //        //Respond(m_players[i].id, sth, 1);
            //        Task.Run(() => Respond(m_players[i].id, sth, 1));
            //    }
            //    // 出现断线
            //    catch
            //    {
            //        // 忽略断线的人
            //        continue;
            //    }
            //}
        }

        // 结束对某个玩家的服务，断开连接
        // 
        void EndService(int id)
        {
            Player thisPlayer = m_players.Find(client => client.id == id);
            // 断开连接
            thisPlayer.socket.Close();
            // 回收 ID 卡
            RecycleId(id);
            //Console.WriteLine("已断开用户" + thisPlayer.name + "的连接");
            MyConsole.Log("已断开用户" + thisPlayer.name + "的连接", /*"ComServer",*/ MyConsole.LogType.Debug);

            m_players.RemoveAll(client => client.id == id);
        }

        /// <summary>
        /// 检查有没有主动关闭连接的同学，关闭掉线同学的连接；可以加并行性
        /// </summary>
        /// <returns>检查出来的断线玩家个数</returns>
        int HandleDisconnect()
        {
            List<int> endServiceIds = new List<int>();

            Task[] tasks = new Task[m_players.Count];
            for (int i = 0; i < m_players.Count; i++)
            {
                Player player = m_players[i];
                int id = player.id;
                string name = player.name;
                tasks[i] = new Task(() =>
                {
                    try
                    {
                        // 询问该玩家是否主动断开连接
                        bool wantDisconnect = (bool)Respond(id, "收到断线请求");
                        // 如果该玩家主动断线
                        if (wantDisconnect)
                        {
                            try
                            {
                                // 询问该客户端是否正常断线
                                bool isNormal = (bool)Respond(id, "收到断线状态");
                                // 如果不是正常断线
                                if (!isNormal)
                                {
                                    // 记名字
                                    abnormallyDisconnectedUserNames.Add(name);
                                }
                            }
                            catch
                            {
                                // 断线了
                                // 记名字
                                abnormallyDisconnectedUserNames.Add(name);
                            }

                            // 终止对他的服务
                            //EndService(id);
                            endServiceIds.Add(id);
                            return;
                            //continue;
                        }
                    }
                    catch
                    {
                        // 有可能这里断线了
                        // 终止对他的服务
                        //EndService(id);
                        endServiceIds.Add(id);
                        abnormallyDisconnectedUserNames.Add(name);
                        return;
                        //continue;
                    }

                    // 如果这个用户掉线了
                    if (player.IsDisconnected())
                    {
                        // 终止对他的服务
                        //EndService(id);
                        endServiceIds.Add(id);
                        // 向所有玩家发送此消息，channel 3 是紧急频道
                        //Broadcast(name, 3);
                        abnormallyDisconnectedUserNames.Add(name);
                        //continue;
                        return;
                    }
                    //else
                    //{
                    //    i++;
                    //}
                });
                tasks[i].Start();
            }
            Task.WaitAll(tasks);
            // 取消所有断线用户的服务
            endServiceIds = endServiceIds.Distinct().ToList();
            int count = endServiceIds.Count;
            for (int i = 0; i < endServiceIds.Count; i++)
            {
                EndService(endServiceIds[i]);
            }

            // 紧急将断线人数发送到所有客户端
            //EmergencyBroadcast(count);

            //if (count > 0)
            //{
            //    throw new Exception("有玩家断线");
            //}
            return count;
        }

        /// <summary>
        /// 重置服务器
        /// </summary>
        void Reset()
        {
            playersIsConnected = new bool[Dealer.playerNumber];
            // 新建荷官
            m_dealer = new Dealer();
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
            // 新建亮底牌计时器
            m_showBottomStopwatch = new Stopwatch();
            // 新建暂存手牌数组
            m_tempHandCards = new List<Card>[Dealer.playerNumber];

            MyConsole.Log("服务器重置完毕");
        }

        //// 网络通信线程的事件
        // ManualResetEvent[] m_comServerEvents;
        void Initialize()
        {
            playersIsConnected = new bool[Dealer.playerNumber];

            //m_comServerEvents = new ManualResetEvent[2];
            //m_comServerEvents[0] = waitingCustomerEvent;
            //m_comServerEvents[1] = doneHandleDisconnect;

            m_playersWaitLine = new List<PlayerInfo>();

            // 新建玩家列表
            m_playerInfos = new List<PlayerInfo>();
            // 新建用户统计信息接口
            //m_userStats = new List<StatObject>();
            //m_userStat = new StatObject();

            //m_players = new Player[Dealer.playerNumber];
            //m_players = new List<PlayerInfo>();
            //m_ip = IPAddress.Parse(m_ipStr);
            //m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //m_socket.Bind(new IPEndPoint(m_ip, m_port));
            ////m_socket.Listen(Dealer.playerNumber);
            //m_socket.Listen(m_backlog);

            //Console.WriteLine("启动监听{0}成功", m_socket.LocalEndPoint.ToString());

            // 初始化用户 ID 卡
            //m_idCards = new List<int>(Enumerable.Range(0, m_roomSize));

            // 新建荷官
            m_dealer = new Dealer();
            // 新建游戏状态机
            m_gameStateMachine = new GameStateMachine();
            // 设置游戏状态机的状态为准备阶段
            m_gameStateMachine.Update(GameStateMachine.State.GetReady);
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
            // 新建亮底牌计时器
            m_showBottomStopwatch = new Stopwatch();
            // 新建暂存手牌数组
            m_tempHandCards = new List<Card>[Dealer.playerNumber];
            //for(int i = 0; i < Dealer.playerNumber; i++)
            //{
            //    m_tempHandCards[i] = new List<Card>();
            //}

            m_checkConnectStopwatch = new Stopwatch();
            // 启动检查数据库连接计时器
            m_checkConnectStopwatch.Start();

            // 初始化 Bmob

#if (DATABASE)
            m_DBClient = new DBClient();
            // 初始化数据库客户端
            m_DBClient.RegisterLogger(MyConsole.Log);
            m_DBClient.Initialize();
            // 设置 10 秒的连接数据库超时，增大房间连接上数据库的几率
            m_DBClient.Connect(10000);
#endif
            MyConsole.Log("房间初始化完成", /*"Program",*/ MyConsole.LogType.Debug);
        }

        // 准备开始游戏
        // 带等候区的玩家进入房间
        void GetReady()
        {
            // 如果有玩家中途离开了
            if (m_players.Count < m_playerInfos.Count)
            {
                // 重置服务器
                Reset();

                List<int> temp = m_players.ConvertAll(player => player.id);

                m_playerInfos.RemoveAll(info => temp.IndexOf(info.id) < 0);
            }
            // 如果玩家信息和玩家网络信息不一致
            else if (m_players.Count > m_playerInfos.Count)
            {
                // 找到没有加入的玩家信息
                List<int> temp = m_playerInfos.ConvertAll(player => player.id);
                foreach (var thisPlayer in m_players.FindAll(player => temp.IndexOf(player.id) < 0))
                {
                    m_playerInfos.Add(new PlayerInfo(thisPlayer.name, thisPlayer.id));
#if (DATABASE)
                    // 从数据库中获取该用户的信息
                    DataObject dataObj = m_DBClient.Find(statTableName, thisPlayer.name);
                    StatObject statObj;
                    // 如果没有记录，说明这是一个新用户
                    if (dataObj == null)
                    {
                        statObj = new StatObject();
                        statObj.username = thisPlayer.name;
                        // 更新到数据库
                        m_DBClient.Update(statTableName, statObj);
                    }
                    // 如果有记录，老玩家了
                    else
                    {
                        statObj = new StatObject(dataObj);
                    }
                    // 更新到玩家信息当中
                    statObj.CopyTo(m_playerInfos.Last());
#endif
                }

            }

            //m_playerInfos.Clear();


            //            // 首先更新主程序掌握的玩家列表
            //            // 对每一个网络交流中的玩家
            //            for (int i = 0; i < m_players.Count; i++)
            //            {
            //                Player comPlayer = m_players[i];
            //                //// 检查是否已经更新到主程序
            //                //int idx = m_players.FindIndex(player => player.id == comPlayer.id);
            //                //// 如果还没有更新到主程序
            //                //if (idx < 0)
            //                //{
            //                //    // 加入此玩家
            //                //    m_players.Add(new PlayerInfo(comPlayer.name, comPlayer.id));
            //                //    //// 加入此玩家的统计数据
            //                //    //m_userStats.Add(new StatObject(comPlayer.name));
            //                //    //// 将统计数据同步到玩家信息中
            //                //    //m_userStats.Last().CopyTo(m_players.Last());
            //                //}

            //                m_playerInfos.Add(new PlayerInfo(comPlayer.name, comPlayer.id));

            //#if (DATABASE)
            //                // 从数据库中获取该用户的信息
            //                DataObject dataObj = m_DBClient.Find(statTableName, comPlayer.name);
            //                StatObject statObj;
            //                // 如果没有记录，说明这是一个新用户
            //                if (dataObj == null)
            //                {
            //                    statObj = new StatObject();
            //                    statObj.username = comPlayer.name;
            //                    // 更新到数据库
            //                    m_DBClient.Update(statTableName, statObj);
            //                }
            //                // 如果有记录，老玩家了
            //                else
            //                {
            //                    statObj = new StatObject(dataObj);
            //                }
            //                // 更新到玩家信息当中
            //                statObj.CopyTo(m_playerInfos.Last());
            //#endif
            //            }


            //// 接收玩家发过来的玩家信息
            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    PlayerInfo thisPlayerInfo = (PlayerInfo)Respond(m_playerInfos[i].id, "收到玩家信息");

            //    // 复制基本玩家信息
            //    //m_players[i].nickname = thisPlayerInfo.nickname;
            //    m_playerInfos[i].CopyBasicInfoFrom(thisPlayerInfo);
            //}

            // 接收玩家发过来的玩家信息；注意是按照 ID 顺序从小到大收集消息的
            object[] infos = Collect();
            List<int> ids = m_playerInfos.ConvertAll(info => info.id);
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
            {
                int idx = m_playerInfos.FindIndex(info => info.id == ids[i]);
                m_playerInfos[idx].CopyBasicInfoFrom((PlayerInfo)infos[i]);
            }


            //// 对每一个主程序中的玩家
            //for (int i = 0; i < m_players.Count; i++)
            //{
            //    //PlayerInfo thisPlayer = m_players[i];
            //    string name = m_players[i].name;
            //    // 检查是否多余（说明该玩家已经断线）
            //    int idx = players.FindIndex(player => player.name == name);
            //    // 如果该玩家不在网络交流中，说明他已经断线了
            //    if (idx < 0)
            //    {
            //        // 移除掉线玩家
            //        m_players.RemoveAll(player => player.name == name);
            //        //m_userStats.RemoveAll(stat => stat.username == name);
            //    }
            //}


            // 更新完成
            //try
            //{
            // 发送当前在线人数
            Broadcast(m_playerInfos.Count);

            // 把现在在线的所有玩家的所有信息发送给所有其他人
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                Broadcast(m_playerInfos[i]);
            }
            //}
            //catch
            //{
            //    throw;
            //}

            // 如果还没有在线玩家
            if (m_playerInfos.Count == 0)
            {
                // 重置房主 ID
                m_roomMasterId = -1;
            }
            // 如果已经有在线玩家
            else
            {
                // 最早进入房间的玩家做房主
                m_roomMasterId = m_playerInfos[0].id;
            }

            // 将房主 ID 发送到客户端
            Broadcast(m_roomMasterId);

            // 检查各游戏玩家的准备状态
            for (int i = 0; i < m_playerReadyStates.Length; i++)
            {
                int idx = m_playerInfos.FindIndex(player => player.id == i);
                // 如果该 ID 的玩家在线
                if (idx >= 0)
                {
                    m_playerReadyStates[i] = (bool)Respond(i, "收到准备状态");
                }
                // 否则，当作没有准备好
                else
                {
                    m_playerReadyStates[i] = false;
                }
            }
            // 将各玩家的状态广播给客户端
            Broadcast(m_playerReadyStates);

            // 如果有房主
            if (m_roomMasterId >= 0)
            {
                // 从房主处接收踢出房间玩家 ID
                int kickedPlayerId = (int)Respond(m_roomMasterId, "收到要踢玩家 ID");
                // 向所有玩家发送被踢玩家 ID
                Broadcast(kickedPlayerId);


                // 检查房主是否有更新游戏局数
                bool updatedRoundSetting = (bool)Respond(m_roomMasterId, "收到局数更新");
                // 如果房主的确更新了游戏局数
                if (updatedRoundSetting)
                {
                    // 接收房主更新的局数
                    m_gameRoundSetting = (int)Respond(m_roomMasterId, "收到最新局数");
                }
                // 广播房主是否更新局数
                Broadcast(updatedRoundSetting);
                // 如果房主的确更新了游戏局数
                if (updatedRoundSetting)
                {
                    // 广播最新的游戏局数
                    Broadcast(m_gameRoundSetting);
                }

            }
            // 广播最新的游戏局数
            Broadcast(m_gameRoundSetting);

            // 如果房间里面没人了；服务器该睡觉了
            if (m_playerInfos.Count == 0)
            {
                // 重置一下服务器
                // 设置默认局数
                m_gameRoundSetting = defaultRound;
                // 设置没有房主
                m_roomMasterId = -1;
            }

        }

        void Ready2Deal()
        {
            // 过渡更新
            m_dealer.Ready2Deal();
            //// 更新玩家的级数
            //for (int i = 0; i < m_dealer.playerLevels.Length; i++)
            //{
            //    m_dealer.playerLevels[i] = m_players[i].level = 1;
            //}

            // 将玩家级数同步到客户端
            Broadcast(m_dealer.playerLevels);

            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    obj = Respond(i, true);
            //}
            Broadcast(true);

        }

        void DealCards()
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
                //Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
            }
            // 初始化排序家
            m_cardSorter = new CardSorter(m_dealer);
            // 向客户端发送一次排序家
            Broadcast(m_cardSorter);

            // 广播当前局数
            Broadcast(m_dealer.round);
            // 广播分数；一局开始，必须要清零；清零已经在score2deal做了

        }

        void Deal2Bid()
        {
            // 过渡更新
            m_dealer.Deal2Bid();

            // 开始计时
            m_touchCardStopwatch.Restart();

            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    // 向客户端发送当前台上方的 ID
            //    // 如果只有 1 个台上方, 则跳过摸牌效果; 否则, 才需要模拟摸牌
            //    //Respond(m_players[i].socket, m_dealer.upperPlayersId);
            //    obj = Respond(i, m_dealer.upperPlayersId);

            //    //// 发送玩家当前的等级
            //    ////Respond(m_players[i].socket, m_players[i].playerInfo.level);
            //    //obj = Respond(i, m_dealer.playerLevels[i]);

            //}

            Broadcast(m_dealer.upperPlayersId);

            m_handCardShowNumber = 0;

            // 这时还不能确定主级数？因为有可能有多个台上方？
            // 姑且先这样：如果台上方只有一个，则更新主级数；否则，留到后面确定
            m_dealer.UpdateMainDeal();

            // 如果当前是首盘
            if (m_dealer.round == 1)
            {
                // 设置所有玩家的级数为 1
                for (int i = 0; i < m_playerInfos.Count; i++)
                {
                    //m_players[i].level = 1;
                    m_dealer.playerLevels[i] = 1;
                }
            }
            //// 将玩家级数同步到荷官（包括上一局更新的级数）
            //for (int i = 0; i < m_dealer.playerLevels.Length; i++)
            //{
            //    m_dealer.playerLevels[i] = m_players[i].level;
            //}

            // 广播玩家级数
            Broadcast(m_dealer.playerLevels);

#if (DEBUG_BID_1)
            // 如果在调试抢底
            // 试试其他台上方和级数
            m_dealer.playerLevels[1] = 6;
            m_dealer.playerLevels[3] = 3;
#endif
        }

        // 处理抢底流程
        // 返回是否结束摸牌
        bool Touch()
        {
            bool isOk;
            // 如果台上方不止一个
            if (m_dealer.upperPlayersId.Length > 1)
            {
                // 是否所有服务器都已经显示当前要显示的那一张牌，即它们是否已经跟上节奏
                bool isFollowed = true;

                //for (int i = 0; i < m_playerInfos.Count; i++)
                //{
                //    // 向 4 个客户端发送当前应该显示的牌数
                //    //Respond(m_players[i].socket, m_handCardShowNumber);
                //    // 向 4 个客户端发送当前玩家的手牌
                //    //Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
                //    //obj = Respond(i, Card.ToInt(m_players[i].cardInHand));
                //    obj = Respond(i, Card.ToInt(m_dealer.playersHandCard[i]));

                //    try
                //    {
                //        // 检查客户端是否跟上步伐
                //        //isFollowed &= (bool)Respond(m_players[i].socket, "收到");
                //        obj = Respond(i, "收到");

                //        //isFollowed &= (bool)Respond(i, "收到");
                //        isFollowed &= (bool)obj;
                //    }
                //    catch
                //    {
                //        throw;
                //    }


                //    // 向该玩家发送当前可以亮牌的花色
                //    //Respond(m_players[i].socket, m_dealer.GetLegalBidColors(i/*,m_handCardShowNumber*/));
                //    Respond(i, m_dealer.GetLegalBidColors(i/*,m_handCardShowNumber*/));


                //    // 接收每个玩家的亮牌决定
                //    //bool isClickClub = (bool)Respond(m_players[i].socket, "收到");
                //    //bool isClickDiamond = (bool)Respond(m_players[i].socket, "收到");
                //    //bool isClickHeart = (bool)Respond(m_players[i].socket, "收到");
                //    //bool isClickSpade = (bool)Respond(m_players[i].socket, "收到");
                //    bool isClickClub = (bool)Respond(i, "收到");
                //    bool isClickDiamond = (bool)Respond(i, "收到");
                //    bool isClickHeart = (bool)Respond(i, "收到");
                //    bool isClickSpade = (bool)Respond(i, "收到");

                //    // 获取当前玩家需要增加的亮牌数
                //    //int bidNeedNumber = m_dealer.BidNeedNumber(i);
                //    // 根据现在的情况，更新该玩家的亮牌和手牌
                //    if (isClickClub)
                //    {
                //        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Club);
                //    }
                //    else if (isClickDiamond)
                //    {
                //        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Diamond);
                //    }
                //    else if (isClickHeart)
                //    {
                //        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Heart);
                //    }
                //    else if (isClickSpade)
                //    {
                //        m_dealer.BidHelper(i,/* m_handCardShowNumber, */Card.Suit.Spade);
                //    }
                //    else// 如果玩家还没有决定亮牌
                //    {

                //    }
                //    // 亮完牌之后，更新玩家的手牌
                //    //m_players[i].cardInHand = m_dealer.playersHandCard[i];
                //    // 将该玩家的手牌发送到客户端
                //    //Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
                //    //Respond(i, Card.ToInt(m_players[i].cardInHand));
                //    Respond(i, Card.ToInt(m_dealer.playersHandCard[i]));

                //}
                //obj = Respond(i, Card.ToInt(m_dealer.playersHandCard[i]));
                // 向 4 个客户端发送当前玩家的手牌
                Scatter(Array.ConvertAll(m_dealer.playersHandCard, handcard => Card.ToInt(handcard)));
                // 检查客户端是否跟上步伐
                Broadcast(new object());
                // 向该玩家发送当前可以亮牌的花色
                Scatter(Array.ConvertAll(Enumerable.Range(0, m_players.Count).ToArray(), i => m_dealer.GetLegalBidColors(i)));
                // 接收每个玩家的亮牌决定
                bool[] isClickClub = Array.ConvertAll(Collect(), obj => (bool)obj);
                bool[] isClickDiamond = Array.ConvertAll(Collect(), obj => (bool)obj);
                bool[] isClickHeart = Array.ConvertAll(Collect(), obj => (bool)obj);
                bool[] isClickSpade = Array.ConvertAll(Collect(), obj => (bool)obj);

                for (int i = 0; i < m_players.Count; i++)
                {
                    // 根据现在的情况，更新该玩家的亮牌和手牌
                    if (isClickClub[i])
                    {
                        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Club);
                    }
                    else if (isClickDiamond[i])
                    {
                        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Diamond);
                    }
                    else if (isClickHeart[i])
                    {
                        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Heart);
                    }
                    else if (isClickSpade[i])
                    {
                        m_dealer.BidHelper(i,/* m_handCardShowNumber, */Card.Suit.Spade);
                    }
                    else// 如果玩家还没有决定亮牌
                    {

                    }
                }
                // 向 4 个客户端发送当前玩家的手牌
                Scatter(Array.ConvertAll(m_dealer.playersHandCard, handcard => Card.ToInt(handcard)));

                //for (int i = 0; i < m_playerInfos.Count; i++)
                //{
                //    // 将当前亮牌玩家的 ID 和亮牌发送到所有玩家
                //    for (int j = 0; j < m_dealer.currentBidCards.Length; j++)
                //    {
                //        //Respond(m_players[i].socket, j);
                //        //Respond(m_players[i].socket, Card.ToInt(m_dealer.currentBidCards[j]));
                //        Respond(i, j);
                //        Respond(i, Card.ToInt(m_dealer.currentBidCards[j]));
                //    }
                //}

                // 将当前亮牌玩家的 ID 和亮牌发送到所有玩家
                for (int j = 0; j < m_dealer.currentBidCards.Length; j++)
                {
                    Broadcast(j);
                    Broadcast(Card.ToInt(m_dealer.currentBidCards[j]));
                }

                // 获取当前应该显示的手牌数
                //int handCardShowNumber = (int)m_touchCardStopwatch.ElapsedMilliseconds / m_touchCardDelay;
                // 如果计时器显示已经超过显示手牌延时，而且所有客户端都表示已经跟上步伐

                if (m_touchCardStopwatch.ElapsedMilliseconds > m_touchCardDelay && isFollowed)
                {
                    for (int i = 0; i < m_playerInfos.Count; i++)
                    {
                        // 准备显示下一张牌
                        m_dealer.playersHandCard[i].Add(m_tempHandCards[i][0]);
                        m_tempHandCards[i].RemoveAt(0);
                    }
                    //m_handCardShowNumber++;
                    // 重启计时器
                    //m_touchCardStopwatch.Reset();
                    //m_touchCardStopwatch.Start();
                    m_touchCardStopwatch.Restart();
                }

                // 如果暂存手牌数组均为空，则说明摸牌已经结束
                isOk = true;
                for (int i = 0; i < m_tempHandCards.Length; i++)
                {
                    isOk &= m_tempHandCards[i].Count == 0;
                }
            }
            else// 如果台上方只有一个
            {
                // 直接把手牌发送到各个玩家
                for (int i = 0; i < m_playerInfos.Count; i++)
                {
                    //m_players[i].cardInHand = m_tempHandCards[i];
                    m_dealer.playersHandCard[i] = m_tempHandCards[i];
                    // 将该玩家的手牌发送到客户端
                    //Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
                    //Respond(i, Card.ToInt(m_players[i].cardInHand));
                    //Respond(i, Card.ToInt(m_dealer.playersHandCard[i]));
                }
                // 向 4 个客户端发送当前玩家的手牌
                Scatter(Array.ConvertAll(m_dealer.playersHandCard, handcard => Card.ToInt(handcard)));

                //// 并指定是此台上方玩家抢到底牌
                //m_dealer.gotBottomPlayerId = m_dealer.upperPlayersId[0];
                //// 记录此台上方抢到底牌
                //m_dealer.BidTimes[m_dealer.upperPlayersId[0]]++;

                isOk = true;

            }

            //// 如果摸牌数达到手牌数, 说明摸牌已经结束
            //return m_handCardShowNumber >= Dealer.cardInHandInitialNumber;

            return isOk;
        }

        // 处理摸牌到最后抢底过渡阶段
        void Touch2LastBid()
        {
            // 过渡更新
            m_dealer.Touch2LastBid();
            // 重新开始计时
            m_lastBidStopwatch.Restart();
        }

        // 处理最后抢底阶段
        // 返回是否超时
        bool LastBid()
        {
            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    // 向 4 个客户端发送当前的剩余时间（秒）
            //    //Respond(m_players[i].socket, (m_lastBidDelay - (int)m_lastBidStopwatch.ElapsedMilliseconds) / 1000);
            //    Respond(i, (m_lastBidDelay - (int)m_lastBidStopwatch.ElapsedMilliseconds) / 1000);

            //    // 向 4 个客户端发送当前玩家的手牌
            //    //Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
            //    Respond(i, Card.ToInt(m_dealer.playersHandCard[i]));

            //    // 向该玩家发送当前可以亮牌的花色
            //    //Respond(m_players[i].socket, m_dealer.GetLegalBidColors(i/*,m_handCardShowNumber*/));
            //    Respond(i, m_dealer.GetLegalBidColors(i/*,m_handCardShowNumber*/));


            //    // 接收每个玩家的亮牌决定
            //    //bool isClickClub = (bool)Respond(m_players[i].socket, "收到");
            //    //bool isClickDiamond = (bool)Respond(m_players[i].socket, "收到");
            //    //bool isClickHeart = (bool)Respond(m_players[i].socket, "收到");
            //    //bool isClickSpade = (bool)Respond(m_players[i].socket, "收到");

            //    bool isClickClub = (bool)Respond(i, "收到");
            //    bool isClickDiamond = (bool)Respond(i, "收到");
            //    bool isClickHeart = (bool)Respond(i, "收到");
            //    bool isClickSpade = (bool)Respond(i, "收到");

            //    // 根据现在的情况，更新该玩家的亮牌和手牌
            //    if (isClickClub)
            //    {
            //        m_dealer.BidHelper(i, /*m_handCardShowNumber, */Card.Suit.Club);
            //    }
            //    else if (isClickDiamond)
            //    {
            //        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Diamond);
            //    }
            //    else if (isClickHeart)
            //    {
            //        m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Heart);
            //    }
            //    else if (isClickSpade)
            //    {
            //        m_dealer.BidHelper(i, /*m_handCardShowNumber,*/ Card.Suit.Spade);
            //    }
            //    else// 如果玩家还没有决定亮牌
            //    {

            //    }
            //    // 更新玩家的手牌
            //    m_dealer.playersHandCard[i] = m_dealer.playersHandCard[i];
            //    // 将该玩家的手牌发送到客户端
            //    //Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
            //    Respond(i, Card.ToInt(m_dealer.playersHandCard[i]));

            //}

            Broadcast((m_lastBidDelay - (int)m_lastBidStopwatch.ElapsedMilliseconds) / 1000);
            // 向 4 个客户端发送当前玩家的手牌
            Scatter(Array.ConvertAll(m_dealer.playersHandCard, handcard => Card.ToInt(handcard)));
            // 向该玩家发送当前可以亮牌的花色
            Scatter(Array.ConvertAll(Enumerable.Range(0, m_players.Count).ToArray(), i => m_dealer.GetLegalBidColors(i)));
            // 接收每个玩家的亮牌决定
            bool[] isClickClub = Array.ConvertAll(Collect(), obj => (bool)obj);
            bool[] isClickDiamond = Array.ConvertAll(Collect(), obj => (bool)obj);
            bool[] isClickHeart = Array.ConvertAll(Collect(), obj => (bool)obj);
            bool[] isClickSpade = Array.ConvertAll(Collect(), obj => (bool)obj);

            for (int i = 0; i < m_players.Count; i++)
            {
                // 根据现在的情况，更新该玩家的亮牌和手牌
                if (isClickClub[i])
                {
                    m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Club);
                }
                else if (isClickDiamond[i])
                {
                    m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Diamond);
                }
                else if (isClickHeart[i])
                {
                    m_dealer.BidHelper(i,/* m_handCardShowNumber,*/ Card.Suit.Heart);
                }
                else if (isClickSpade[i])
                {
                    m_dealer.BidHelper(i,/* m_handCardShowNumber, */Card.Suit.Spade);
                }
                else// 如果玩家还没有决定亮牌
                {

                }
            }
            // 向 4 个客户端发送当前玩家的手牌
            Scatter(Array.ConvertAll(m_dealer.playersHandCard, handcard => Card.ToInt(handcard)));

            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    // 将当前亮牌玩家的 ID 和亮牌发送到所有玩家
            //    for (int j = 0; j < m_dealer.currentBidCards.Length; j++)
            //    {
            //        //Respond(m_players[i].socket, j);
            //        //Respond(m_players[i].socket, Card.ToInt(m_dealer.currentBidCards[j]));

            //        Respond(i, j);
            //        Respond(i, Card.ToInt(m_dealer.currentBidCards[j]));
            //    }
            //}

            // 将当前亮牌玩家的 ID 和亮牌发送到所有玩家
            for (int j = 0; j < m_dealer.currentBidCards.Length; j++)
            {
                Broadcast(j);
                Broadcast(Card.ToInt(m_dealer.currentBidCards[j]));
            }

            return m_lastBidDelay < m_lastBidStopwatch.ElapsedMilliseconds;
        }

        // 处理最后抢底到埋底的过渡阶段
        void LastBid2BidBury()
        {
            // 过渡更新
            m_dealer.LastBid2BidBury();

            // 在最后摸牌结束后，将玩家的亮牌重新放回到手牌当中
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                m_dealer.playersHandCard[i].AddRange(m_dealer.currentBidCards[i]);
                // 将手牌发送到客户端
                // 向 4 个客户端发送当前玩家的手牌
                //Respond(m_players[i].socket, Card.ToInt(m_players[i].playerInfo.cardInHand));
                //Respond(i, Card.ToInt(m_dealer.playersHandCard[i]));

                //// 将庄家 ID 发送给所有玩家
                ////Respond(m_players[i].socket, m_dealer.gotBottomPlayerId);
                //Respond(i, m_dealer.gotBottomPlayerId);
            }

            // 向 4 个客户端发送当前玩家的手牌
            Scatter(Array.ConvertAll(m_dealer.playersHandCard, handcard => Card.ToInt(handcard)));
            // 将庄家 ID 发送给所有玩家
            Broadcast(m_dealer.gotBottomPlayerId);

            // 将底牌发给庄家
            //Respond(m_players[m_dealer.gotBottomPlayerId].socket, Card.ToInt(m_dealer.bottom));
            Respond(m_dealer.gotBottomPlayerId, Card.ToInt(m_dealer.bottom));

            // 并将底牌加入到庄家手牌
            //m_players[m_dealer.gotBottomPlayerId].cardInHand.AddRange(m_dealer.bottom);

            m_dealer.AddBottom(m_dealer.gotBottomPlayerId);

            //MyConsole.Log("排序之前");
            //List<Card> temp = new List<Card>(m_players[m_dealer.gotBottomPlayerId].cardInHand);
            //Card.PrintDeck(temp);
            //m_cardSorter.Sort(ref temp, m_dealer.gotBottomPlayerId, GameStateMachine.State.BidBury);
            //MyConsole.Log("排序之后");
            //Card.PrintDeck(temp);
            //// 如果是首盘
            //if (m_dealer.round == 1)
            //{
            //    // 抢到底牌方即为做庄
            //    m_dealer.bankerPlayerId.Add(m_dealer.gotBottomPlayerId);
            //}

            // 启动埋底计时器
            m_bidBuryStopwatch.Restart();


            // 设置庄家
            // 首盘牌抢到底牌方即为做庄；这个可能要炒底来handle一下
            m_dealer.UpdateBankerBid();

            // 将庄家 ID 发送到客户端
            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    Respond(i, m_dealer.bankerPlayerId.ToArray());
            //}

            // 将庄家 ID 发送到客户端
            Broadcast(m_dealer.bankerPlayerId.ToArray());

            // 抢底阶段结束，更新主牌信息
            m_dealer.UpdateMainBid();
        }

        // 抢底阶段：处理庄家埋底
        bool BidBury()
        {
            Judgement judgement;
            bool isOk;

            bool isTimeOut = m_bidBuryStopwatch.ElapsedMilliseconds > m_bidBuryBottomDelay;

            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    // 发送庄家剩余时间
            //    //Respond(m_players[i].socket, (m_bidBuryBottomDelay - (int)m_bidBuryStopwatch.ElapsedMilliseconds) / 1000);
            //    Respond(i, (m_bidBuryBottomDelay - (int)m_bidBuryStopwatch.ElapsedMilliseconds) / 1000);

            //}
            Broadcast((m_bidBuryBottomDelay - (int)m_bidBuryStopwatch.ElapsedMilliseconds) / 1000);
            // 告知当前炒底玩家是否超时
            //Respond(m_players[m_dealer.gotBottomPlayerId].socket, isTimeOut);
            Respond(m_dealer.gotBottomPlayerId, isTimeOut);


            // 检查当前亮牌玩家是否需要提示
            //bool isNeedTips = (bool)Respond(m_players[m_dealer.gotBottomPlayerId].socket, "收到");
            bool isNeedTips = (bool)Respond(m_dealer.gotBottomPlayerId, "收到");

            // 检查是否开启代理
            //bool isAutoPlay = (bool)Respond(m_players[m_dealer.gotBottomPlayerId].socket, "收到");
            bool isAutoPlay = (bool)Respond(m_dealer.gotBottomPlayerId, "收到");


            // 如果玩家需要埋底提示，或者开启代理，或者已经超时
            if (isNeedTips || isAutoPlay || isTimeOut)
            {
                // 从荷官处获取亮牌提示
                Card[] buryCardTips = m_dealer.AutoBuryCard(m_dealer.gotBottomPlayerId);
                // 将亮牌提示返回到客户端
                //Respond(m_players[m_dealer.gotBottomPlayerId].socket, Card.ToInt(buryCardTips));
                Respond(m_dealer.gotBottomPlayerId, Card.ToInt(buryCardTips));

            }

            // 接收当前炒底玩家要埋的底牌
            //m_buryCards = Card.ToCard((int[])Respond(m_players[m_dealer.gotBottomPlayerId].socket, "收到出牌"));
            m_buryCards = Card.ToCard((int[])Respond(m_dealer.gotBottomPlayerId, "收到出牌"));

            // 如果玩家有埋底
            if (m_buryCards.Length > 0)
            {
                Console.WriteLine("玩家 id=" + m_dealer.gotBottomPlayerId + "正在埋牌");
                // 检验埋底的合法性
                judgement = m_dealer.IsLegalBury(m_buryCards, m_dealer.gotBottomPlayerId);
                // 向客户端发送埋底合法性
                //Respond(m_players[m_dealer.gotBottomPlayerId].socket, judgement);
                Respond(m_dealer.gotBottomPlayerId, judgement);
                // 如果玩家所埋的牌合法
                if (judgement.isValid)
                {
                    //// 将埋牌从玩家手牌中去除
                    //for (int i = 0; i < m_buryCards.Length; i++)
                    //{
                    //    m_players[m_dealer.gotBottomPlayerId].cardInHand.Remove(m_buryCards[i]);
                    //}
                    //// 将埋牌放到底牌
                    //m_dealer.bottom = m_buryCards;

                    m_dealer.BuryCards(m_dealer.gotBottomPlayerId, m_buryCards);

                    // 统计埋底次数
                    m_dealer.buryTimes[m_dealer.gotBottomPlayerId]++;
                    // 统计埋底分数
                    m_dealer.buryScore[m_dealer.gotBottomPlayerId] += m_dealer.countScore(m_dealer.CardArrayToCardList(m_buryCards));


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

        // 没人抢底，重新发牌抢底
        void Bid2Deal()
        {
            // 过渡更新
            m_dealer.Bid2Deal();
            // 清空玩家手牌；因为可能没人抢底，要重新发牌
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                m_dealer.playersHandCard[i].Clear();
            }
        }

        void Bid2Fry()
        {
            // 暂停并重置计时器
            m_touchCardStopwatch.Stop();
            m_touchCardStopwatch.Reset();
            // 重置计数器
            m_handCardShowNumber = 0;

            //try
            //{
            // 过渡更新
            m_dealer.Bid2Fry();
            //}
            //catch(Exception e)
            //{
            //MyConsole.Log(e.Message);
            //}

            //// 重置炒底亮牌数下界
            //m_dealer.fryCardLowerBound = 0;
            //// 设置当前庄家的下一个玩家为第一个炒底的玩家
            //// 因为庄家已经买过底了
            //m_dealer.currentFryPlayerId = (m_dealer.gotBottomPlayerId + 1) % Dealer.playerNumber;
            //// 清空荷官存储的炒底阶段亮牌
            //m_dealer.ClearShowCards();
            // 重启炒底阶段亮牌计时器
            m_showCardStopwatch.Restart();

        }

        // 处理炒底流程
        // 测试：最多只能出 5 张牌；只能出比别人多；可以选择不跟
        // 处理炒底阶段亮牌流程
        // 返回：亮牌阶段是否结束；玩家要么成功亮牌，要么不跟
        bool FryShow()
        {
            // 亮牌合法性判决
            Judgement judgement;
            // 亮牌阶段是否结束
            bool isOk;

            // 当前玩家是否已经超时
            bool isTimeOut = m_showCardStopwatch.ElapsedMilliseconds > m_showCardDelay;
            // 当前玩家有没有亮牌
            bool hasShow = m_dealer.showCards[m_dealer.currentFryPlayerId].Count > 0;


            //for (int j = 0; j < m_playerInfos.Count; j++)
            //{
            //    // 向所有玩家发送当前炒底玩家 ID
            //    //Respond(m_players[j].socket, m_dealer.currentFryPlayerId);

            //    Respond(j, m_dealer.currentFryPlayerId);

            //    // 发送玩家先前有没有亮牌
            //    Respond(j, hasShow);

            //    // 发送当前炒底玩家剩余思考时间
            //    //Respond(m_players[j].socket, (m_showCardDelay - (int)m_showCardStopwatch.ElapsedMilliseconds) / 1000);

            //    Respond(j, (m_showCardDelay - (int)m_showCardStopwatch.ElapsedMilliseconds) / 1000);

            //}

            Broadcast(m_dealer.currentFryPlayerId);
            Broadcast(hasShow);
            Broadcast((m_showCardDelay - (int)m_showCardStopwatch.ElapsedMilliseconds) / 1000);

            // 告知当前炒底玩家是否超时
            //Respond(m_players[m_dealer.currentFryPlayerId].socket, isTimeOut);

            Respond(m_dealer.currentFryPlayerId, isTimeOut);
            // 如果当前玩家有亮牌
            if (hasShow)
            {
                // 把当前亮牌玩家之前的亮牌重新加回去他的手牌里去
                //m_players[m_dealer.currkentFryPlayerId].cardInHand.AddRange(m_dealer.showCards[m_dealer.currentFryPlayerId]);

                // 清空亮牌
                //m_dealer.showCards[m_dealer.currentFryPlayerId].Clear();

                m_dealer.ReturnShowCards(m_dealer.currentFryPlayerId);

                // 把手牌发到客户端
                //Respond(m_dealer.currentFryPlayerId, Card.ToInt(m_players[m_dealer.currentFryPlayerId].cardInHand));
                Respond(m_dealer.currentFryPlayerId, Card.ToInt(m_dealer.playersHandCard[m_dealer.currentFryPlayerId]));

            }
            // 如果当前玩家没有亮牌
            else
            {

            }

            // 如果超时了
            if (isTimeOut)
            {
                // 认为玩家不跟
                m_isFollow = false;
            }
            else
            {
                // 询问客户端玩家是否选择不跟
                //m_isFollow = (bool)Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
                m_isFollow = (bool)Respond(m_dealer.currentFryPlayerId, "收到");

            }

            // 如果玩家选择跟牌
            if (m_isFollow)
            {
                // 检查当前亮牌玩家是否需要提示
                //bool isNeedTips = (bool)Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
                bool isNeedTips = (bool)Respond(m_dealer.currentFryPlayerId, "收到");

                // 检查当前炒底玩家是否开启代理
                //bool isAutoPlay = (bool)Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
                //bool isAutoPlay = (bool)Respond(m_dealer.currentFryPlayerId, "收到");


                // 如果玩家需要亮牌提示
                if (isNeedTips)
                {
                    // 从荷官处获取亮牌提示
                    Card[] showCardTips = m_dealer.AutoAddShowCard(m_dealer.currentFryPlayerId);
                    // 将亮牌提示返回到客户端
                    //Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(showCardTips));
                    Respond(m_dealer.currentFryPlayerId, Card.ToInt(showCardTips));

                }
                //// 如果玩家开启了代理
                //if (isAutoPlay)
                //{
                //    // 从荷官处获取亮牌提示
                //    Card[] showCardTips = m_dealer.AutoAddShowCard(m_dealer.currentFryPlayerId);
                //    // 将亮牌提示返回到客户端
                //    //Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(showCardTips));
                //    Respond(m_dealer.currentFryPlayerId, Card.ToInt(showCardTips));

                //}

                // 接收当前炒底玩家的新增亮牌
                //m_addFryCards = Card.ToCard((int[])Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到出牌"));
                m_addFryCards = Card.ToCard((int[])Respond(m_dealer.currentFryPlayerId, "收到出牌"));

                // 如果玩家有出牌
                if (m_addFryCards.Length > 0)
                {
                    Console.WriteLine("玩家 id=" + m_dealer.currentFryPlayerId + ", 正在亮牌");
                    // 交给荷官检查
                    judgement = m_dealer.IsLegalShow(m_addFryCards, m_dealer.currentFryPlayerId);
                    // 先返回判断结果
                    //Respond(m_players[m_dealer.currentFryPlayerId].socket, judgement);
                    Respond(m_dealer.currentFryPlayerId, judgement);

                    // 如果合法
                    if (judgement.isValid)
                    {
                        Console.WriteLine("玩家 id=" + m_dealer.currentFryPlayerId + " 成功亮牌，准备埋底");

                        //// 将亮牌加入筹码
                        //m_dealer.showCards[m_dealer.currentFryPlayerId].AddRange(m_addFryCards);
                        //// 将亮牌从玩家手牌中去除
                        //for (int i = 0; i < m_addFryCards.Length; i++)
                        //{
                        //    m_players[m_dealer.currentFryPlayerId].cardInHand.Remove(m_addFryCards[i]);
                        //}

                        m_dealer.ShowCards(m_dealer.currentFryPlayerId, m_addFryCards);

                        //计算炒底次数
                        m_dealer.fryTimes[m_dealer.currentFryPlayerId]++;


                        // 客户端会自行将亮牌从手牌中去除，不用从服务器发送过去了
                        // 更新炒底的筹码下界
                        m_dealer.fryCardLowerBound = m_dealer.showCards[m_dealer.currentFryPlayerId].Count;
                        // 重置不跟的玩家个数
                        m_dealer.skipFryCount = 0;
                        //更新荷官的最后炒底亮牌
                        m_dealer.lastFryShowCard = m_addFryCards[0];
                        // 更新最后亮牌玩家 ID
                        m_dealer.lastFryShowPlayerId = m_dealer.currentFryPlayerId;
                        // 增加一个人完成炒底亮牌
                        m_dealer.UpdateFryShowHistory();
                        // 将该玩家的亮牌点数存放到记录里面
                        m_dealer.showPointsHistory.Add(m_addFryCards[0].points);
                        // 存储更新庄家信息
                        m_dealer.StoreBankerIdHistory(m_addFryCards[0].points);
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
                //for (int j = 0; j < m_playerInfos.Count; j++)
                //{
                //    //Respond(m_players[j].socket, isOk);
                //    Respond(j, isOk);

                //}
                Broadcast(isOk);
                if (isOk)
                {
                    // 向客户端发送当前玩家最新的亮牌
                    //for (int j = 0; j < m_playerInfos.Count; j++)
                    //{
                    //    //Respond(m_players[j].socket, Card.ToInt(m_dealer.showCards[m_dealer.currentFryPlayerId].ToArray()));
                    //    Respond(j, Card.ToInt(m_dealer.showCards[m_dealer.currentFryPlayerId].ToArray()));
                    //}
                    Broadcast(Card.ToInt(m_dealer.showCards[m_dealer.currentFryPlayerId].ToArray()));
                }
                else
                {

                }
            }
            else// 否则，如果玩家不跟
            {
                isOk = true;
                // 向所有客户端发送状态，告知不要接收出牌
                //for (int j = 0; j < m_playerInfos.Count; j++)
                //{
                //    //Respond(m_players[j].socket, false);
                //    Respond(j, false);
                //}
                Broadcast(false);
                // 不跟也是完成炒底亮牌
                //m_dealer.fryMoves++;
                m_dealer.UpdateFryShowHistory();
            }
            return isOk;
        }

        // 处理炒底阶段亮牌到埋底过渡阶段
        void FryShow2Bury()
        {
            // 过渡更新
            m_dealer.FryShow2Bury();

            // 告知客户端当前炒底玩家没有跟牌
            //Respond(m_players[m_dealer.currentFryPlayerId].socket, m_isFollow);
            Respond(m_dealer.currentFryPlayerId, m_isFollow);

            // 如果玩家有跟牌
            if (m_isFollow)
            {
                // 将底牌发送给亮牌成功的玩家
                // 向当前炒底玩家发送底牌
                //Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(m_dealer.bottom));
                Respond(m_dealer.currentFryPlayerId, Card.ToInt(m_dealer.bottom));

                // 将底牌加入到炒底玩家的手牌当中去
                //m_players[m_dealer.currentFryPlayerId].cardInHand.AddRange(m_dealer.bottom);

                m_dealer.AddBottom(m_dealer.currentFryPlayerId);
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
        bool FryBury()
        {
            Judgement judgement;
            bool isOk;

            // 当前玩家是否已经超时
            bool isTimeOut = m_buryCardStopwatch.ElapsedMilliseconds > m_buryBottomDelay;

            //for (int j = 0; j < m_playerInfos.Count; j++)
            //{
            //    // 发送当前炒底玩家剩余思考时间
            //    //Respond(m_players[j].socket, (m_buryBottomDelay - (int)m_buryCardStopwatch.ElapsedMilliseconds) / 1000);
            //    Respond(j, (m_buryBottomDelay - (int)m_buryCardStopwatch.ElapsedMilliseconds) / 1000);

            //}
            Broadcast((m_buryBottomDelay - (int)m_buryCardStopwatch.ElapsedMilliseconds) / 1000);
            // 告知当前炒底玩家是否超时
            //Respond(m_players[m_dealer.currentFryPlayerId].socket, isTimeOut);
            Respond(m_dealer.currentFryPlayerId, isTimeOut);

            // 检查当前亮牌玩家是否需要提示
            //bool isNeedTips = (bool)Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
            bool isNeedTips = (bool)Respond(m_dealer.currentFryPlayerId, "收到");

            // 检查是否开启代理
            //bool isAutoPlay = (bool)Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到");
            bool isAutoPlay = (bool)Respond(m_dealer.currentFryPlayerId, "收到");


            // 如果玩家需要埋底提示，或者开启代理，或者已经超时
            if (isNeedTips || isAutoPlay || isTimeOut)
            {
                // 从荷官处获取亮牌提示
                Card[] buryCardTips = m_dealer.AutoBuryCard(m_dealer.currentFryPlayerId);
                // 将亮牌提示返回到客户端
                //Respond(m_players[m_dealer.currentFryPlayerId].socket, Card.ToInt(buryCardTips));
                Respond(m_dealer.currentFryPlayerId, Card.ToInt(buryCardTips));

            }
            // 接收当前炒底玩家要埋的底牌
            //m_buryCards = Card.ToCard((int[])Respond(m_players[m_dealer.currentFryPlayerId].socket, "收到出牌"));
            m_buryCards = Card.ToCard((int[])Respond(m_dealer.currentFryPlayerId, "收到出牌"));

            // 如果玩家有埋底
            if (m_buryCards.Length > 0)
            {
                Console.WriteLine("玩家 id=" + m_dealer.currentFryPlayerId + "正在埋牌");
                // 检验埋底的合法性
                judgement = m_dealer.IsLegalBury(m_buryCards, m_dealer.currentFryPlayerId);
                // 向客户端发送埋底合法性
                //Respond(m_players[m_dealer.currentFryPlayerId].socket, judgement);
                Respond(m_dealer.currentFryPlayerId, judgement);

                // 如果玩家所埋的牌合法
                if (judgement.isValid)
                {
                    //// 将埋牌从玩家手牌中去除
                    //for (int i = 0; i < m_buryCards.Length; i++)
                    //{
                    //    m_players[m_dealer.currentFryPlayerId].cardInHand.Remove(m_buryCards[i]);
                    //}
                    //// 将埋牌放到底牌
                    //m_dealer.bottom = m_buryCards;

                    m_dealer.BuryCards(m_dealer.currentFryPlayerId, m_buryCards);

                    // 统计埋底次数
                    m_dealer.buryTimes[m_dealer.currentFryPlayerId]++;
                    // 统计埋底分数
                    m_dealer.buryScore[m_dealer.currentFryPlayerId] += m_dealer.countScore(m_dealer.CardArrayToCardList(m_buryCards));

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
        void FryBury2Show()
        {
            // 过渡更新
            m_dealer.FryBury2Show();
            // 重启亮牌计时器
            m_showCardStopwatch.Restart();
        }

        /// <summary>
        /// 炒底到找朋友过渡阶段
        /// </summary>
        void Fry2FindFriend()
        {
            // 炒底阶段结束，更新主牌信息
            m_dealer.UpdateMainFry();

            // 发送主花色和主级数
            Broadcast(m_dealer.GetMainLevel());
            Broadcast((int)m_dealer.GetMainSuit());

            // 过渡更新
            m_dealer.Fry2FindFriend();

            // 重启寻友计时器
            m_findFriendStopwatch.Restart();
            // TODO：炒底确定庄家
            // DONE

            for (int j = 0; j < m_playerInfos.Count; j++)
            {
                // 将炒底的亮牌重新放到玩家的手牌里去
                //m_players[j].cardInHand.AddRange(m_dealer.showCards[j]);

                m_dealer.ReturnShowCards(j);

                // 向玩家发送手牌
                //Respond(m_players[j].socket, Card.ToInt(m_players[j].playerInfo.cardInHand/*.ToArray()*/));
                //Respond(j, Card.ToInt(m_players[j].cardInHand/*.ToArray()*/));
                //Respond(j, Card.ToInt(m_dealer.playersHandCard[j]));

            }
            // 向玩家发送手牌
            Scatter(Array.ConvertAll(m_dealer.playersHandCard, handcard => Card.ToInt(handcard)));

            //// 清空亮牌
            //for (int j = 0; j < m_players.Count; j++)
            //{
            //    m_dealer.showCards[j].Clear();
            //}

            // 更新庄家
            m_dealer.UpdateBankerFry();

            // 将庄家 ID 发送到客户端
            //for (int i = 0; i < m_playerInfos.Count; i++)
            //{
            //    Respond(i, m_dealer.bankerPlayerId.ToArray());
            //}
            Broadcast(m_dealer.bankerPlayerId.ToArray());
        }

        /// <summary>
        /// 找朋友阶段
        /// </summary>
        /// <returns>是否完成找朋友</returns>
        bool FindFriend()
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
            //for (int j = 0; j < m_playerInfos.Count; j++)
            //{
            //    // 发送庄家剩余思考时间
            //    //Respond(m_players[j].socket, (m_findFriendDelay - (int)m_findFriendStopwatch.ElapsedMilliseconds) / 1000);
            //    Respond(j, (m_findFriendDelay - (int)m_findFriendStopwatch.ElapsedMilliseconds) / 1000);

            //}
            Broadcast((m_findFriendDelay - (int)m_findFriendStopwatch.ElapsedMilliseconds) / 1000);
            // 告知庄家是否超时
            //Respond(m_players[bankerId].socket, isTimeOut);
            Respond(bankerId, isTimeOut);

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
                //bool isFindFriend = (bool)Respond(m_players[bankerId].socket, "收到");
                bool isFindFriend = (bool)Respond(bankerId, "收到");

                // 检查庄家是否选择单打
                //bool isAlone = (bool)Respond(m_players[bankerId].socket, "收到");
                bool isAlone = (bool)Respond(bankerId, "收到");

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
            //for (int j = 0; j < m_playerInfos.Count; j++)
            //{
            //    //Respond(m_players[j].socket, hasOperation);
            //    Respond(j, hasOperation);

            //}
            Broadcast(hasOperation);
            // 如果庄家有操作了
            if (hasOperation)
            {
                // 告诉所有玩家，庄家是否选择单打
                //for (int j = 0; j < m_playerInfos.Count; j++)
                //{
                //    //Respond(m_players[j].socket, m_dealer.bankerIsFightAlone);
                //    Respond(j, m_dealer.bankerIsFightAlone);

                //}
                Broadcast(m_dealer.bankerIsFightAlone);
                // 如果庄家选择寻友
                if (!m_dealer.bankerIsFightAlone)
                {
                    // 向所有玩家发送信号牌
                    //for (int j = 0; j < m_playerInfos.Count; j++)
                    //{
                    //    //Respond(m_players[j].socket, m_dealer.signCard);
                    //    Respond(j, m_dealer.signCard);
                    //}
                    Broadcast(m_dealer.signCard);

                    // 统计找朋友次数
                    m_dealer.findFriendTimes[bankerId]++;
                }
                // 如果庄家选择单打
                else
                {
                    // 统计单打次数
                    m_dealer.singleTimes[bankerId]++;
                }
            }
            return isOk;
        }


        /// <summary>
        /// 找完朋友到停留过渡阶段
        /// </summary>
        void FindFriend2Linger()
        {
            // 过渡更新
            m_dealer.FindFriend2Linger();

            // 重启计时器
            m_findFriendLingerStopwatch.Restart();
        }

        /// <summary>
        /// 找完朋友的停留阶段
        /// </summary>
        /// <returns>是否完成过渡</returns>
        bool FindFriendLinger()
        {
            return m_findFriendLingerStopwatch.ElapsedMilliseconds > m_findFriendLingerDelay;
        }

        /// <summary>
        /// 找朋友到对战的过渡阶段
        /// </summary>
        void FindFriend2Fight()
        {
            // 过渡更新
            m_dealer.FindFriend2Fight();
            // 标志可以开始计时
            m_isOkCountDown = true;
        }

        /// <summary>
        /// 对战阶段
        /// </summary>
        void Fight()
        {

            // 如果不是第 1 轮出牌，而且所有玩家都出过 1 次牌，而且还没完成 1 次延时
            // 则继续延迟；否则不延迟，正常出牌
            bool isClearDelay = m_dealer.circle > 1 && m_dealer.handOutPlayerCount == 0 && !m_doneClearPlayCardDelay;
            //bool isClearDelaying = isClearDelay && m_clearPlayCardStopwatch.ElapsedMilliseconds < m_clearPlayCardDelay;
            //for (int j = 0; j < m_playerInfos.Count; j++)
            //{
            //    // 通知客户端是否需要延迟清理
            //    //Respond(m_players[j].socket, isClearDelay);
            //    Respond(j, isClearDelay);

            //}
            Broadcast(isClearDelay);
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
                //for (int j = 0; j < m_playerInfos.Count; j++)
                //{
                //    // 通知所有玩家允许出牌者的 id
                //    //Respond(m_players[j].socket, m_dealer.currentPlayerId);
                //    Respond(j, m_dealer.currentPlayerId);

                //    // 通知所有玩家，首家 ID
                //    //Respond(m_players[j].socket, m_dealer.firstHomePlayerId);
                //    Respond(j, m_dealer.firstHomePlayerId);

                //    // 向所有客户端发送剩余思考时间（毫秒）
                //    //Respond(m_players[j].socket, Math.Max(0, (m_handOutTimeLimit - (int)m_handOutStopwatch.ElapsedMilliseconds) / 1000));
                //    Respond(j, Math.Max(0, (m_handOutTimeLimit - (int)m_handOutStopwatch.ElapsedMilliseconds) / 1000));

                //    // 向所有客户端发送当前出牌者是否已经超时思考
                //    //Respond(m_players[j].socket, isTimeOut);
                //    Respond(j, isTimeOut);


                //}
                // 通知所有玩家允许出牌者的 id
                Broadcast(m_dealer.currentPlayerId);
                // 通知所有玩家，首家 ID
                Broadcast(m_dealer.firstHomePlayerId);
                // 向所有客户端发送剩余思考时间（毫秒）
                Broadcast(Math.Max(0, (m_handOutTimeLimit - (int)m_handOutStopwatch.ElapsedMilliseconds) / 1000));
                // 向所有客户端发送当前出牌者是否已经超时思考
                Broadcast(isTimeOut);

                // 检查当前亮牌玩家是否需要提示
                //bool isNeedTips = (bool)Respond(m_players[m_dealer.currentPlayerId].socket, "收到");
                bool isNeedTips = (bool)Respond(m_dealer.currentPlayerId, "收到");

                // 玩家是否开启代理
                //bool isAutoPlay = (bool)Respond(m_players[m_dealer.currentPlayerId].socket, "收到");
                bool isAutoPlay = (bool)Respond(m_dealer.currentPlayerId, "收到");


                // 如果玩家需要亮牌提示
                if (isNeedTips)
                {
                    // 从荷官处获取亮牌提示
                    Card[] dealCardTips = m_dealer.AutoHandOut(/*m_dealer.currentPlayerId*/);
                    // 将亮牌提示返回到客户端
                    //Respond(m_players[m_dealer.currentPlayerId].socket, Card.ToInt(dealCardTips));
                    Respond(m_dealer.currentPlayerId, Card.ToInt(dealCardTips));

                }

                // 如果玩家选择代理；或时间不够
                if (isAutoPlay || isTimeOut)
                {
                    // 自动出牌
                    m_dealCards = m_dealer.AutoHandOut(/*m_dealer.currentPlayerId*/);
                    // 将选牌发到客户端
                    Respond(m_dealer.currentPlayerId, Card.ToInt(m_dealCards));
                }
                else// 如果玩家还有剩余思考时间；也没有选择代理
                {
                    // 接受他选择的出牌
                    //object temp = Serializer.Receive(m_players[m_dealer.currentPlayerId].socket);
                    //Card[] m_dealCards = Card.ToCard((int[])temp);

                    //m_dealCards = Card.ToCard((int[])Respond(m_players[m_dealer.currentPlayerId].socket, "收到出牌"));
                    m_dealCards = Card.ToCard((int[])Respond(m_dealer.currentPlayerId, "收到出牌"));
                }
                Judgement judgement;
                // 如果有出牌
                if (m_dealCards.Length > 0)
                {
                    Console.WriteLine("{0} 的出牌", m_playerInfos[m_dealer.currentPlayerId].username);
                    Card.PrintDeck(m_dealCards);

                    // 用来判断出牌合法性的所有信息都包含在 Dealer 里头
                    judgement = m_dealer.IsLegalDeal(/*m_players.ToArray(), */m_dealCards);
                    Console.Write(judgement.isValid);
                    Console.Write(' ');
                    Console.WriteLine(judgement.message);

                    // 当玩家是自主行动，不是代理出牌
                    //if (!isTimeOut && !isAutoPlay)
                    //{
                    // 先把合法性判断返回客户端
                    // 一定要保证 Receive 和 Send 操作之间, 没有其他网络通信
                    //Respond(m_players[m_dealer.currentPlayerId].socket, judgement);
                    Respond(m_dealer.currentPlayerId, judgement);
                    //}

                    // 如果出牌合法
                    if (judgement.isValid)
                    {
                        // 若压制则更新首家
                        //if (judgement.message == "shot")
                        //{
                        //m_dealer.UpdateFirstHome(m_dealer.currentPlayerId);
                        //}
                        //// 将选牌从手牌中扣除
                        //for (int j = 0; j < m_dealCards.Length; j++)
                        //{
                        //    // 找到选牌在手牌中的位置
                        //    m_players[m_dealer.currentPlayerId].cardInHand.Remove(m_dealCards[j]);
                        //}

                        m_dealer.HandOut(/*m_dealer.currentPlayerId,*/ m_dealCards);

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
                        // 更新庄家
                        m_dealer.UpdateBankerFight(m_dealCards);
                    }
                    // 如果甩牌失败；并且现在是首家出牌
                    else if (judgement.message == "throwFail" && m_dealer.IsFisrtPlayerPlaying())
                    {
                        // 获取玩家出牌中的最小的牌
                        Card minCard = m_dealer.GetMinCard(m_dealCards);
                        // 将这张牌作为首家的出牌
                        m_dealCards = new Card[1] { minCard };
                        // 自动帮他出了这张牌
                        m_dealer.HandOut(m_dealCards);
                        // 更新庄家；可能有出信号牌哦
                        m_dealer.UpdateBankerFight(m_dealCards);
                        // 重置思考计时器
                        m_handOutStopwatch.Reset();
                    }
                    // 向出牌玩家发送手牌
                    // 如果出牌合法，这手牌有所减少；否则，手牌没有改变
                    //Respond(m_players[m_dealer.currentPlayerId].socket, Card.ToInt(m_players[m_dealer.currentPlayerId].playerInfo.cardInHand));
                    //Respond(m_dealer.currentPlayerId, Card.ToInt(m_players[m_dealer.currentPlayerId].cardInHand));
                    Respond(m_dealer.currentPlayerId, Card.ToInt(m_dealer.playersHandCard[m_dealer.currentPlayerId]));

                    //for (int i = 0; i < m_playerInfos.Count; i++)
                    //{
                    //    // 将出牌的处理结果发放给所有玩家
                    //    //Respond(m_players[i].socket, judgement.isValid);
                    //    Respond(i, judgement.isValid);
                    //}

                }
                else// 否则如果玩家还没有出牌
                {
                    //for (int i = 0; i < m_playerInfos.Count; i++)
                    //{
                    //    // 告知所有玩家，当前玩家还没有出牌
                    //    //Respond(m_players[i].socket, false);
                    //    Respond(i, false);

                    //}
                    //Broadcast(false);
                    // 继续计时
                    // 如果已经
                    judgement = new Judgement("玩家还没有出牌", false);

                }
                // 设置是否同步出牌 flag
                bool flag = m_dealCards.Length > 0 && (judgement.isValid || (judgement.message == "throwFail" && m_dealer.IsFisrtPlayerPlaying()));
                // 向所有客户端发送
                Broadcast(flag);
                if (flag)
                {
                    // 向所有玩家发送该玩家的出牌
                    //for (int j = 0; j < m_playerInfos.Count; j++)
                    //{
                    //    // 先发送 ID
                    //    //Respond(m_players[j].socket, m_players[m_dealer.currentPlayerId].playerInfo.id);
                    //    Respond(j, m_playerInfos[m_dealer.currentPlayerId].id);

                    //    // 再发送出牌
                    //    //Respond(m_players[j].socket, Card.ToInt(m_dealCards));
                    //    Respond(j, Card.ToInt(m_dealCards));

                    //    // 发送当前庄家 ID；因为有可能有人出了信号牌，和原来的庄家成了朋友
                    //    Respond(j, m_dealer.bankerPlayerId.ToArray());
                    //}
                    // 先发送 ID
                    Broadcast(m_dealer.currentPlayerId);
                    // 再发送出牌
                    Broadcast(Card.ToInt(m_dealCards));
                    // 发送当前庄家 ID；因为有可能有人出了信号牌，和原来的庄家成了朋友
                    Broadcast(m_dealer.bankerPlayerId.ToArray());
                    // 存储出牌到荷官
                    m_dealer.handOutCards[m_dealer.currentPlayerId] = new List<Card>(m_dealCards);

                    //// 如果最后一个玩家出牌
                    //if (m_dealer.handOutPlayerCount == 0)
                    //{
                    //    // 标志需要 1 次延时
                    //    m_doneClearPlayCardDelay = false;
                    //    // 清空荷官中存储的本轮玩家出牌
                    //    m_dealer.ClearHandOutCards();
                    //    // 进入下一轮出牌
                    //    m_dealer.circle++;
                    //}
                    // 下一玩家出牌
                    m_dealer.handOutPlayerCount++;

                    // 如果最后一个玩家出牌
                    if (m_dealer.handOutPlayerCount == 0)
                    {
                        // 更新首家
                        m_dealer.UpdateFirstHome();

                        // 先计算分数
                        m_dealer.addScore();

                        // 标志需要 1 次延时
                        m_doneClearPlayCardDelay = false;
                        // 清空荷官中存储的本轮玩家出牌
                        m_dealer.ClearHandOutCards();
                        // 进入下一轮出牌
                        m_dealer.circle++;
                        // 最后一轮
                        //if (m_dealer.AllPlayersHandEmpty())
                        //    m_dealer.addLevel();
                        //else



                        //// 将玩家分数更新到主线程
                        //for (int i = 0; i < m_dealer.score.Length; i++)
                        //{
                        //    m_players[i].score = m_dealer.score[i];
                        //}
                    }


                    // 必须保证首家 ID 已经确定了，才能更新下一出牌玩家 ID
                    m_dealer.UpdateNextPlayer();

                    // 将分数同步到客户端
                    Broadcast(m_dealer.score);

                    // 标志可以开始为下一玩家出牌思考计时
                    m_isOkCountDown = true;
                }
                // 否则, 还是同一玩家出牌
                else
                {

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

        /// <summary>
        /// 对战到积分阶段
        /// </summary>
        void Fight2Score()
        {
            // 过渡更新
            m_dealer.Fight2Score();
        }

        // 处理计分流程
        void Score()
        {
            // 更新玩家的级数；更新台上方；注意要用到m_dealer.score
            m_dealer.addLevel();
            //for (int i = 0; i < m_dealer.playerLevels.Length; i++)
            //{
            //    // 更新主线程中玩家的级数
            //    m_players[i].level = m_dealer.playerLevels[i];
            //}
            // 发送新级数到客户端
            Broadcast(m_dealer.playerLevels);
        }

        /// <summary>
        /// 处理积分到亮底牌过渡
        /// </summary>
        void Score2ShowBottom()
        {
            // 向客户端广播底牌；请确保没有清除
            Broadcast(Card.ToInt(m_dealer.bottom));
            // 重启亮底牌计时器
            m_showBottomStopwatch.Restart();
        }

        /// <summary>
        /// 处理亮底牌
        /// </summary>
        /// <returns>是否完成延迟</returns>
        bool ShowBottom()
        {
            return m_showBottomStopwatch.ElapsedMilliseconds > m_showBottomDelay;
        }

        /// <summary>
        /// 处理亮底牌到重新发牌过渡
        /// </summary>
        void ShowBottom2Deal()
        {
#if (DATABASE)
            // 将玩家信息统计之后更新到数据库服务器
            UpdatePlayerStats();
#endif
            // 过渡更新
            m_dealer.ShowBottom2Deal();
            // 清空玩家手牌
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                m_dealer.playersHandCard[i].Clear();
            }

            // 这里同步一下客户端的战绩；直接把玩家信息发送到客户端
            // 先发送玩家人数
            Broadcast(m_playerInfos.Count);
            // 然后发送玩家信息
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                Broadcast(m_playerInfos[i]);
            }

        }

        // 将玩家信息统计之后更新到数据库服务器
        void UpdatePlayerStats()
        {
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                int playerindex = m_playerInfos.FindIndex(player => player.id == i);//第i个玩家的下标

                //m_players[i].UpdateStat();
                m_playerInfos[playerindex].UpdateStat(
                                                    m_dealer.playerLevels[i],//玩家i的级数
                                                    m_dealer.playerAddLevels[i],    // 玩家 i 的升级数
                                                    m_dealer.BidTimes[i],//抢底次数
                                                    m_dealer.fryTimes[i],//炒底次数
                                                    m_dealer.buryTimes[i],//埋底次数
                                                    m_dealer.buryScore[i],//埋底分数
                                                    m_dealer.singleTimes[i],//单打次数
                                                    m_dealer.findFriendTimes[i],
                                                    m_dealer.bankerPlayerId[0] == i,  //当前玩家是否为庄家
                                                    m_dealer.bottomSuccessID == i,  //当前玩家是否为抄底玩家
                                                    m_dealer.bottomSuccessScore,//抄底分数
                                                    Array.IndexOf(m_dealer.upperPlayersId, i) >= 0  // 是否为台上方
                                                    );

                StatObject statObj = new StatObject();
                statObj.CopyFrom(m_playerInfos[i]);

                // 更新数据到数据库服务器
                m_DBClient.Update(statTableName, statObj);

                // 更新用户信息到数据库；只更新用户的积分和头衔
                //UserObject userObj = new UserObject();
                //userObj.CopyFrom(m_players[i]);
                // 更新积分
                //userObj.experience = statObj.grades;
                m_DBClient.Update(userTableName, m_playerInfos[i].username, "experience", statObj.grades);

                // 更新头衔
                DataObject dataObj = m_DBClient.Find(levelTitleTableName, statObj.level.ToString());
                //userObj.title = new LevelTitleObject(dataObj).title;
                m_DBClient.Update(userTableName, m_playerInfos[i].username, "title", new LevelTitleObject(dataObj).title);

                // 更新用户信息到数据库
                //m_DBClient.Update(userTableName, userObj);
            }
        }

        /// <summary>
        /// 积分到重新发牌的过渡阶段
        /// </summary>
        void Score2Deal()
        {
#if (DATABASE)
            // 将玩家信息统计之后更新到数据库服务器
            UpdatePlayerStats();
#endif
            // 过渡更新
            m_dealer.Score2Deal();
            // 清空玩家手牌
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                m_dealer.playersHandCard[i].Clear();
            }

            // 这里同步一下客户端的战绩；直接把玩家信息发送到客户端
            // 先发送玩家人数
            Broadcast(m_playerInfos.Count);
            // 然后发送玩家信息
            for (int i = 0; i < m_playerInfos.Count; i++)
            {
                Broadcast(m_playerInfos[i]);
            }

        }

        // 更新荷官需要掌握的信息
        void UpdateDealer()
        {
            //for (int i = 0; i < m_players.Count; i++)
            //{
            //    // 更新荷官掌握的玩家手牌
            //    m_dealer.playersHandCard[i] = m_players[i].cardInHand;
            //    //// 更新玩家的级数
            //    //m_dealer.playerLevels[i] = m_players[i].level;
            //}
            // 更新台上方玩家
            //m_dealer.UpdateUpperPlayers();

            m_dealer.Update();

            // 用荷官来构造排序家
            m_cardSorter = new CardSorter(m_dealer);

            // 将排序家一股脑的同步到客户端去
            Broadcast(m_cardSorter);

            //// 将荷官要用来排序的必要信息同步到客户端
            //Broadcast(m_dealer.upperPlayersId);
            //Broadcast(m_dealer.playerLevels);
            //Broadcast(m_dealer.mainNumber);
            //Broadcast(m_dealer.mainColor);
        }

        // 检查游戏是否准备好，即
        bool GameIsReady()
        {
            for (int i = 0; i < Dealer.playerNumber; i++)
            {
                // 是否 4 个对战玩家都在线
                if (m_playerInfos.FindIndex(player => player.id == i) < 0)
                {
                    return false;
                }
            }
            // 是否所有玩家都准备好
            for (int i = 0; i < m_playerReadyStates.Length; i++)
            {
                if (!m_playerReadyStates[i])
                {
                    return false;
                }
            }
            return true;
        }

        // 游戏主循环
        void GameLoop()
        {
            bool isOk;
            while (true)
            {
                // 如果没有玩家在房间内
                if (m_players.Count == 0)
                {
                    // 结束游戏循环
                    break;
                }

                // 定时检查房间和数据库的连接
                if (m_checkConnectStopwatch.ElapsedMilliseconds > m_checkConnectDelay)
                {
                    m_DBClient.CheckConnection();
                    // 重启计时器
                    m_checkConnectStopwatch.Restart();
                }

                // 如果这个房间已经满人了，就不可能从接待处来人了；
                // 或者，如果这个房间还没有满人，可能从接待处来人，但是如果前台还在等待玩家连接，那么还是可以运行下去的
                //if ((m_playerInfos.Count == Dealer.playerNumber && m_isOk2Loop) || (m_playerInfos.Count < Dealer.playerNumber && ComServer.waitingCustomerEvent.WaitOne(0)))
                if (ComServer.waitingCustomerEvent.WaitOne(0))
                {
                    m_receivedEvent.Reset();
                    try
                    {
                        // 这时候，虽然断线玩家已经在 ComServer 没有 copy 了，但是还没有同步到主线程
                        // 此时必定还有断线玩家的信息

                        // 首先构造玩家的昵称列表
                        List<string> runPlayerNicknames = new List<string>();

                        for (int i = 0; i < m_runPlayerNames.Count; i++)
                        {
                            runPlayerNicknames.Add(m_playerInfos.Find(player => player.username == m_runPlayerNames[i]).nickname);
                        }

                        // 首先告知客户端，逃跑玩家人数
                        //Broadcast(m_runPlayerNames.Count);
                        // 将逃跑玩家的昵称发送到客户端
                        Broadcast(runPlayerNicknames.ToArray());


                        // 向所有玩家发送最新的游戏状态
                        Broadcast(m_gameStateMachine.state);

                        // 更新荷官掌握的信息
                        UpdateDealer();

                        switch (m_gameStateMachine.state)
                        {
                            // 准备阶段
                            case GameStateMachine.State.GetReady:
                                GetReady();
                                if (GameIsReady())
                                {
                                    m_gameStateMachine.Update(GameStateMachine.Signal.Ready);
                                }
                                break;
                            case GameStateMachine.State.Ready2Deal:
                                Ready2Deal();
                                m_gameStateMachine.Update(GameStateMachine.Signal.DoneReady2Deal);
                                break;
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
                                //if (isOkTouch && noHigherBid)
                                //{
                                //    Console.WriteLine("不可能有更高出价者, 抢底结束");
                                //    m_gameStateMachine.Update(GameStateMachine.Signal.NoHigherBid);
                                //}
                                // 如果摸牌结束，而且还有人可能要亮牌
                                //else if (isOkTouch && !noHigherBid)
                                //{
                                //    // 完成摸牌，准备进入最后抢底阶段
                                //    m_gameStateMachine.Update(GameStateMachine.Signal.DoneTouch);
                                //}

                                // 如果摸牌结束，直接进入最后抢底阶段
                                // 不判断是否还有可以亮牌，以免泄漏信息
                                if (isOkTouch)
                                {
                                    // 如果有多于 1 个台上方
                                    if (m_dealer.upperPlayersId.Length > 1)
                                    {
                                        // 完成摸牌，准备进入最后抢底阶段
                                        m_gameStateMachine.Update(GameStateMachine.Signal.DoneTouch);
                                    }
                                    // 否则，如果只有 1 个台上方
                                    else
                                    {
                                        // 直接进入埋底阶段
                                        m_gameStateMachine.Update(GameStateMachine.Signal.SingleUpperPlayer);
                                    }


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
                                if (/*m_dealer.NoHigherBid() || */isOverTime)
                                {
                                    // 如果有抢底的人
                                    if (m_dealer.hasBidder)
                                    {
                                        // 让庄家埋底
                                        m_gameStateMachine.Update(GameStateMachine.Signal.EndLastBid);
                                    }
                                    // 否则，重新发牌
                                    else
                                    {
                                        m_gameStateMachine.Update(GameStateMachine.Signal.NoBidder);
                                    }
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
                            case GameStateMachine.State.Bid2Deal:
                                Bid2Deal();
                                m_gameStateMachine.Update(GameStateMachine.Signal.DoneBid2Deal);
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
                            case GameStateMachine.State.Score2ShowBottom:
                                Score2ShowBottom();
                                m_gameStateMachine.Update(GameStateMachine.Signal.DoneScore2ShowBottom);
                                break;
                            case GameStateMachine.State.ShowBottom:
                                isOk = ShowBottom();
                                if (isOk)
                                {
                                    m_gameStateMachine.Update(GameStateMachine.Signal.DoneShowBottom);
                                }
                                break;
                            case GameStateMachine.State.ShowBottom2Deal:
                                ShowBottom2Deal();
                                // 如果已经游戏完指定局数
                                if (m_dealer.round > m_gameRoundSetting)
                                {
                                    m_gameStateMachine.Update(GameStateMachine.Signal.FinishRounds);
                                    // 重置服务器
                                    Reset();
                                }
                                // 还没有完成指定局数
                                else
                                {
                                    m_gameStateMachine.Update(GameStateMachine.Signal.FinishOneRound);
                                }
                                break;
                            case GameStateMachine.State.Score2Deal:
                                Score2Deal();
                                // 如果已经游戏完指定局数
                                if (m_dealer.round > m_gameRoundSetting)
                                {
                                    m_gameStateMachine.Update(GameStateMachine.Signal.FinishRounds);
                                    // 重置服务器
                                    Reset();
                                }
                                // 还没有完成指定级数
                                else
                                {
                                    m_gameStateMachine.Update(GameStateMachine.Signal.DoneScore2Deal);
                                }
                                break;
                        }

                    }
                    //catch (AggregateException ae)
                    //{
                    //    // 可能是断线了
                    //    MyConsole.Log("主循环异步通信异常");

                    //    foreach (var ex in ae.InnerExceptions)
                    //    {
                    //        // Get stack trace for the exception with source file information
                    //        var st = new StackTrace(ex, true);

                    //        StackFrame[] frames = st.GetFrames();

                    //        for (int i = 0; i < frames.Length; i++)
                    //        {
                    //            MyConsole.Log(frames[i].ToString());
                    //        }

                    //        MyConsole.Log(ex.Message);
                    //    }

                    //    // 向所有玩家发送紧急通告
                    //    EmergencyBroadcast("有玩家断线");
                    //    // 重新设置游戏为准备中状态
                    //    m_gameStateMachine.Update(GameStateMachine.State.GetReady);

                    //    //// Get the line number from the stack frame
                    //    //var line = frame.GetFileLineNumber();

                    //    //MyConsole.Log(string.Format("{0}行 - {1}", line, ex.Message));
                    //    // 重置服务器
                    //    Reset();

                    //}
                    catch (AggregateException ae)
                    {
                        MyConsole.Log("主循环异常");
                        foreach (var ex in ae.Flatten().InnerExceptions)
                        {
                            // Get stack trace for the exception with source file information
                            var st = new StackTrace(ex, true);

                            StackFrame[] frames = st.GetFrames();

                            for (int i = 0; i < frames.Length; i++)
                            {
                                MyConsole.Log(frames[i].ToString());
                            }

                            MyConsole.Log(ex.Message);
                        }

                        //// 检查一下 socket 里面还有没有消息没读完
                        //for (int i = 0; i < m_players.Count; i++)
                        //{
                        //    MyConsole.Log(string.Format("玩家 ID = {0}；socket 内字节：{1}", m_players[i].id, m_players[i].Receive()));
                        //}


                        // 向所有玩家发送紧急通告
                        //EmergencyBroadcast("有玩家断线");
                        // 重新设置游戏为准备中状态
                        m_gameStateMachine.Update(GameStateMachine.State.GetReady);

                        //// Get the line number from the stack frame
                        //var line = frame.GetFileLineNumber();

                        //MyConsole.Log(string.Format("{0}行 - {1}", line, ex.Message));
                        // 重置服务器
                        Reset();
                    }
                    // 其他异常
                    catch (Exception ex)
                    {
                        MyConsole.Log("主循环异常");
                        // Get stack trace for the exception with source file information
                        var st = new StackTrace(ex, true);

                        StackFrame[] frames = st.GetFrames();

                        for (int i = 0; i < frames.Length; i++)
                        {
                            MyConsole.Log(frames[i].ToString());
                        }

                        MyConsole.Log(ex.Message);
                        m_gameStateMachine.Update(GameStateMachine.State.GetReady);

                        //// Get the line number from the stack frame
                        //var line = frame.GetFileLineNumber();

                        //MyConsole.Log(string.Format("{0}行 - {1}", line, ex.Message));
                        // 重置服务器
                        Reset();
                    }
                    finally
                    {

                    }
                    // 处理断线
                    int disconnectCount = HandleDisconnect();

                    // 紧急广播断线人数
                    try
                    {
                        Broadcast(disconnectCount);
                    }
                    catch
                    {

                    }

                    if (disconnectCount > 0)
                    {
                        // 重新设置游戏为准备中状态
                        m_gameStateMachine.Update(GameStateMachine.State.GetReady);

                        //// Get the line number from the stack frame
                        //var line = frame.GetFileLineNumber();

                        //MyConsole.Log(string.Format("{0}行 - {1}", line, ex.Message));
                        // 重置服务器
                        Reset();
                    }


                    // 向客户端广播其他玩家的逃跑消息
                    // 首先广播逃跑的人数

                    // copy 一份逃跑玩家用户名列表
                    m_runPlayerNames = new List<string>(abnormallyDisconnectedUserNames);
#if (DATABASE)
                    // 处理逃跑；虽然 ComServer 里面已经去掉逃跑玩家的记录；但是主线程此时还没有同步，还保留逃跑玩家的记录
                    while (abnormallyDisconnectedUserNames.Count > 0)
                    //for(int i = 0; i < abnormallyDisconnectedUserNames.Count;)
                    {
                        m_isOk2Loop = false;

                        // 获取逃跑玩家用户名
                        string username = abnormallyDisconnectedUserNames[0];
                        // 获取当前逃跑次数
                        int totalRunTimes = m_playerInfos.Find(player => player.username == username).totalRunTimes;
                        // 更新数据库
                        m_DBClient.Update(statTableName, username, "totalRunTimes", totalRunTimes + 1);
                        // 获取当前积分
                        int grades = m_playerInfos.Find(player => player.username == username).grades;

                        // 扣 3 分更新到数据库；但是不能有负分
                        int level;
                        if (grades > 100)
                        {
                            grades = Math.Max(0, grades - 3);
                            m_DBClient.Update(statTableName, username, "grades", grades);
                            level = PlayerInfo.GetLevel(grades);
                            m_DBClient.Update(statTableName, username, "level", level);
                        }
                        else
                        {
                            // 不扣分了，新手保护
                            level = 1;
                        }

                        // 同时也更新用户信息
                        m_DBClient.Update(userTableName, username, "experience", grades);

                        // 更新头衔
                        DataObject dataObj = m_DBClient.Find(levelTitleTableName, level.ToString());
                        //userObj.title = new LevelTitleObject(dataObj).title;
                        m_DBClient.Update(userTableName, username, "title", new LevelTitleObject(dataObj).title);
                        // 更新分数
                        m_DBClient.Update(userTableName, username, "experience", grades);

                        // 去掉网络部分的异常断线记录
                        abnormallyDisconnectedUserNames.RemoveAt(0);
                    }
#endif
                    // 指示游戏主循环已经结束
                    m_doneGameLoopEvent.Set();
                }
                else
                {
                    //MyConsole.Log("收到开始接待事件");
                    // 指示游戏主循环已经结束
                    m_receivedEvent.Set();
                }
            }
        }

        /// <summary>
        /// 房间服务函数
        /// </summary>
        public void Serve()
        {
            // 设置线程名称
            Thread.CurrentThread.Name = string.Format("房间 {0}", m_id);

            // 初始化一下
            Initialize();

            // 开始游戏主循环
            GameLoop();

            // 断开和数据库连接
            m_DBClient.Disconnect();

            MyConsole.Log(string.Format("房间 {0} 停止服务", id));
            // 标志完成了房间服务
            m_doneService = true;
        }
    }
}
