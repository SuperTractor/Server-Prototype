using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Networking
{
    // 网络通信用的消息，可以理解成 信封+信件
    [Serializable]
    public class Message
    {
        // 消息头部分，信封
        //// 发件人邮戳
        //private int m_senderTag;
        //public int senderTag
        //{
        //    get { return m_senderTag; }
        //}
        //// 收件人邮戳
        //private int m_receiverTag;
        //public int receiverTag
        //{
        //    get
        //    {
        //        return m_receiverTag;
        //    }
        //}
        // 通信频道
        private int m_channel;
        public int channel
        {
            get { return m_channel; }
        }
        // 数据部分，信件
        private object m_data;
        public object data
        {
            get
            {
                return m_data;
            }
        }
        // 构造函数
        public Message(/*int senderTag, int receiverTag,*/ object data,int channel)
        {
            //m_senderTag = senderTag;
            //m_receiverTag = receiverTag;
            m_data = data;
            m_channel = channel;
        }
    }
}
