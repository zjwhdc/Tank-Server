using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DB_Test.transmision;
using System.Text.Json;
using MySqlConnector;
using System.Threading.Tasks.Dataflow;
using DB_Test.RoomManager;

namespace DB_Test
{
   
    class SendBuffer//发送数据类
    {
        //记录要的发送的数据总长度
        public int count;
        //记录发送了多少数据
        public int writecount;
        //public byte[] buffer;
        public Socket socket;
    }
    public enum E_MessageType
    {
        Register = 0,
        Login = 1,
    }

    internal class NetManager
    {
        //自身服务器socket
        public static Socket socketserver;
        //客户端socket容器
        public static Dictionary<Socket, Client> clientdic = new Dictionary<Socket, Client>();
        //用于记录发送数据的临时缓存
        static Dictionary<byte[], SendBuffer> senddic = new Dictionary<byte[], SendBuffer>();
        //记录玩家所在所有房间
        static Dictionary<int,Room> roomdic = new Dictionary<int,Room>();
        //私有空参构造 以防止外部实例化

        private static readonly Random _rand = new Random();
        private NetManager() { }

        /// <summary>
        /// 接受客户端连接函数
        /// </summary>
        /// <param name="ipaddress">IP地址</param>
        /// <param name="port">端口号</param>
        public static void AccepctServer(string ipaddress,int port)
        {
            socketserver = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipaddress), port);

