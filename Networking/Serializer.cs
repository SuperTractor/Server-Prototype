using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Networking
{
    public class Serializer
    {
        public static byte[] Serialize(object sth)
        {
            using (var memoryStream = new MemoryStream())
            {
                (new BinaryFormatter()).Serialize(memoryStream, sth);
                return memoryStream.ToArray();
            }
        }

        public static object Deserialize(byte[] message)
        {
            using (var memoryStream = new MemoryStream(message))
            {
                object obj;
                try
                {
                    var bin_form = new BinaryFormatter();
                    obj = bin_form.Deserialize(memoryStream);
                }
                catch
                {
                    throw;
                }
                return obj;
            }
        }
        //进一步封装接口
        public static void Send(Socket mySocket, object sth)
        {
            byte[] temp = Serialize(sth);
            try
            {
                mySocket.Send(temp);
                // 等待接受者的回复(wait for reply)
                //ReceiveNoReply(mySocket);
            }
            catch/* (SocketException ex)*/
            {
                //Console.Write(ex.Message);
                //throw new Exception("此客户端断开了连接");
                throw;
            }
        }

        //public static void SendNoWait(Socket mySocket, object sth)
        //{
        //    byte[] temp = Serialize(sth);
        //    try
        //    {
        //        mySocket.Send(temp);
        //    }
        //    catch (SocketException ex)
        //    {
        //        Console.Write(ex.Message);
        //    }
        //}

        public static int BufferSize = 4096;
        public static object Receive(Socket mySocket)
        {
            byte[] temp = new byte[BufferSize];
            mySocket.Receive(temp);
            // 回复发送者(reply)
            //SendNoWait(mySocket, "");
            object obj;
            try
            {
                obj = Deserialize(temp);
            }
            catch
            {
                throw;
            }
            return obj;
        }
        //public static object ReceiveNoReply(Socket mySocket)
        //{
        //    byte[] temp = new byte[BufferSize];
        //    mySocket.Receive(temp);
        //    return Deserialize(temp);
        //}
    }


}
