#define DATABASE
//#undef DATABASE

#undef RULE

// 调试抢底
//#define DEBUG_BID_1
#undef DEBUG_BID_1

#define READY_STAGE
//#undef READY_STAGE

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
    public class Program
    {

        static void Initialize()
        {
            Thread.CurrentThread.Name = "主线程";
            // 设置命令行的编码为 utf8
            Console.OutputEncoding = Encoding.Unicode;



            // 初始化 ComServer
            //ComServer.Initialize(Dealer.playerNumber, m_roomSize, m_doneGameLoopEvent);
            ComServer.Initialize(Dealer.playerNumber/*, m_roomSize, m_doneGameLoopEvent*/);

        }

        // 启动前台接待线程
        static void StartWaitClient()
        {
            // 新建前台接待线程
            Thread waitThread = new Thread(new ThreadStart(ComServer.WaitClient));
            waitThread.Name = "接待处";
            // 启动前台接待线程
            waitThread.Start();
            // Spin for a while waiting for the started thread to become
            // alive:
            while (!waitThread.IsAlive) ;

        }
        // 启动断线处理线程
        // 在游戏开始前，这个线程负责实时更新玩家那边知道的房间内的玩家列表
        // 游戏开始后，这个线程负责及时通知是否出现了断线；客户端那边的断线处理线程接收到通知之后，会改变客户端的游戏主线程
        static void StartDisconnectHandler()
        {
            // 新建前台接待线程
            Thread handlerThread = new Thread(new ThreadStart(ComServer.HandleDisconnect));
            handlerThread.Name = "断线经理";
            // 启动前台接待线程
            handlerThread.Start();
            // Spin for a while waiting for the started thread to become
            // alive:
            while (!handlerThread.IsAlive) ;

        }


        static void Main(string[] args)
        {
            // 初始化服务器
            Initialize();

            // 启动前台接待线程
            StartWaitClient();

            // 启动断线经理
            //StartDisconnectHandler();



            // 死循环
            //while (true)
            //{
            // 等待游戏预备信号
            //ComServer.gameReadyEvent.WaitOne();

            //try
            //{
            // 开始游戏主循环
            //GameLoop();
            //}
            //// 如果有玩家断线了
            //catch (Exception e)
            //{
            //    // 调试：输出异常信息
            //    Console.WriteLine(e.Message);
            //    // 重新开始 GameLoop
            //    continue;
            //}
            //finally
            //{

            //}


            //// 开始游戏主循环
            //GameLoop();
            //}
        }
    }
}
