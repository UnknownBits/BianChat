using System;
using System.Collections.Generic;
using System.ComponentModel;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Console_Tcp
{
    internal class mysql
    {

        
        MySqlConnection conn;

        public mysql()
        {
            string connStr = $"server = 221.224.90.88; user = visitor; database = bian; port = 5000; password = {password}";
            conn = new MySqlConnection(connStr);
            try { 
                conn.Open(); 
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString()); 
            }
        }
        public int Get_user_id(string user_name)
        {
            string sql = $"SELECT UserInfo.Uid FROM UserInfo WHERE UserInfo.UserName = \"{user_name}\"";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            int user_id = 5;
            while (rdr.Read())
            {
                var s = rdr[0].ToString();
                if (s != null)
                {
                    user_id = int.Parse(s);
                }
                else
                {
                    throw new Exception("返回值为空");
                }
            }
            rdr.Close();
            return user_id;
        }
        public MySqlDataReader Get_password_SHA256(int uid)
        {
            string sql = $"SELECT UserInfo.`Password` FROM UserInfo WHERE UserInfo.Uid = {uid}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            return rdr;
        }
    }
}
