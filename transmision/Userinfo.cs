using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Test.transmision
{
    //用于解析客户端消息的缓存类
    internal class Userinfo
    {
        //用于装接收的消息
        public byte[] data;
        //记录接收数据的长度
        public int count;

        private Userinfo() { }
        public Userinfo(byte[] data,int count)
        {
            this.data = data;
            this.count = count;
        }
        
    }
}
