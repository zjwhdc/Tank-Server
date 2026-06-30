using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DB_Test.RoomManager
{
    internal class Player
    {
        //玩家账号
        public int username { get; set; }
        //玩家阵营 0为红 1为蓝
        public int redandblue {  get; set; }
        public float x {  get; set; }
        public float y {  get; set; }
        public float z {  get; set; }
    }
}
