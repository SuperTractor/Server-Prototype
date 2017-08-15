using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using GameUtility;
using System.Threading;
using System.Diagnostics;
using System.IO;
using ConsoleUtility;

namespace Networking
{
    public class Player
    {
        private int m_id;
        public int id
        {
            get
            {
                return m_id;
            }
            //set;
        }
        // 玩家的名字
        private string m_name;
        public string name
        {
            get
            {
                return m_name;
            }
            set
            {
                m_name = value;
            }
        }

        // 用来同步线程的门
        private object m_monitor = new object();
        // 与该玩家通信的 socket
        private Socket m_socket;
        public Socket socket
        {
            get
            {
                return m_socket;
            }
            //set;
        }

        // 和这个玩家对应的消息池，这是一个队列，0位头部，LAST为尾部
        // 专门开设 1 个邮差线程，负责接收和发送消息
        // 其他的线程只能从消息池里查看自己频道的消息，这样可以避免，某线程在侦听信道时，其他线程收到了它的消息，造成死锁
        // 如果取到的消息不属于自己的频道
        // 则将消息放到消息池里，重复上述步骤
        // 收件箱
        private List<Message> m_inMessagePool;
        // 发件箱
        private List<Message> m_outMessagePool;


        // 邮差管理员线程
        // 负责协调收发邮件，这是因为socket只有一个，相当于路只有1条，每次只能收或者发，2者不能同时进行
        // 这可不可以用锁代替？
        private Thread m_postManagerThread;
        // 邮差的职责，收发邮件
        // 收件邮差线程
        private Thread m_inPostmanThread;
        // 收件邮差是否完成工作
        private bool m_doneReceive;

        // 收邮件延时等待时间（毫秒）
        static int receiveDelay = 20;
        //// 收邮件等待计时器
        //private Stopwatch m_receiveStopwatch;

        // 发件邮差线程
        private Thread m_outPostmanThread;
        // 发件邮差是否完成工作
        private bool m_doneSend;
        // 网络通信锁
        private bool m_isLocked;
        public bool isLocked
        {
            get
            {
                return m_isLocked;
            }
        }

        // 网络流
        Stream m_networkStream;
        // 二进制读取流，用于接收数据
        BinaryReader m_binaryReader;
        // 二进制写入流，用于发送数据
        BinaryWriter m_binaryWriter;

        /// <summary>
        /// 获取特定频道的最早消息
        /// </summary>
        /// <param name="channel">网络通信频道</param>
        /// <returns>最早消息</returns>
        Message Receive(int channel=0)
        {
            Message thisMessage;
            //MyConsole.Log("准备从收件箱中获取频道" + channel.ToString() + "上来自客户端" + m_name + "的消息", "Player-Receive", MyConsole.LogType.Debug);


            //// 等待客户端从该频道发来消息
            //do
            //{
            //    thisMessage = m_inMessagePool.Find(message => message.channel == channel);
            //}
            //while (thisMessage == null);

            //MyConsole.Log("成功从收件箱中获得频道" + channel.ToString() + "上来自客户端" + m_name + "的消息", "Player-Receive", MyConsole.LogType.Debug);

            //thisMessage = (Message)Serializer.Receive(m_socket);

            // 先接收数据长度
            int size = m_binaryReader.ReadInt32();
            // 再接收数据
            byte[] buf = m_binaryReader.ReadBytes(size);
            // 反序列化
            thisMessage = (Message)Serializer.Deserialize(buf);

            return thisMessage;
        }


        /// <summary>
        /// 发送消息接口，将要发送的消息放进发件箱内
        /// </summary>
        /// <param name="message">要发送的消息</param>
        void Send(Message message)
        {
            //MyConsole.Log("准备向客户端" + m_name + "发件箱投放消息", "Player-Send", MyConsole.LogType.Debug);

            //m_outMessagePool.Add(message);
            //MyConsole.Log("成功向客户端" + m_name + "发件箱投放消息", "Player-Send", MyConsole.LogType.Debug);

            //Serializer.Send(m_socket, message);
            // 发送消息
            byte[] buf = Serializer.Serialize(message);
            // 先发送数据长度
            m_binaryWriter.Write(buf.Length);
            // 再发送数据本身
            m_binaryWriter.Write(buf);
        }

