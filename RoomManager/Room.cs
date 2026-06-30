using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DB_Test.RoomManager
{
    
    internal class Room
    {
        public int roomid { get; set; }
        public List<Player> playerlist { get; set; } = new List<Player>();
        
    }
}
