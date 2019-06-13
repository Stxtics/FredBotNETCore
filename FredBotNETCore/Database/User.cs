using Discord.WebSocket;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class User
    {
        public long UserID { get; set; }
        public string Username { get; set; }
        public string PR2Name { get; set; } = null;
        public int? JV2ID { get; set; } = null;
        public int Balance { get; set; } = 10;
        public string LastUsed { get; set; } = null;

        public static bool Exists(SocketUser user)
        {
            Database database = new Database();

            string str = string.Format("SELECT * FROM users WHERE user_id = '{0}' LIMIT 1", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                long userId = (long)tableName["user_id"];

                if (userId != 0)
                {
                    database.CloseConnection();
                    return true;
                }
            }

            return false;
        }

        public static void Add(SocketUser user)
        {
            Database database = new Database();

            string str = string.Format("INSERT INTO users (user_id, username ) VALUES ('{0}', \"{1}\")", user.Id, user.Username);
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static List<User> GetTop()
        {
            List<User> users = new List<User>();
            User user;
            Database database = new Database();

            string str = string.Format("SELECT * FROM users ORDER BY balance desc LIMIT 10");
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                user = new User
                {
                    UserID = (long)tableName["user_id"],
                    Username = (string)tableName["username"]
                };
                if (DBNull.Value.Equals(tableName["pr2_name"]))
                {
                    user.PR2Name = null;
                }
                else
                {
                    user.PR2Name = (string)tableName["pr2_name"];
                }
                if (DBNull.Value.Equals(tableName["jv2_id"]))
                {
                    user.JV2ID = null;
                }
                else
                {
                    user.JV2ID = int.Parse((string)tableName["jv2_id"]);
                }
                user.Balance = (int)tableName["balance"];
                if (DBNull.Value.Equals(tableName["last_used"]))
                {
                    user.LastUsed = null;
                }
                else
                {
                    user.LastUsed = (string)tableName["last_used"];
                }
                users.Add(user);
            }
            database.CloseConnection();
            return users;
        }

        public static bool IsVerified(SocketUser user)
        {
            Database database = new Database();

            string str = string.Format("SELECT * FROM users WHERE user_id = '{0}' LIMIT 1", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                string pr2Name = (string)tableName["pr2_name"];

                if (pr2Name != null)
                {
                    database.CloseConnection();

                    return true;
                }
            }
            database.CloseConnection();

            return false;
        }

        public static User GetUser(string type, string value)
        {
            Database database = new Database();
            User user = new User();
            string str = string.Format("SELECT * FROM users WHERE {0} = '{1}' LIMIT 1", type, value);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                user.UserID = (long)tableName["user_id"];
                user.Username = (string)tableName["username"];
                if (DBNull.Value.Equals(tableName["pr2_name"]))
                {
                    user.PR2Name = null;
                }
                else
                {
                    user.PR2Name = (string)tableName["pr2_name"];
                }
                if (DBNull.Value.Equals(tableName["jv2_id"]))
                {
                    user.JV2ID = null;
                }
                else
                {
                    user.JV2ID = int.Parse((string)tableName["jv2_id"]);
                }
                user.Balance = (int)tableName["balance"];
                if (DBNull.Value.Equals(tableName["last_used"]))
                {
                    user.LastUsed = null;
                }
                else
                {
                    user.LastUsed = (string)tableName["last_used"];
                }
            }
            database.CloseConnection();
            return user;
        }

        public static void SetValue(SocketUser user, string type, string value)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("UPDATE users SET {1} = \"{2}\" WHERE user_id = {0} LIMIT 1", user.Id, type, value);
                MySqlDataReader reader = database.FireCommand(str);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch (Exception)
            {
                database.CloseConnection();
                return;
            }
        }
    }
}