        // 服务器响应客户端请求
        //public static object Respond(Socket socket, object sth, int channel = 0)
        //{
        //    //object obj;
        //    Message message;
        //    try
        //    {
        //        // 等待客户端请求
        //        message = (Message)Serializer.Receive(socket);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        throw;
        //    }
        //    try
        //    {
        //        // 发送返回信息
        //        Serializer.Send(socket, new Message(sth, channel));
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        throw;
        //    }
        //    // 返回请求信息
        //    return message.data;
        //}

        // 服务器响应客户端请求
        public Message Respond(Message msg)
        {
            //if (isLocked)
            //{
            //    throw new Exception("尝试对锁住的客户端通信");
            //}
            Message message;

            //if (IsDisconnected())
            //{
            //    throw new Exception("客户端" + name + "断开了连接");
            //}
            try
            {
                // 检查消息池内有无此频道的消息
                message = (Message)Serializer.Receive(m_socket);
                //message = Receive();
            }
            catch
            {
                throw;
            }
            //do
            //{
            //    // 检查消息池内有无此频道的消息
            //    // 这一步相当于 receive
            //    message = thisPlayer.Receive(channel);
            //}
            //// 一直等到获取到此频道的消息为止
            //while (message == null);

            //try
            //{
            //    // 等待客户端请求
            //    //obj = Serializer.Receive(socket);
            //    message = (Message)Serializer.Receive(socket);
            //    // 
            //}
            //catch/* (Exception e)*/
            //{
            //    throw new Exception("客户端" + thisPlayer.name + "断开了连接");
            //}

            // 发送消息
            Serializer.Send(m_socket, msg);

            //Send(msg);

            //try
            //{
            //    // 发送返回信息
            //    Serializer.Send(socket, new Message(sth, channel));
            //}
            //catch /*(Exception e)*/
            //{
            //    throw;
            //}
            // 返回请求信息
            //return obj;
            return message;
        }

        // 服务器响应客户端请求
        //public static object Respond(Socket socket, object sth, int channel = 0)
        //{
        //    //object obj;
        //    Message message;
        //    try
        //    {
        //        // 等待客户端请求
        //        message = (Message)Serializer.Receive(socket);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        throw;
        //    }
        //    try
        //    {
        //        // 发送返回信息
        //        Serializer.Send(socket, new Message(sth, channel));
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //        throw;
        //    }
        //    // 返回请求信息
        //    return message.data;
        //}


        // 检查玩家是否断线
        public bool IsDisconnected()
        {
            bool isDisconnect;
            // 防止其他线程访问socket
            //lock (m_monitor)
            //{
            //isDisconnect = m_socket.Poll(0, SelectMode.SelectRead) && m_socket.Available == 0;
            //}
            int code;
            object obj;
            Message message;
            try
            {
                /*message = */
                Respond(new Message(0, 1));
                //code = (int)message.data;
            }
            catch (Exception e)
            {
                MyConsole.Log(e.Message, MyConsole.LogType.Error);

                // 有异常，很可能断线了
                return true;
            }
            //return isDisconnect;

            //int code = (int)Respond(0);
            //return code != 1;
            return false;
        }

        // 收件邮差线程
        public void InPostmanJob()
        {
            MyConsole.Log("正在启动客户端" + m_name + "的收件邮差", /*"Player-InPostmanJob",*/ MyConsole.LogType.Debug);
            //m_doneReceive = false;

            //Stopwatch receiveStopwatch = new Stopwatch();
            //receiveStopwatch.Start();
            // 还没有等待完
            //while (receiveStopwatch.ElapsedMilliseconds < receiveDelay)
            while (true)
            {
                //try
                //{
                //lock (m_monitor)
                //{
                // 不断接收客户端发送过来的消息，添进去收件池当中去
                m_inMessagePool.Add((Message)Serializer.Receive(m_socket));
                //}

                //}
                //// 如果玩家断线了
                //catch (Exception e)
                //{
                //    MyConsole.Log(e.Message, /*"Player-InPostmanJob",*/ MyConsole.LogType.Error);
                //    // 不干了
                //    break;
                //}
            }
            //m_doneReceive = true;
            MyConsole.Log("客户端" + m_name + "的收件邮差完成工作", /*"Player-InPostmanJob",*/ MyConsole.LogType.Debug);
            //MyConsole.Log("查看 socket 接收状态-" + m_socket.Available.ToString(), "Player-InPostmanJob", MyConsole.LogType.Debug);

        }

