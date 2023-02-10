using MySqlConnector;

namespace Server
{
    public class MySql : IDisposable
    {
        public MySqlConnection conn;
        private bool disposedValue;

        public MySql()
        {
            conn = new MySqlConnection($"server = 221.224.90.88; user = visitor; database = bian; port = 5000; password = H#ok3365)~!mQ.v");
            try { conn.Open(); }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

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

        public bool Vaild_Password(int uid, string password_SHA256)
        {
            string sql = $"SELECT UserInfo.`Password` FROM UserInfo WHERE UserInfo.Uid = {uid}";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            var s = rdr[0].ToString();
            if (s != null && password_SHA256 != null && s == password_SHA256) { rdr.Close(); return true; }
            else { rdr.Close(); return false; }
        }

        protected virtual void Dispose(bool disposing) { if (!disposedValue) { if (disposing) { conn.Close(); } disposedValue = true; } }
        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    }
}

