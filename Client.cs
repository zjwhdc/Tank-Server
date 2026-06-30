using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using DB_Test.transmision;

namespace DB_Test
{
    internal class Client
    {
        //客户端对应socket
        public Socket clientSocket;
        //客户端ip和端口记录
        public string desc;
        //客户端接收数据信息类
        public Userinfo userinfo;

        public Client(Socket clientSocket,string desc, Userinfo userinfo)
        {
            this.clientSocket = clientSocket;
            this.desc = desc;
            this.userinfo = userinfo;
        }

    }
}