        // 发件邮差线程
        public void OutPostmanJob()
        {
            MyConsole.Log("正在启动客户端" + m_name + "的发件邮差", /*"Player-OutPostmanJob",*/ MyConsole.LogType.Debug);


            //MyConsole.Log("查看 socket 接收状态-" + m_socket.Available.ToString(), "Player-PostManagerJob", MyConsole.LogType.Debug);

            //try
            //{
            // 不断将发件箱里的消息，按时间顺序，发送到客户端
            while (m_outMessagePool.Count > 0)
            {
                //lock (m_monitor)
                //{
                MyConsole.Log("查看 socket 接收状态-" + m_socket.Available.ToString(), /*"Player-PostManagerJob",*/ MyConsole.LogType.Debug);

                Serializer.Send(m_socket, m_outMessagePool[0]);
                //}

                m_outMessagePool.RemoveAt(0);
            }
            //}
            //// 如果玩家断线了
            //catch (Exception e)
            //{
            //    MyConsole.Log(e.Message, /*"Player-OutPostmanJob", */MyConsole.LogType.Error);
            //    // 不干了
            //}
            MyConsole.Log("客户端" + m_name + "的发件邮差完成工作", /*"Player-OutPostmanJob", */MyConsole.LogType.Debug);

        }

        // 邮差管理员线程
        void PostManagerJob()
        {
            MyConsole.Log("正在启动客户端" + m_name + "的邮差", /*"Player-PostManagerJob", */MyConsole.LogType.Debug);
            // 只要客户端是连接着的
            while (!IsDisconnected())
            {
                m_inPostmanThread = new Thread(new ThreadStart(InPostmanJob));
                m_outPostmanThread = new Thread(new ThreadStart(OutPostmanJob));

                m_inPostmanThread.Name = m_id + "收件邮差";
                m_outPostmanThread.Name = m_id + "发件邮差";

                // 首先去收件
                m_inPostmanThread.Start();
                while (m_inPostmanThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    // 等待收件邮差结束工作
                }
                //MyConsole.Log("查看 socket 接收状态-" + m_socket.Available.ToString(), "Player-PostManagerJob", MyConsole.LogType.Debug);


                // 然后去发件
                m_outPostmanThread.Start();
                while (m_outPostmanThread.ThreadState == System.Threading.ThreadState.Running)
                {
                    // 等待发件邮差结束工作
                }
            }
            MyConsole.Log("客户端" + m_name + "断开连接", /*"Player-PostManagerJob",*/ MyConsole.LogType.Debug);

        }

        public Player(string name, int id, Socket socket)
        {
            m_name = name;
            m_id = id;
            m_socket = socket;
            //cardInHand = new List<Card>();
            // 新建消息池
            m_inMessagePool = new List<Message>();
            m_outMessagePool = new List<Message>();
            // 新建邮差线程
            //m_postManagerThread = new Thread(new ThreadStart(PostManagerJob));
            //m_postManagerThread.Name = m_id + "邮差管理员";
            // 新建计时器
            //m_receiveStopwatch = new Stopwatch();

            // 初始化网络通信流
            m_networkStream = new NetworkStream(m_socket);
            m_binaryReader = new BinaryReader(m_networkStream);
            m_binaryWriter = new BinaryWriter(m_networkStream);

        }

        // 加锁
        public void Lock()
        {
            m_isLocked = true;
        }
        // 解锁
        public void Unlock()
        {
            m_isLocked = false;
        }

        /// <summary>
        /// 检查是否有特定频道的最早消息
        /// </summary>
        /// <param name="channel">网络通信频道</param>
        /// <returns>消息在消息池中的索引</returns>
        public int PeekMessagePool(int channel)
        {
            return m_inMessagePool.FindIndex(message => message.channel == channel);
        }

        /// <summary>
        /// 将不需要的信息倒进消息池内
        /// </summary>
        /// <param name="message">要倒的消息</param>
        public void PourMessage(Message message)
        {
            m_inMessagePool.Add(message);
        }



    }
}
