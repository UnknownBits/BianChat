using System.Data.SQLite;

namespace Server
{
    public class SQLite : IDisposable
    {
        public SQLiteConnection conn;
        private bool disposedValue;

        public SQLite()
        {
            conn = new SQLiteConnection($"Data Source={Environment.DatabaseFileName};");
            try { conn.Open(); }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public bool GetUserId(string user_name, out int user_id)
        {
            try
            {
                string sql = $"SELECT UserInfo.Uid FROM UserInfo WHERE UserInfo.UserName = \"{user_name}\"";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                var value = rdr[0].ToString();
                if (int.TryParse(value, out int number))
                {
                    user_id = number;
                    rdr.Close();
                    return true;
                }
                else
                {
                    user_id = 0;
                    rdr.Close();
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                user_id = 0;
                return false;
            }
        }

        public bool Vaild_Password(int uid, string password_SHA256)
        {
            try
            {
                string sql = $"SELECT UserInfo.Password FROM UserInfo WHERE UserInfo.Uid = {uid}";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                SQLiteDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                var value = rdr[0].ToString();
                if (value != null && password_SHA256 != null && value == password_SHA256) { rdr.Close(); return true; } else { rdr.Close(); return false; } }
            catch (Exception ex) { Console.WriteLine(ex); return false; }
        }
        public bool AddValue(string user_name, string password, string email)
        {
            try
            {
                string sql = $"INSERT INTO UserInfo(UserName,Password,Email) VALUES ('{user_name}', '{password}', '{email}');";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public bool AddValue(string user_name, string password, string email,string QQ)
        {
            try
            {
                string sql = $"INSERT INTO UserInfo(UserName,Password,Email,QQ) VALUES ('{user_name}', '{password}', '{email}','{QQ}');";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
        protected virtual void Dispose(bool disposing) { if (!disposedValue) { if (disposing) { conn.Close(); } disposedValue = true; } }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    }
}