using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace Networking
{
    // 客户端专用交流接口
    public class ComClient
    {
        // 向服务器发送请求
        public static object Request(Socket socket)
        {
            // 向服务器发送请求
            Serializer.Send(socket, "");
            // 等待服务器返回
            return Serializer.Receive(socket);
        }
        // 向服务器发送请求
        public static object Request(Socket socket, object sth)
        {
            // 向服务器发送请求
            Serializer.Send(socket, sth);
            // 等待服务器返回
            return Serializer.Receive(socket);
        }
    }
}
