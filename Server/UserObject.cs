using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DatabaseUtility;
using GameUtility;
namespace Server
{
    /// <summary>
    /// 对应 user 表单一个记录
    /// </summary>
    /// 
    //[Serializable]
    public class UserObject : DataObject
    {
        public string password
        {
            get
            {
                return (string)Get("password");
            }
            set
            {
                Set("password", value);
            }
        }
        public string nickname
        {
            get
            {
                return (string)Get("nickname");
            }
            set
            {
                Set("nickname", value);
            }
        }
        public string headImageName
        {
            get
            {
                return (string)Get("headImageName");
            }
            set
            {
                Set("headImageName", value);
            }
        }
        // 积分；经验值；从stat.xml中的grades同步过来
        public int experience
        {
            get
            {
                return (int)Get("experience");
            }
            set
            {
                Set("experience", value);
            }
        }
        // 头衔；对应于stat.xml中的level
        public string title
        {
            get
            {
                return (string)Get("title");
            }
            set
            {
                Set("title", value);
            }
        }
        // 是否在线戳
        public bool isOnline
        {
            get
            {
                return (bool)Get("isOnline");
            }
            set
            {
                Set("isOnline", value);
            }
        }
        public UserObject()
        {
            variables.Add(new NamedVariable("password", "string"));
            variables.Add(new NamedVariable("nickname", "string"));
            variables.Add(new NamedVariable("headImageName", "string"));
            variables.Add(new NamedVariable("experience", "int"));
            variables.Add(new NamedVariable("title", "string"));
            variables.Add(new NamedVariable("isOnline", "bool"));
            // 初始化
            password = "";
            nickname = "玩家";
            headImageName = "Head0";
            experience = 0;
            title = "";
            isOnline = false;
        }
        public UserObject(DataObject dataObj)
        {
            variables = new List<NamedVariable>(dataObj.variables);
        }
        // 将用户信息复制到玩家信息
        public void CopyTo(PlayerInfo info)
        {
            info.username = username;
            info.nickname = nickname;
            info.headImageName = headImageName;
            info.experience = experience;
            info.title = title;
            info.isOnline = isOnline;
        }
        // 将玩家信息复制到用户信息
        public void CopyFrom(PlayerInfo info)
        {
            username = info.username;
            nickname = info.nickname;
            headImageName = info.headImageName;
            experience = info.experience;
            title = info.title;
            isOnline = info.isOnline;
        }
    }
}
