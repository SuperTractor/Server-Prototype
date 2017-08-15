using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Networking
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

        // 客户端列表
        static List<Player> m_players;
        public static List<Player> players
        {
            get
            {
                return m_players;
            }
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
            m_ip = IPAddress.Parse(m_ipStr);
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Bind(new IPEndPoint(m_ip, m_port));
            m_socket.Listen(m_backlog);
            // 初始化用户 ID 卡
            m_idCards = new List<int>(Enumerable.Range(0, capacity));

            //Console.WriteLine("启动监听{0}成功", m_socket.LocalEndPoint.ToString());
            MyConsole.Log("启动监听" + m_socket.LocalEndPoint.ToString() + "成功",/* "ComServer",*/ MyConsole.LogType.Debug);
            // 引用传进来的事件
            m_doneGameLoopEvent = doneGameLoopEvent;
            // 存放事件，以备后用
            m_events[0] = m_doneGameLoopEvent;
            m_events[1] = m_doneHandleDisconnect;
        }

        public static object Respond(int playerId, object sth, int channel = 0)
        {
            Player thisPlayer = m_players.Find(player => player.id == playerId);
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

        // 等待客户端连接
        public static void WaitClient()
        {
            MyConsole.Log("准备开始接待线程"/*, Thread.CurrentThread.Name*/, MyConsole.LogType.Debug);
            int idx;
            // 服务器还没有满时
            // 测试：多少用户都接
            while (true)
            {
                MyConsole.Log("等待客户端接入"/*, Thread.CurrentThread.Name*/, MyConsole.LogType.Debug);
                // 指示正在等待客户端连接
                m_waitingCustomerEvent.Set();
                // 接收客户端连接
                Socket socket = m_socket.Accept();

                // 指示已经有客户端连接，准备开始接待事宜
                m_waitingCustomerEvent.Reset();

                // 获取首先触发的事件
                //idx = WaitHandle.WaitAny(m_events);

                //// 等待断线处理工作完成
                //m_doneHandleDisconnect.WaitOne();
                //// 等待游戏流程完成
                m_doneGameLoopEvent.WaitOne();
                //// 重置
                //m_doneHandleDisconnect.Reset();
                //m_doneGameLoopEvent.Reset();

                // 指示没有完成接待
                m_doneReceptEvent.Reset();

                MyConsole.Log("接收连接 " + socket.RemoteEndPoint.ToString(),/* Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);

                // 分配 ID 卡
                int id = DistributeId();
                //try
                //{
                //// 获取名字
                //string name = (string)Respond(socket, id);
                // 新增客户端
                m_players.Add(new Player("temp-name", id, socket));
                //Player thisPlayer = m_players.Last();
                // 启动邮差
                //StartPostman(thisPlayer);

                // 获取名字，发送 ID
                MyConsole.Log("准备从" + socket.RemoteEndPoint.ToString() + "获取用户名",/* Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);

                string name = (string)Respond(id, id, 2);
                m_players.Last().name = name;

                // 先锁住对该客户端的网络通信，好在主线程里做同步
                //m_players.Last().Lock();

                //Console.WriteLine("用户" + name + "进入房间；ID = " + id.ToString());
                MyConsole.Log("用户" + name + "进入房间；ID = " + id.ToString(), /*Thread.CurrentThread.Name,*/ MyConsole.LogType.Debug);

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
                // 标志完成接待工作
                m_doneReceptEvent.Set();
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
                // 如果这个用户掉线了
                if (player.IsDisconnected())
                {
                    // 终止对他的服务
                    EndService(id);
                    // 向所有玩家发送此消息，channel 3 是紧急频道
                    //Broadcast(name, 3);
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
