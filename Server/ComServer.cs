//#define ALI
#undef ALI

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using ConsoleUtility;
using DBNetworking;
using Networking;
using System.Diagnostics;

namespace Server
{
    // 服务器专用交流接口
    // channel = 0 -> 用于正常沟通的网络通信频道
    // channel = 1 -> 用于通知断线的网络通信频道
    // channel = 2 -> 用于登录的网络通信频道

    public class ComServer
    {
        // 服务的端口
        static int m_port = 8885;
        // 服务器用来监听连接的 socket
        static Socket m_socket;
        // 服务器的 IP 地址
        static IPAddress m_ip;
        //static string m_ipStr = "120.24.217.239";
        static string m_ipStr = "172.21.186.34";
        // 挂起的连接队列的最大长度
        static int m_backlog = 1000;
        // 服务器容量
        static int m_capacity;
        // 游戏开始的玩家数目
        static int m_requiredNumber;
        // 分发给玩家的 ID 卡；玩家连接上服务器后，发他一个；如果他断线了，我们就收回来，待会发给其他人
        static List<int> m_idCards;

        // 游戏客户端列表
        static List<Player> m_players;
        public static List<Player> players
        {
            get
            {
                return m_players;
            }
        }

        // 观战客户端列表
        static List<Player> m_watchPlayers = new List<Player>();
        public static List<Player> watchPlayers
        {
            get { return m_watchPlayers; }
        }


        //// 客户端的 socket 列表
        //public static List<Socket> clientSockets = new List<Socket>();
        //// 指示客户端当前是否连接上服务器的标志
        //public static List<bool> clientIsConnected = new List<bool>();
        //// 指示用户 ID 的列表
        //public static List<int> clientIds = new List<int>();

        //// 客户端的 socket 列表
        //public static Socket[] clientSockets;

        //// 指示客户端当前是否连接上服务器的标志
        //public static bool[] clientIsConnected;

        // 断线客户端 ID 列表
        public static List<int> disconnectClientIds;

        // 指示正在等待客户端连接服务器的事件
        private static ManualResetEvent m_waitingCustomerEvent = new ManualResetEvent(false);
        public static ManualResetEvent waitingCustomerEvent
        {
            get { return m_waitingCustomerEvent; }
        }

        // 指示完成接待客户端连接服务器的事件
        private static ManualResetEvent m_doneReceptEvent = new ManualResetEvent(false);
        public static ManualResetEvent doneReceptEvent
        {
            get { return m_doneReceptEvent; }
        }


        // 指示完成处理断线工作的事件
        // 设初始为true，使得首先触发 游戏 线程启动
        private static AutoResetEvent m_doneHandleDisconnect = new AutoResetEvent(true);
        public static AutoResetEvent doneHandleDisconnect
        {
            get { return m_doneHandleDisconnect; }
        }


        private static AutoResetEvent m_doneGameLoopEvent;

        // 游戏准备好的事件
        private static AutoResetEvent m_gameReadyEvent = new AutoResetEvent(false);
        public static AutoResetEvent gameReadyEvent
        {
            get { return m_gameReadyEvent; }
        }

        // 存放游戏进程和断线处理进程
        static AutoResetEvent[] m_events = new AutoResetEvent[2];

        //static string m_statTableName = "stat";

        // 记录断线玩家的用户名；主线程会根据这个记录来修改逃跑次数
        public static List<string> abnormallyDisconnectedUserNames = new List<string>();

        // 房间 ID 列表
        static List<string> m_roomIds = new List<string>();
        // 房间 ID 数位
        static int m_roomIdLength = 6;

        // 房间（服务）列表
        static List<RoomService> m_rooms = new List<RoomService>();

