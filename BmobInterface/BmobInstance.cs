using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cn.bmob.exception;
using cn.bmob.Extensions;
using cn.bmob.http;
using cn.bmob.io;
using cn.bmob.json;
using cn.bmob.response;
using cn.bmob.tools;
using cn.bmob.api;
using cn.bmob.config;

using ConsoleUtility;

namespace BmobInterface
{
    public class BmobInstance
    {
        // 创建 Bmob 实例
        static BmobWindows m_bmob;
        public static BmobWindows bmob
        {
            get { return m_bmob; }
        }
        // Bmob 应用密钥
        static string m_appKey = "b8a425de61165951af4051382e9ec80a";
        static string m_resetKey = "ed19a91d4dfe434a4117e0c4c39bca75";

        // 用户统计数据列表
        //public static List<StatObject> userStats;

        static public void Initialize()
        {
            // 创建 Bmob 实例
            m_bmob = new BmobWindows();
            // 初始化 bmob 实例
            m_bmob.initialize(m_appKey, m_resetKey);
            // 注册调试工具
            BmobDebug.Register(msg => { MyConsole.Log((string)msg, MyConsole.LogType.Debug); });
            // 创建用户统计数据表接口
            //userStats = new List<StatObject>();
        }



        // 提交所有数据
        public static void Summit()
        {
            // 首先查看有没有这个名字的用户

            // 如果有，则更新他的数据

            // 如果没有，则创建他的数据
        }

        // 获取所有数据
        public static void Request(string username)
        {
            // 首先查看有没有这个名字的用户

            // 如果没有，则创建他的数据

            // 然后再获取
        }
    }
}
