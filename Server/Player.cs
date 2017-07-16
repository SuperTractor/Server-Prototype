using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using GameUtility;



namespace server_0._0._1
{
    public class Player
    {
        // 玩家信息
        public PlayerInfo playerInfo { get; set; }
        // 与该玩家通信的 socket
        public Socket socket { get; set; }

        public Player(PlayerInfo playerInfo,Socket socket)
        {
            this.playerInfo = playerInfo;
            this.socket = socket;
        }
    }
}