        // 获取本机地址
        static IPAddress GetLocalAddress()
        {

#if (ALI)
            return IPAddress.Parse(Console.ReadLine());
#else
            var host = Dns.GetHostEntry(Dns.GetHostName());

            for (int i = host.AddressList.Length - 1; i >= 0; i--)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    return host.AddressList[i];
                }
            }

            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return ip;
            //    }
            //}
            throw new Exception("没有找到 IP 地址");
#endif

        }


        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="capacity">服务器接收的客户端个数</param>
        public static void Initialize(int requiredNumber, int capacity, AutoResetEvent doneGameLoopEvent)
        {
            //clientIsConnected = new bool[capacity];
            //clientSockets = new Socket[capacity];
            //disconnectClientIds = new List<int>();

            m_capacity = capacity;
            m_requiredNumber = requiredNumber;
            // 初始化客户端列表
            m_players = new List<Player>();
            // 初始化断线客户端 ID
            disconnectClientIds = new List<int>();
            // 初始化 socket
            //m_ip = IPAddress.Parse(m_ipStr);

            // 获取本机地址
            m_ip = GetLocalAddress();

            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Bind(new IPEndPoint(m_ip, m_port));
            m_socket.Listen(m_backlog);
            // 初始化用户 ID 卡
            m_idCards = new List<int>(Enumerable.Range(0, capacity));

            // 初始化房间 ID 列表
            List<int> tempRoomIds = new List<int>(Enumerable.Range(0, (int)Math.Pow(10, m_roomIdLength)));
            // 数字左端端填充 0 
            m_roomIds = tempRoomIds.ConvertAll(id => id.ToString().PadLeft(m_roomIdLength, '0'));

            //Console.WriteLine("启动监听{0}成功", m_socket.LocalEndPoint.ToString());
            MyConsole.Log("启动监听" + m_socket.LocalEndPoint.ToString() + "成功",/* "ComServer",*/ MyConsole.LogType.Debug);
            // 引用传进来的事件
            m_doneGameLoopEvent = doneGameLoopEvent;
            // 存放事件，以备后用
            m_events[0] = m_doneGameLoopEvent;
            m_events[1] = m_doneHandleDisconnect;
        }

        public static void Initialize(int playerNumber)
        {
            m_requiredNumber = playerNumber;
            // 初始化客户端列表
            m_players = new List<Player>();
            // 初始化断线客户端 ID
            disconnectClientIds = new List<int>();
            // 初始化 socket
            //m_ip = IPAddress.Parse(m_ipStr);

            // 获取本机地址
            m_ip = GetLocalAddress();

            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Bind(new IPEndPoint(m_ip, m_port));
            m_socket.Listen(m_backlog);
            // 初始化房间 ID 列表
            List<int> tempRoomIds = new List<int>(Enumerable.Range(0, (int)Math.Pow(10, m_roomIdLength)));
            // 数字左端端填充 0 
            m_roomIds = tempRoomIds.ConvertAll(id => id.ToString().PadLeft(m_roomIdLength, '0'));

            //Console.WriteLine("启动监听{0}成功", m_socket.LocalEndPoint.ToString());
            MyConsole.Log("启动监听" + m_socket.LocalEndPoint.ToString() + "成功",/* "ComServer",*/ MyConsole.LogType.Debug);

        }

        public static object Respond(int playerId, object sth, int channel = 0)
        {
            Player thisPlayer = m_players.Find(player => player.id == playerId);
            // 如果在游戏玩家中间找不到
            if (thisPlayer == null)
            {
                // 那一定要去观战玩家中去找
                thisPlayer = m_watchPlayers.Find(player => player.id == playerId);
            }
            Message message;
            try
            {
                message = thisPlayer.Respond(new Message(sth, channel));
            }
            // 可能是断线了
            catch
            {
                throw;
            }
            return message.data;
        }

        // 向所有客户端广播
        public static void Broadcast(object sth, int channel = 0)
        {
            for (int i = 0; i < m_players.Count; i++)
            {
                // 只对没有锁住的客户端通信
                //if (!m_players[i].isLocked)
                //{
                try
                {
                    Respond(m_players[i].id, sth, channel);
                }
                // 出现断线
                catch
                {
                    throw;
                }
                //}
            }
        }

        public static void EmergencyBroadcast(object sth)
        {
            for (int i = 0; i < m_players.Count; i++)
            {
                // 只对没有锁住的客户端通信
                //if (!m_players[i].isLocked)
                //{
                try
                {
                    Respond(m_players[i].id, sth, 1);
                }
                // 出现断线
                catch
                {
                    // 忽略断线的人
                    continue;
                }
                //}

            }
        }

        // 分配 ID 卡
        static int DistributeId()
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
        static void RecycleId(int id)
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

        // 随机分配房间号码
        static string DistributeRoomId(string id = null)
        {
            if (id == null)
            {
                if (m_roomIds.Count > 0)
                {
                    Random rdn = new Random();
                    int idx = rdn.Next() % m_roomIds.Count;
                    string roomId = m_roomIds[idx];
                    m_roomIds.Remove(roomId);
                    return roomId;
                }
                else
                {
                    throw new Exception("尝试发房间 ID 卡，然而早就发完了");
                }
            }
            else
            {
                int idx = m_roomIds.IndexOf(id);
                if (idx >= 0)
                {
                    m_roomIds.Remove(id);
                    return id;
                }
                else
                {
                    throw new Exception(string.Format("ID {0} 早就被发走了", id));
                }
            }

        }

        // 回收房间号码
        static void RecycleRoomId(string roomId)
        {
            int idx = m_roomIds.IndexOf(roomId);
            if (idx >= 0)
            {
                throw new Exception("尝试回收还没分发的房间 ID 卡");
            }
            else
            {
                m_roomIds.Add(roomId);
            }
        }

        // 开启邮差线程
        static void StartPostman(Player player)
        {
            Thread inPostmanThread = new Thread(new ThreadStart(player.InPostmanJob));
            Thread outPostmanThread = new Thread(new ThreadStart(player.OutPostmanJob));

            inPostmanThread.Name = player.id + "收件邮差";
            outPostmanThread.Name = player.id + "发件邮差";

            // 启动邮差线程
            // 此时必须保证客户端已经连接上服务器
            inPostmanThread.Start();
            outPostmanThread.Start();
            // Spin for a while waiting for the started thread to become
            // alive:
            while (!inPostmanThread.IsAlive) ;
            while (!outPostmanThread.IsAlive) ;

            //m_postManagerThread.Start();
        }

        /// <summary>
        /// 开一个新房间
        /// </summary>
        /// <returns>返回新房间在列表中的下标</returns>
        static int OpenNewRoom(string roomId = null)
        {
            // 如果没有指定要开的房号
            if (roomId == null)
            {
                // 新建房间实例；使用系统分配的房间 ID
                m_rooms.Add(new RoomService(DistributeRoomId()));
                return m_rooms.Count - 1;
            }
            // 如果指定了要开的房号
            else
            {
                // 检查这个房间是否已经开了
                int idx = m_rooms.FindIndex(room => room.id == roomId);
                // 如果这个房间已经开了
                if (idx >= 0)
                {
                    // 那就不用重复开房了
                    return idx;
                }
                // 否则，就真的要开房了
                else
                {
                    // 新建房间实例
                    m_rooms.Add(new RoomService(DistributeRoomId(roomId)));
                    // 去除相应的 ID 卡
                    //m_roomIds.Remove(roomId);
                    return m_rooms.Count - 1;
                }
            }
        }

        /// <summary>
        /// 回收已经没有玩家的房间
        /// </summary>
        static void RecycleRooms()
        {
            // 先获取停止服务的房间的 ID
            List<string> stopServiceRoomId = m_rooms.FindAll(room => room.doneService).ConvertAll(room => room.id);
            // 然后移出所有停止服务的房间
            m_rooms.RemoveAll(room => room.doneService);
            // 然后回收房间 ID
            for (int i = 0; i < stopServiceRoomId.Count; i++)
            {
                MyConsole.Log(string.Format("已回收房间 {0}", stopServiceRoomId[i]));
                RecycleRoomId(stopServiceRoomId[i]);
            }
        }

        // 打开一个新的房间

        // 接待处：等待客户端连接
        public static void WaitClient()
        {
            MyConsole.Log("准备开始接待线程"/*, Thread.CurrentThread.Name*/, MyConsole.LogType.Debug);
            int idx;
            // 服务器还没有满时
            // 测试：多少用户都接；接了再说
            while (true)
            {
                MyConsole.Log("等待客户端接入"/*, Thread.CurrentThread.Name*/, MyConsole.LogType.Debug);

                m_waitingCustomerEvent.Set();
                // 接收客户端连接
                Socket socket = m_socket.Accept();
                MyConsole.Log("接收连接 " + socket.RemoteEndPoint.ToString(),/* Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);

                // 指示已经有客户端连接，准备开始接待事宜
                m_waitingCustomerEvent.Reset();

                // 获取首先触发的事件
                //idx = WaitHandle.WaitAny(m_events);

                //// 等待断线处理工作完成
                //m_doneHandleDisconnect.WaitOne();
                //// 等待游戏流程完成
                //m_doneGameLoopEvent.WaitOne();
                //// 重置
                //m_doneHandleDisconnect.Reset();
                //m_doneGameLoopEvent.Reset();

                // 先回收已经停止服务的房间
                RecycleRooms();

                // 等待所有房间收到开始接待的事件
                for (int i = 0; i < m_rooms.Count; i++)
                {
                    m_rooms[i].receivedEvent.WaitOne();
                }

                // 指示没有完成接待
                m_doneReceptEvent.Reset();

                // 创建玩家实例
                Player thisPlayer = new Player(socket);

                try
                {
                    // 如果玩家数目已经超出范围

                    // 获取玩家的用户名
                    thisPlayer.name = (string)thisPlayer.Respond();

                    // 接收玩家选择的匹配模式
                    int mode = (int)thisPlayer.Respond();
                    // 返回客户端的消息
                    string message;
                    // 返回客户端的结果
                    bool isOk;
                    // 匹配模式代码；0 - 快速匹配；1 - 精准匹配；2 - 开房
                    // 如果是快速匹配模式
                    if (mode == 0)
                    {
                        // 选取一个没有满员的
                        List<RoomService> notFullRooms = m_rooms.FindAll(room => !room.IsFull());
                        bool hasNotFullRoom = notFullRooms.Count > 0;
                        // 告知客户端是否有没有满员的房间
                        thisPlayer.Respond(hasNotFullRoom);
                        // 如果所有房间都已经满员了
                        if (!hasNotFullRoom)
                        {
                            // 那就只能开一个新的房间了
                            idx = OpenNewRoom();
                            // 将此玩家加入指定房间；注意addPlayer里面也有网络通信
                            m_rooms[idx].AddPlayer(thisPlayer);
                            // 设置返回消息
                            message = string.Format("成功进入房间 {0}", m_rooms[idx].id);
                            isOk = true;
                        }
                        // 如果的确有的房间没有满员
                        else
                        {
                            // 最多人的房间
                            int maxPlayerNumber = notFullRooms.Max(room => room.GetPlayerNumber());
                            idx = m_rooms.FindIndex(room => room.GetPlayerNumber() == maxPlayerNumber);
                            // 等待此房间完成一次循环
                            //m_rooms[idx].doneGameLoopEvent.WaitOne();
                            // 将此玩家加入指定房间；注意addPlayer里面也有网络通信
                            m_rooms[idx].AddPlayer(thisPlayer);
                            // 设置返回消息
                            message = string.Format("成功进入房间 {0}", m_rooms[idx].id);
                            isOk = true;
                        }
                    }
                    // 如果是精准匹配模式
                    else if (mode == 1)
                    {
                        // 获取玩家选择的房号，假定是 6 位数字（客户端负责检查）
                        string roomId = (string)thisPlayer.Respond();
                        // 检查一下这个房间开了没有
                        idx = m_rooms.FindIndex(room => room.id == roomId);
                        bool isOpen = idx >= 0;
                        // 告知客户端此房间开了没有
                        thisPlayer.Respond(isOpen);

                        // 如果这个房间开了
                        if (isOpen)
                        {
                            // 检查这房间满人没有
                            bool isFull = m_rooms[idx].IsFull();
                            // 告知客户端此房间满人没有
                            thisPlayer.Respond(isFull);
                            // 如果这个房间是已经满人了
                            if (isFull)
                            {
                                message = string.Format("房间 {0} 已经满员，请选择其他房间", roomId);
                                isOk = false;
                            }
                            // 如果这个房间还没有满人
                            else
                            {
                                // 等待该房间先跑完一次主循环
                                //m_rooms[idx].doneGameLoopEvent.WaitOne();
                                // 将此玩家加入指定房间；注意addPlayer里面也有网络通信
                                m_rooms[idx].AddPlayer(thisPlayer);
                                // 设置返回消息
                                message = string.Format("成功进入房间 {0}", roomId);
                                isOk = true;
                            }
                        }
                        // 如果这个房间还没有开
                        else
                        {
                            // 那就只能开一个新的房间了
                            idx = OpenNewRoom(roomId);
                            // 将此玩家加入指定房间；注意addPlayer里面也有网络通信
                            m_rooms[idx].AddPlayer(thisPlayer);
                            // 设置返回消息
                            message = string.Format("成功进入房间 {0}", roomId);
                            isOk = true;
                        }
                    }
                    // 如果是开房模式
                    else
                    {
                        // 那就只能开一个新的房间了
                        idx = OpenNewRoom();
                        // 将此玩家加入指定房间；注意addPlayer里面也有网络通信
                        m_rooms[idx].AddPlayer(thisPlayer);
                        // 设置返回消息
                        message = string.Format("成功进入房间 {0}", m_rooms[idx].id);
                        isOk = true;
                    }
                    // 向客户端发送服务器返回消息
                    thisPlayer.Respond(message);
                    // 向客户端发送服务器返回结果
                    thisPlayer.Respond(isOk);
                    // 如果玩家成功进入了房间
                    if (isOk)
                    {
                        // 如果这是一个新房间
                        if (m_rooms[idx].GetPlayerNumber() == 1)
                        {
                            // 那么启动房间的服务
                            Thread roomServiceThread = new Thread(new ThreadStart(m_rooms[idx].Serve));
                            roomServiceThread.Start();
                            // 注意不能等待房间服务器启动，因为房间服务也在等待接待线程完成；两相等待，导致死锁
                            //while (roomServiceThread.IsAlive) ;
                        }
                        // 如果这是一个旧房间，接待完成后自动会开始服务
                        else
                        {

                        }
                        m_rooms[idx].SetOk2Loop();
                    }
                    // 如果没有成功进入房间，就断开他的连接
                    else
                    {
                        // 断开和这个客户端的连接
                        socket.Close();
                    }
                }
                catch (Exception ex)
                {
                    // 断开和这个客户端的连接
                    socket.Close();

                    // Get stack trace for the exception with source file information
                    var st = new StackTrace(ex, true);

                    StackFrame[] frames = st.GetFrames();

                    for (int i = 0; i < frames.Length; i++)
                    {
                        MyConsole.Log(frames[i].ToString());
                    }

                    MyConsole.Log(ex.Message);

                }
                finally
                {
                    // 标志完成接待工作
                    m_doneReceptEvent.Set();
                }

                // 分配 ID 卡
                //int id = DistributeId();

                // 指示正在等待客户端连接
                //if (m_players.Count < 4)
                //{
                //try
                //{
                //// 获取名字
                //string name = (string)Respond(socket, id);
                // 新增客户端
                //m_players.Add(new Player("temp-name", id, socket));



                ////Player thisPlayer = m_players.Last();
                //// 启动邮差
                ////StartPostman(thisPlayer);

                //// 获取名字，发送 ID
                //MyConsole.Log("准备从" + socket.RemoteEndPoint.ToString() + "获取用户名",/* Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);

                //string name = (string)Respond(id, id, 2);
                //m_players.Last().name = name;
                ////Console.WriteLine("用户" + name + "进入房间；ID = " + id.ToString());
                //MyConsole.Log("用户" + name + "进入房间；ID = " + id.ToString(), /*Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);

                //}
                //// 如果游戏玩家已经满员
                //else
                //{
                //    // 将连接的玩家加入观战玩家列表
                //    m_watchPlayers.Add(new Player("temp-name", id, socket));
                //    // 获取名字，发送 ID
                //    MyConsole.Log("准备从" + socket.RemoteEndPoint.ToString() + "获取用户名",/* Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);
                //    string name = (string)Respond(id, id, 2);
                //    m_watchPlayers.Last().name = name;
                //    //Console.WriteLine("用户" + name + "进入房间；ID = " + id.ToString());
                //    MyConsole.Log("用户" + name + "进入房间；ID = " + id.ToString(), /*Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);
                //}

                // 先锁住对该客户端的网络通信，好在主线程里做同步
                //m_players.Last().Lock();


                // 发送给新加入客户端，指示要先运行的线程的代码
                //Respond(id, idx, 2);

                //}
                //// 如果上面 Respond 一句发生异常，可能是掉线
                //catch (Exception e)
                //{
                //    MyConsole.Log(e.Message, /*"ComServer", */MyConsole.LogType.Error);
                //}
                //finally
                //{
                //    // 回收 ID 卡
                //    RecycleId(id);
                //    // 断开连接
                //    socket.Close();
                //    // 删除玩家信息
                //    m_players.RemoveAll(player => player.id == id);
                //}
                //if (m_players.Count == m_requiredNumber)
                //{
                //    m_gameReadyEvent.Set();
                //}

                // 触发先被阻塞的线程启动
                //m_events[idx].Set();
            }
        }

        // 结束对某个玩家的服务，断开连接
        // 
        public static void EndService(int id)
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

        ///// <summary>
        ///// 检查有没有主动关闭连接的同学，关闭掉线同学的连接
        ///// </summary>
        ///// <returns>检查出来的断线玩家个数</returns>
        //public static List<Player> HandleDisconnect()
        //{
        //    List<Player> disconnectPlayers = new List<Player>();
        //    // 检查每个客户端
        //    for (int i = 0; i < m_players.Count; i++)
        //    {
        //        Player player = m_players[i];
        //        // 如果这个用户掉线了
        //        if (player.IsDisconnected())
        //        {
        //            disconnectPlayers.Add(player);
        //            // 终止对他的服务
        //            EndService(player.id);
        //        }
        //    }
        //    return disconnectPlayers;
        //}


        /// <summary>
        /// 检查有没有主动关闭连接的同学，关闭掉线同学的连接
        /// </summary>
        /// <returns>检查出来的断线玩家个数</returns>
        public static void HandleDisconnect()
        {
            //while (true)
            //{
            //// 如果还在等待客户端连接服务器
            //if (m_waitingCustomerEvent.WaitOne(0))
            //{
            //if (doneReceptEvent.WaitOne(0))

            //{
            //doneReceptEvent.WaitOne();
            // 等待游戏流程完成工作
            //m_doneGameLoopEvent.WaitOne();
            //MyConsole.Log("准备开始断线处理"/*, Thread.CurrentThread.Name*/, MyConsole.LogType.Debug);

            // 指示还没有完成断线处理工作
            //m_doneHandleDisconnect.Reset();
            // 检查每个客户端
            for (int i = 0; i < m_players.Count;)
            {
                Player player = m_players[i];
                int id = player.id;
                string name = player.name;

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
                        EndService(id);
                        continue;
                    }
                }
                catch
                {
                    // 有可能这里断线了
                    // 终止对他的服务
                    EndService(id);

                    abnormallyDisconnectedUserNames.Add(name);

                    continue;
                }

                // 如果这个用户掉线了
                if (player.IsDisconnected())
                {
                    // 终止对他的服务
                    EndService(id);
                    // 向所有玩家发送此消息，channel 3 是紧急频道
                    //Broadcast(name, 3);
                    abnormallyDisconnectedUserNames.Add(name);
                    continue;
                }
                else
                {
                    i++;
                }
            }


            // 指示已经完成断线处理工作
            //m_doneHandleDisconnect.Set();
            //Thread.Sleep(1000);

            //}
            //}
            // 如果正在处理客户端连接后的事宜
            //else
            //{

            //}
            //}
        }

        // 解锁所有玩家
        public static void UnlockAll()
        {
            for (int i = 0; i < m_players.Count; i++)
            {
                m_players[i].Unlock();
            }
        }

    }
}
