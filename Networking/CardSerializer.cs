using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameUtility;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
namespace Networking
{
    // 专门用于网络传输牌的类
    public class CardSerializer
    {
        // 发送牌组
        public static void Send(Socket socket,Card[] cards)
        {
            // 将牌组转化为 int 数组
            int[] cards_int = Card.ToInt(cards);
            // 再发送出去
            Serializer.Send(socket, cards_int);
        }
        public static void Send(Socket socket, List<Card> cards)
        {
            // 将牌组转化为 int 数组
            int[] cards_int = Card.ToInt(cards);
            // 再发送出去
            Serializer.Send(socket, cards_int);
        }
        public static Card[] Receive(Socket socket)
        {
            // 先接受 Int 数组存储的牌组
            int[] cards_int = (int[])Serializer.Receive(socket);
            //string test = (string)Serializer.Receive(socket);
            //Console.WriteLine(test);
            // 返回 Card 数组存储的牌组
            return Card.ToCard(cards_int);
            //return null;
        }
    }
}
