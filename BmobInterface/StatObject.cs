using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cn.bmob.api;
using cn.bmob.config;
//using cn.bmob.example;
using cn.bmob.exception;
using cn.bmob.Extensions;
using cn.bmob.http;
using cn.bmob.io;
using cn.bmob.json;
using cn.bmob.response;
using cn.bmob.tools;
using GameUtility;


namespace BmobInterface
{
    public class StatObject : BmobTable
    {
        // 表名
        public const String tableName = "stat";
        // 列名
        public const string usernameCol = "username";
        public const string highScoreCol = "highScore";
        public const string totalScoreCol = "totalScore";
        public const string highLevelCol = "highLevel";
        public const string totalLevelCol = "totalLevel";


        String fTable;
        // 以下对应云端字段名称
        // 用户名
        public String username { get; set; }
        // 最高得分
        public BmobInt highScore { get; set; }
        // 累计得分
        public BmobInt totalScore { get; set; }
        // 最高级数
        public BmobInt highLevel { get; set; }
        // 累计级数
        public BmobInt totalLevel { get; set; }


        // 复制函数
        public void Copy(StatObject source)
        {
            ACL = source.ACL;
            objectId = source.objectId;

            username = source.username;
            highScore = source.highScore;
            totalScore = source.totalScore;
            highLevel = source.highLevel;
            totalLevel = source.totalLevel;
        }

        //public StatObject(String tableName)
        //{
        //    fTable = tableName;
        //}

        // 表名
        public override string table
        {
            get
            {
                if (fTable != null)
                {
                    return fTable;
                }
                return base.table;
            }
        }

        //读字段信息
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            username = input.getString(usernameCol);
            highScore = input.getInt(highScoreCol);
            totalScore = input.getInt(totalScoreCol);
            highLevel = input.getInt(highLevelCol);
            totalLevel = input.getInt(totalLevelCol);
        }

        //写字段信息
        public override void write(BmobOutput output, bool all)
        {
            base.write(output, all);

            output.Put(usernameCol, username);
            output.Put(highScoreCol, highScore);
            output.Put(totalScoreCol, totalScore);
            output.Put(highLevelCol, highLevel);
            output.Put(totalLevelCol, totalLevel);
        }

        // 创建一条新记录
        void Create()
        {
            highScore = 0;
            totalScore = 0;
            highLevel = 0;
            totalLevel = 0;

            BmobInstance.bmob.Create(tableName, this, (resp, exception) =>
            {
                if (exception != null)
                {
                    BmobDebug.Log("创建失败, 失败原因为： " + exception.Message);
                    return;
                }

                BmobDebug.Log("创建成功, @" + resp.createdAt);
            });
        }

        // 更新一行数据
        void Update()
        {
            BmobInstance.bmob.Update(tableName, objectId, this, (resp, exception) =>
            {
                if (exception != null)
                {
                    BmobDebug.Log("修改失败, 失败原因为： " + exception.Message);
                    return;
                }

                BmobDebug.Log("修改成功, @" + resp.updatedAt);
            });
        }

        // 查询数据
        // 根据当前用户名查询该用户的记录，并保存在此对象内
        // 如果没有查到，会新增一条记录
        void Find(/*string username*/)
        {
            // 创建一个查询对象
            BmobQuery query = new BmobQuery();
            // 查询用户名为 username 的记录
            query.WhereEqualTo(usernameCol, username);
            BmobInstance.bmob.Find<StatObject>(tableName, query, (resp, exception) =>
            {
                if (exception != null)
                {
                    BmobDebug.Log("查询失败, 失败原因为： " + exception.Message);
                    return;
                }

                //对返回结果进行处理
                List<StatObject> list = resp.results;
                //BmobDebug.Log(string.Format("获取对象个数：{0}", list.Count));
                //foreach (var stat in list)
                //{
                //    BmobDebug.Log("获取的对象为： " + stat.ToString());
                //}

                // 如果找到记录
                if (list.Count > 0)
                {
                    // 复制到此对象
                    Copy(list[0]);
                }
                // 如果记录为空
                else
                {
                    BmobDebug.Log(string.Format("查询用户{0}记录为空；创建新记录中...", username));
                    Create();
                    //BmobDebug.Log(string.Format("成功创建新记录"));
                }

            });
        }

        // 构造函数
        public StatObject(string username)
        {
            fTable = tableName;
            // 初始化用户名
            this.username = username;
            // 从 Bmob 上查找该用户记录
            Find();
        }
        // 一定要定义无参数构造函数，否则 bmob 会出错
        public StatObject()
        {
            fTable = tableName;
        }

        // 复制信息到 playerInfo 类
        public void CopyTo(PlayerInfo playerInfo)
        {
            playerInfo.highScore = highScore.Get();
            playerInfo.totalScore = totalScore.Get();
            playerInfo.highLevel = highLevel.Get();
            playerInfo.totalLevel = totalLevel.Get();
        }

    }
}
