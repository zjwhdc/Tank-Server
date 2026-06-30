using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Test.transmision
{
    public class StructType//用于json序列化类
    {
        //判断为什么协议类型
        public int id { get; set; }

        //玩家账号
        public int user { get; set; }
        //玩家密码
        public string password { get; set; }
        //玩家描述 记录端口信息
        public string desc { get; set; }
    }
}
