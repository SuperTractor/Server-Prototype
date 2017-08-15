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

namespace Networking
{
    public class StatObject : BmobTable
    {
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

        // 构造函数
        public StatObject()
        {

        }

        public StatObject(String tableName)
        {
            fTable = tableName;
        }

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
            username = input.getString("username");
            highScore = input.getInt("highScore");
            totalScore = input.getInt("totalScore");
            highLevel = input.getInt("highLevel");
            totalLevel = input.getInt("totalLevel");
        }

        //写字段信息
        public override void write(BmobOutput output, bool all)
        {
            base.write(output, all);

            output.Put("username", username);
            output.Put("highScore", highScore);
            output.Put("totalScore", totalScore);
            output.Put("highLevel", highLevel);
            output.Put("totalLevel", totalLevel);
        }

    }
}
