using System.Globalization;
using DB_Test.transmision;
using MySqlConnector;

namespace DB_Test
{
    internal class Program
    {

        static void Main(string[] args)
        {
            new UdpServer().StartSrever();
            NetManager.AccepctServer("127.0.0.1", 8080);
            Console.ReadLine();
            
        }
    }
}
