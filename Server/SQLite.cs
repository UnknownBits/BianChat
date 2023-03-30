using System.Data.SQLite;

namespace Server
{
    public class SQLite : IDisposable
    {
        private bool disposedValue;
        public SQLiteConnection conn;
        public static string DatabaseFileName = $@"Database.db";
        private object queueObj = new object();

        public SQLite()
        {
            conn = new SQLiteConnection($"Data Source={DatabaseFileName};");
            try { conn.Open(); }
            catch (Exception ex){
                Console.WriteLine(ex);
            }
        }

        public static void CreateDatabase()
        {
            if (!File.Exists(DatabaseFileName)) SQLiteConnection.CreateFile(DatabaseFileName);
            SQLiteConnection conn = new SQLiteConnection($"Data Source={DatabaseFileName};");
            try {
                conn.Open();
                string sql = "CREATE TABLE IF NOT EXISTS UserInfo(\"Uid\" integer NOT NULL,\"State\" integer NOT NULL,\"Username\" text NOT NULL,\"Password\" text NOT NULL,\"Email\" text,PRIMARY KEY (\"Uid\"));";
                SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {   conn.Close();
                Console.WriteLine(ex); 
            }
        }

        public bool GetUserId(string username, out int UID)
        {
            lock (queueObj)
                try
                {
                    string sql = $"SELECT UserInfo.Uid FROM UserInfo WHERE UserInfo.UserName = \"{username}\"";
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    using SQLiteDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var value = rdr[0].ToString();
                        if (int.TryParse(value, out UID)) return true;
                        else
                        {
                            UID = 0;
                            return false;
                        }
                    }
                    else
                    {
                        UID = 0;
                        return false;
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex); UID = 0; return false; }
        }

        public enum ValuesType {
            Username,
            Password,
            ProfilePhoto,
            Email,
            MaxValue = Email
        }

        public bool GetValue(int UID, ValuesType type,out string data)
        {
            lock (queueObj)
                try
                {
                    string sql = $"SELECT UserInfo.{type} FROM UserInfo WHERE UserInfo.Uid = {UID}";
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    using SQLiteDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var value = rdr[0].ToString();
                        if (value != null)
                        {
                            data = value;
                            return true;
                        }
                        else { data = ""; return false; }
                    }
                    else
                    {
                        data = "";
                        return false;
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex); data = ""; return false; }
        }

        public bool Vaild_Password(int uid, string password_SHA256)
        {
            lock (queueObj)
                try
                {
                    string sql = $"SELECT UserInfo.Password FROM UserInfo WHERE UserInfo.Uid = {uid}";
                    SQLiteCommand cmd = new SQLiteCommand(sql, conn);
                    using SQLiteDataReader rdr = cmd.ExecuteReader();
                    if (rdr.Read())
                    {
                        var value = rdr[0].ToString();
                        if (value != null && password_SHA256 != null && value == password_SHA256)
                            return true;
                        else return false;
                    }
                    else return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
        }
        public bool AddValue(string username, string password, string email)
        {
            lock (queueObj)
                try
                {
                    string sql = $"INSERT INTO UserInfo(Username,Password,ProfilePhoto,FriendList,Email) VALUES ('{username}', '{password}', '', '', '{email}');";
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

        public bool AddValue(string username, string password, string profilePhoto, string email)
        {
            lock (queueObj)
            {
                try
                {
                    string sql = $"INSERT INTO UserInfo(Username,Password,ProfilePhoto,FriendList,Email) VALUES ('{username}', '{password}', '{profilePhoto}', '', '{email}');";
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
        }

        public bool UpdateValue(int uid, ValuesType type, string content)
        {
            lock (queueObj)
            {
                try
                {
                    string sql = $"UPDATE UserInfo SET {type} = '{content}' WHERE UserInfo.Uid = {uid}";
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
        }

        protected virtual void Dispose(bool disposing) { if (!disposedValue) { if (disposing) conn.Close(); disposedValue = true; } }

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    }
}