                socketserver.Bind(endPoint);
                socketserver.Listen(100);
                socketserver.NoDelay = true;
                Print("服务器启动");
                //等待客户端连接
                socketserver.BeginAccept(AcceptCallback, socketserver);
            }
            catch (Exception e)
            {
                Print("连接失败"+e.ToString());
            }

        }

        /// <summary>
        /// 接受客户端回调函数
        /// </summary>
        /// <param name="asyncResult"></param>
        static void AcceptCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket s = asyncResult.AsyncState as Socket;
                Socket clientsocket = socketserver.EndAccept(asyncResult);
                IPEndPoint iPEndPoint = clientsocket.RemoteEndPoint as IPEndPoint;

                //记录对应客户端
                Client c = new Client(clientsocket, iPEndPoint.Address.ToString(), new Userinfo(new byte[1024*1024], 0));
                Print(iPEndPoint.Address.ToString());
                Adddic(clientsocket, c);
                Print("有客户端连入" + clientsocket.RemoteEndPoint.ToString());

                //用客户端接受消息
                ReceveMsg(clientsocket);

                //继续接受客户端连接
                socketserver.BeginAccept(AcceptCallback, s);
            }
            catch(Exception e)
            {
                Print(e.ToString());
            }
        }


        /// <summary>
        /// 接受消息函数
        /// </summary>
        /// <param name="s">要接收的客户端</param>
        public static void ReceveMsg(Socket s)
        {
            try
            {
                //开启接受数据
                s.BeginReceive(clientdic[s].userinfo.data, 0, 1024, 0, ReceveMsgCallback, s);
            }
            catch(Exception e)
            {
                Print("接收失败" + e.ToString());
            }
        }
        /// <summary>
        /// 接收消息回调函数
        /// </summary>
        /// <param name="asyncResult"></param>
        static void ReceveMsgCallback(IAsyncResult asyncResult)
        {
            try
            {
                Socket s = asyncResult.AsyncState as Socket;
                int count = s.EndReceive(asyncResult);
                clientdic[s].userinfo.count += count;
                if (count <= 0)
                {
                    //接收到的数据小于0就是连接断开
                    return;
                }
                while (true)
                {

                    if (clientdic[s].userinfo.count < 4)
                    {
                        //如果接受到的数据小于4说明不达到解析数据的要求
                        break;
                    }
                    //得到前四个字节 数据的总长度
                    int len = BitConverter.ToInt32(clientdic[s].userinfo.data, 0);
                    if (clientdic[s].userinfo.count < (len + 4))
                    {
                        //总接收到的字节数小于总字节
                        break;
                    }

                    //把完整消息给拷贝出来交给OnReadFullMessage处理
                    byte[] data = new byte[len+4];
                    Buffer.BlockCopy(clientdic[s].userinfo.data, 0, data, 0, len+4);
                    OnReadFullMessage(data,s);

                    //重新调整客户端缓冲区
                    Buffer.BlockCopy(clientdic[s].userinfo.data, len + 4, clientdic[s].userinfo.data, 0, clientdic[s].userinfo.count - (len + 4));
                    clientdic[s].userinfo.count -= len + 4;
                }

                //继续接收消息
                s.BeginReceive(clientdic[s].userinfo.data, clientdic[s].userinfo.count, 1024, 0, ReceveMsgCallback, s);
            }
            catch (Exception e)
            {
                Print("接收消息失败" + e.ToString());
            }
        }




        /// <summary>
        /// 读取整条消息并广播
        /// </summary>
        /// 参数1发送的字节 参数2是要发送的客户端
        private static void OnReadFullMessage(byte[] bytes,Socket s)
        {
            int num =  BitConverter.ToInt32(bytes, 0);
            //除了前四个字节全部转为json
            string jsonstr = Encoding.UTF8.GetString(bytes, 4, num).TrimEnd('\0');
            Print("接收到消息：" + jsonstr);
            //将字符串反序列化为对象
            StructType structType = JsonSerializer.Deserialize<StructType>(jsonstr);
            //判断消息类型
            switch (structType.id)
            {
                    //0为注册协议
                case 0:
                    List<UserData> userlist = DBHelper.GetContankgame("user");
                    foreach (UserData userdata in userlist)
                    {
                        if (structType.user == userdata.Id)
                        {
                            //如果有相同账号则发送注册失败协议
                            //空 通过端口信息发送 找到对应客户端
                            string succes = "1|注册失败";
                            byte[] succesbytes = Encoding.UTF8.GetBytes(succes);
                            int succeslen = succesbytes.Length;
                            byte[] succesdata = new byte[succeslen + 4];
                            BitConverter.GetBytes(succeslen).CopyTo(succesdata, 0);
                            succesbytes.CopyTo(succesdata, 4);
                            SendMsg(succesdata, s);
                            return;
                        }
                    }
                    //如果没有相同账号则将账号添加进数据库
                    DBHelper.InsertUserData(structType.user, structType.password);
                    //在发送注册成功协议
                    //print(jsonstr); 
                    string str = "2|注册成功";
                    byte[] strbytes = Encoding.UTF8.GetBytes(str);
                    int len = strbytes.Length;
                    byte[] data = new byte[len + 4];
                    BitConverter.GetBytes(len).CopyTo(data, 0);
                    strbytes.CopyTo(data, 4);
                    SendMsg(data,s);
                    break;

                case 1: //1为登录协议
                    List<UserData> loginuserlist = DBHelper.GetContankgame("user");
                    foreach (UserData userdata in loginuserlist)
                    {
                        if (structType.user == userdata.Id)
                        {
                            //如果有相同账号则比对密码
                            if (structType.password == userdata.Password)
                            {
                                //如果密码相同则发送登陆成功协议
                                string succes = "3|登录成功";
                                byte[] succesbytes = Encoding.UTF8.GetBytes(succes);
                                int succeslen = succesbytes.Length;
                                byte[] succesdata = new byte[succeslen + 4];
                                BitConverter.GetBytes(succeslen).CopyTo(succesdata, 0);
                                succesbytes.CopyTo(succesdata, 4);
                                SendMsg(succesdata, s);
                                return;
                            }
                            
                        }
                    }
                    string failstr = "4|登录失败";
                    byte[] failstrbytes = Encoding.UTF8.GetBytes(failstr);
                    int faillen = failstrbytes.Length;
                    byte[] faildata = new byte[faillen + 4];
                    BitConverter.GetBytes(faillen).CopyTo(faildata, 0);
                    failstrbytes.CopyTo(faildata, 4);
                    SendMsg(faildata, s);
                    break;
                case 2://为创建房间协议
                    Print("收到创建房间协议");
                    Room room = new Room();//创建房间对象
                    Player player = new Player();//创建房间玩家对象
                    player.username = structType.user;
                    player.redandblue = 1;//初始分配阵营都为蓝方
                    room.roomid = structType.user;//赋值房间id
                    room.playerlist.Add(player);
                    roomdic.Add(structType.user, room);
                    string roomstr = JsonSerializer.Serialize(room);
                    roomstr = "5|" + roomstr;
                    byte[] roombytes = Encoding.UTF8.GetBytes(roomstr);
                    int roomlen = roombytes.Length;//字节数组长度
                    byte[] roombytes2 = new byte[roomlen + 4];
                    BitConverter.GetBytes(roomlen).CopyTo(roombytes2,0);
                    roombytes.CopyTo(roombytes2,4);
                    //发送创建房间协议
                    SendMsg(roombytes2,s);
                    break;
                case 3://为加入房间协议
                    Print("收到加入房间协议");
                    if (structType.user == int.Parse(structType.password))
                    {
                        //如果客户端点击加入房间的是房主就不加入玩家player对象直接返回room房间对象给客户端
                        //给客户端发送房间信息
                        Room roomjoin = roomdic[int.Parse(structType.password)];
                        string strjoin = JsonSerializer.Serialize(roomjoin);
                        strjoin = "6|" + strjoin;
                        byte[] bytejoin = Encoding.UTF8.GetBytes(strjoin);
                        int strjoinlen = bytejoin.Length;
                        byte[] datajoin = new byte[strjoinlen + 4];
                        BitConverter.GetBytes(strjoinlen).CopyTo(datajoin, 0);
                        bytejoin.CopyTo(datajoin, 4);
                        //发送房间协议
                        SendMsg(datajoin, s);
                        Print("为房主发送的加入房间协议，发送成功 消息为："+Encoding.UTF8.GetString(datajoin,4,datajoin.Length-4));
                    }
                    else
                    {
                        //如果进入到这里就说明点击加入房间的不是房主
                        Player p = new Player();
                        p.username = structType.user;
                        roomdic[int.Parse(structType.password)].playerlist.Add(p);
                        //设置红蓝阵营方
                        p.redandblue = roomdic[int.Parse(structType.password)].playerlist.Count / 2;


                        Room roomjoin = roomdic[int.Parse(structType.password)];
                        string strjoin = JsonSerializer.Serialize(roomjoin);
                        strjoin = "6|" + strjoin;
                        byte[] bytejoin = Encoding.UTF8.GetBytes(strjoin);
                        int strjoinlen = bytejoin.Length;
                        byte[] datajoin = new byte[strjoinlen + 4];
                        BitConverter.GetBytes(strjoinlen).CopyTo(datajoin, 0);
                        bytejoin.CopyTo(datajoin, 4);
                        //发送房间协议
                        SendMsg(datajoin, s);
                        Print("为普通玩家发送的加入房间协议 发送成功");
                    }
                        break;
                case 4://为刷新房间协议
                    //将房间信息发送给对应客户端
                    Print("收到刷新房间协议");
                    Fullroom fr = new Fullroom();
                    foreach (Room r in roomdic.Values)
                    {
                        fr.Roomslist.Add(r);
                    }
                    string frstr = JsonSerializer.Serialize(fr);
                    frstr = "7|" + frstr;
                    byte[] bytestr = Encoding.UTF8.GetBytes(frstr);
                    int bytestrlen = bytestr.Length;
                    byte[] datafr = new byte[bytestrlen+4];
                    BitConverter.GetBytes(bytestrlen).CopyTo(datafr, 0);
                    bytestr.CopyTo(datafr, 4);

                    SendMsg(datafr,s);
                    Print("发送刷新房间协议成功");
                    fr = null;
                    break;
                case 5://为开始游戏协议
                    Print("收到序号5开始游戏协议");
                    foreach (int id in roomdic.Keys)
                    {
                        //只有房主才能点击开始游戏
                        if (structType.user == id)
                        {
                            //List<int> li = new List<int>();
                            for (int i = 0; i < roomdic[id].playerlist.Count; i++)
                            {
                                //给所有房间里的玩家发送随机地图坐标信息
                                roomdic[id].playerlist[i].y = RandomFloat(30, 33);
                                roomdic[id].playerlist[i].x = RandomFloat(450,600 );
                                roomdic[id].playerlist[i].z = RandomFloat(500,600 );

                            }
                            foreach (var v in clientdic.Keys)
                            {
                                //将位置房间位置坐标发送给所有人
                                Room r = roomdic[id];
                                string rstr = JsonSerializer.Serialize(r);
                                rstr = "8|" + rstr;
                                byte[] bytes1 = Encoding.UTF8.GetBytes(rstr);
                                int bytes1len = bytes1.Length;
                                byte[] bytes2 = new byte[bytes1len + 4];
                                BitConverter.GetBytes(bytes1len).CopyTo(bytes2, 0);
                                bytes1.CopyTo(bytes2, 4);
                                SendMsg(bytes2, v);
                                Print("发送消息成功："+rstr);
                            }
                        }
                    }

                    break;
            }
            
        }


        /// <summary>
        /// 发送字节消息
        /// </summary>
        /// <param name="bytes">发送的字节</param>
        /// <param name="s">要发送的客户端</param>
        private static void SendMsg(byte[] bytes,Socket s)
        {
            try
            {
                SendBuffer sendBuffer = new SendBuffer();
                sendBuffer.count = bytes.Length;
                sendBuffer.socket = s;
                senddic.Add(bytes, sendBuffer);
                s.BeginSend(bytes, 0, bytes.Length, 0, SendCallback, bytes);
            }
            catch
            {
                Print("发送信息失败");
            }
        }


        static void SendCallback(IAsyncResult asyncResult)
        {
            try
            {
                byte[] sendbyte = asyncResult.AsyncState as byte[];
                int count = senddic[sendbyte].socket.EndSend(asyncResult);
                senddic[sendbyte].writecount += count;
                if (senddic[sendbyte].count - senddic[sendbyte].writecount > 0)
                {
                    //说明数据还没发送完毕
                    senddic[sendbyte].socket.BeginSend(sendbyte, senddic[sendbyte].writecount, sendbyte.Length - senddic[sendbyte].writecount, 0, SendCallback, sendbyte);
                }
                else
                {
                    //发送成功
                    senddic.Remove(sendbyte);
                    Print("发送成功");
                }
            }
            catch
            {

            }
        }



        /// <summary>
        /// 为客户端字典添加socket
        /// </summary>
        /// <param name="s">客户端socket</param>
        /// <param name="c">客户端类</param>
        static void Adddic(Socket s,Client c)
        {
            if (clientdic.ContainsKey(s))
            {
                return;
            }
            clientdic.Add(s, c);
        }



        /// <summary>
        /// 获取 [400, 600) 之间的随机float
        /// </summary>
        public static float RandomFloat(float f,float f2)
        {
            // NextDouble() → [0.0,1.0)
            double t = _rand.NextDouble();
            double value = f + t * (f2 - f);
            return (float)value;
        }

        /// <summary>
        /// 输出函数
        /// </summary>
        /// <param name="str">输出字符串</param>
        static void Print(string str)
        {
            Console.WriteLine(str);
        }
    }
}
