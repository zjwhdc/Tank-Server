using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using DB_Test.transmision;
using MySqlConnector;

namespace DB_Test
{
    internal static class DBHelper
    {
        private static string contankgame = "server=127.0.0.1;port=3306;database=tankgame;user=root;password=你的密码;";
        //用于返回的账号列表信息
        private static List<UserData> userDatasList = new List<UserData>();

        /// <summary>
        /// 获取坦克数据库每行信息
        /// </summary>
        /// <param name="tablename">要获取的表名</param>
        /// <returns>返回读取的数据类</returns>
        public static List<UserData> GetContankgame(string tablename)
        {
            //函数执行前先清空
            userDatasList.Clear();
            try
            {
                MySqlConnection con = new MySqlConnection(contankgame);
                
                    //打开数据库
                    con.Open();

                    string str = $"select * from {tablename}";
                    MySqlCommand cmd = new MySqlCommand(str, con);

                    MySqlDataReader md = cmd.ExecuteReader();

                    
                    while (md.Read())
                     {
                           UserData data = new UserData();
                           data.Id = md.GetInt32("userid");
                           data.Password = md.GetString("password");
                           userDatasList.Add(data);
                     }
                return userDatasList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("数据库查询失败"+ex.ToString());
                return null;
            }

            
        }

        /// <summary>
        /// 新增坦克账号密码函数
        /// </summary>
        /// <param name="userid">账号</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static bool InsertUserData(int userid,string password)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(contankgame))
                {
                    con.Open();
                    string str = "insert into user(userid,password) values(@userid,@password)";
                    MySqlCommand md = new MySqlCommand(str,con);
                    md.Parameters.AddWithValue("@userid", userid);
                    md.Parameters.AddWithValue("@password", password);
                    md.ExecuteNonQuery();
                    Console.WriteLine("插入数据成功");
                    return true;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("插入数据失败"+e.ToString());
                return false;
            }
        }


        /// <summary>
        /// 修改密码函数
        /// </summary>
        /// <param name="data">需要修改的字段 比如password</param>
        /// <param name="userid">要修改密码的账号</param>
        /// <param name="value">要修改的密码</param>
        /// <returns></returns>
        public static bool UpdateData(string data,int userid,object value)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(contankgame))
                {
                    con.Open();
                    string str = $"update user set {data}=@value where userid={userid}";
                    MySqlCommand md = new MySqlCommand(str, con);
                    md.Parameters.AddWithValue("@value", value);
                    md.ExecuteNonQuery ();
                    return true;    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("修改数据失败" + e.ToString);
                return false;
            }
        }

        /// <summary>
        /// 清空表
        /// </summary>
        public static void DeleteTable(string strtable)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(contankgame))
                {
                    con.Open ();
                    string str = $"TRUNCATE TABLE {strtable}";
                    MySqlCommand md = new MySqlCommand(str,con);
                    md.ExecuteNonQuery();
                }
            }
            catch(Exception e)
            {
                Console.Write("清空表失败"+e.ToString());
            }
        }


        /// <summary>
        /// 删除账户的函数
        /// </summary>
        /// <param name="userid">要删除的账号</param>
        /// <returns></returns>
        public static bool DeletUser(int userid)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(contankgame))
                {
                    con.Open();
                    string str = $"delete from user where userid = {userid}";
                    MySqlCommand md = new MySqlCommand(str, con);
                    md.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("删除账户失败"+e.ToString());
                return false;
            }
        }
    }
}
