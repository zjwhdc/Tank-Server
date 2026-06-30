using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Test.transmision
{
    //记录账号密码信息类 用于数据库返回
    internal class UserData
    {
        //账号
        public int Id { get; set; }
        //密码
        public string Password { get; set; }
    }
}
