using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using MySqlConnector;

namespace Server_Console
{
    internal class mysql
    {
        /// <summary>
        /// 
        /// </summary>
        MySqlConnection conn;
        
        /// <summary>
        /// 
        /// </summary>
        public mysql()
        {
            string connStr = $"server = 221.224.90.88; user = visitor; database = bian; port = 5000; password = H#ok3365)~!mQ.v";
            conn = new MySqlConnection(connStr);
            try { 
                conn.Open(); 
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString()); 
            }
        }

        /// <summary>
        /// 获取用户id
        /// </summary>
        /// <param name="user_name"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int Get_user_id(string user_name)
        {
            string sql = $"SELECT UserInfo.Uid FROM UserInfo WHERE UserInfo.UserName = \"{user_name}\"";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            var value = rdr[0].ToString();
            if (value != null)
            {
                rdr.Close();
                return int.Parse(value);
            }
            else
            {
                rdr.Close();
                throw new Exception("sql返回值为空");
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="password_SHA256"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Vaild_Password(int uid, string password_SHA256)
        {
            string sql = $"SELECT UserInfo.`Password` FROM UserInfo WHERE UserInfo.Uid = {uid}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            var s = rdr[0].ToString();
            if (s != null && password_SHA256 != null && s == password_SHA256)
            {
                rdr.Close();
                return true;
            }
            else
            {
                rdr.Close();
                throw new Exception("返回值为空");
            }
        }

        public string Get_SHA256(string Data)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(Data);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] by = Sha256.ComputeHash(SHA256Data);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }
    }
}
