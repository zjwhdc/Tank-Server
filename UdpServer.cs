using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DB_Test
{
    internal class UdpServer
    {
        public UdpClient udpserver;
        //byte[] data = new byte[1024];
        //存放所有连接中的udp客户端
        Dictionary<string, IPEndPoint> dic = new();
        //接收消息的缓存区
        byte[] redata;
        public void StartSrever()
        {
            udpserver = new UdpClient(8000);
            Print("udp启动");
            udpserver.BeginReceive(ReceveCallback,null);
        }

        void ReceveCallback(IAsyncResult asyncResult)
        {
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Any, 0);
            
            redata = udpserver.EndReceive(asyncResult, ref iPEndPoint);
            if (!dic.ContainsKey(iPEndPoint.ToString()))
            {
                //将第一次连接的加入字典
                dic.Add(iPEndPoint.ToString(), iPEndPoint);
            }
            //Print("收到udp消息："+Encoding.UTF8.GetString(redata));

            //处理消息
            ReadMessage(redata, iPEndPoint.ToString());

            
            //继续接受消息
            udpserver.BeginReceive(ReceveCallback,null);
        }


        public void ReadMessage(byte[] bytes, string name)
        {
            string s = Encoding.UTF8.GetString(bytes);
            string[] strings = s.Split('|');
            switch (strings[0])
            {
                //位置同步处理
                case "pos":
                    //将消息广播
                    Broadcast(bytes, name);
                break;
                case "box":
                    Broadcast(bytes, name);
                break;
                case "fire":
                    Broadcast(bytes, name);
                break;
            }
        }


        void Broadcast(byte[] bytes,string name)
        {
            foreach (var v in dic.Keys)
            {
                if (v == name)
                {
                    continue;
                }
                udpserver.BeginSend(bytes, bytes.Length, dic[v], SendCallback, null);
                //Print("广播消息成功");
            }
            
        }

        void SendCallback(IAsyncResult i)
        {
            udpserver.EndSend(i);

        }

        


        public void Print(string s)
        {
            Console.WriteLine(s);
        }
    }

}
