using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;


namespace Networking
{
    // 服务器专用交流接口
    public class ComServer
    {
        // 服务器响应客户端请求
        public static object Respond(Socket socket, object sth)
        {
            // 等待客户端请求
            object obj = Serializer.Receive(socket);

            // 处理请求(暂无)
            // 如果有请求的处理操作, 可能需要用到 C# 的 delegate 机制或者 lambda 表达式, 将操作传进来

            // 发送返回信息
            Serializer.Send(socket, sth);
            // 返回请求信息
            return obj;
        }


    }
}
