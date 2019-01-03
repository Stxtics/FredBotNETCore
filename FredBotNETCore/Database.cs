using Discord;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;

namespace FredBotNETCore
{
    public class Database
    {
        private static readonly string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "TextFiles");
        private string Table { get; set; }
        private const string server = "localhost";
        private const string database = "FredBotDatabase";
        private const string username = "root";
        private readonly string password = new StreamReader(path: Path.Combine(downloadPath, "DatabasePassword.txt")).ReadLine();
        private readonly MySqlConnection dbConnection;

        public Database(string table)
        {
            Table = table;
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = server,
                UserID = username,
                Password = password,
                Database = database,
                SslMode = MySqlSslMode.None
            };

            string connectionString = stringBuilder.ToString();

            dbConnection = new MySqlConnection(connectionString);

            dbConnection.Open();
        }

        public MySqlDataReader FireCommand(string query)
        {
            if (dbConnection == null)
            {
                return null;
            }

            MySqlCommand command = new MySqlCommand(query, dbConnection);

            MySqlDataReader mySqlReader = command.ExecuteReader();

            return mySqlReader;
        }

        public void CloseConnection()
        {
            if (dbConnection != null)
            {
                dbConnection.Close();
            }
        }

        public static List<string> CheckExistingUser(IUser user)
        {
            List<string> result = new List<string>();
            Database database = new Database("FredBotDatabase");

            string str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                string userId = (string)tableName["user_id"];

                result.Add(userId);
            }
            database.CloseConnection();
            return result;
        }

        public static string EnterUser(IUser user)
        {
            Database database = new Database("FredBotDatabase");

            string str = string.Format("INSERT INTO fredbottable (user_id, username, pr2_name, jv2_id, balance, last_used ) VALUES ('{0}', '{1}', 'Not verified', '0', '{2}', '5/7/18')", user.Id, user.Username, 10);
            MySqlDataReader table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }

        public static List<string> GetTop()
        {
            List<string> result = new List<string>();
            Database database = new Database("FredBotDatabase");

            string str = string.Format("SELECT * FROM fredbottable ORDER BY balance desc");
            MySqlDataReader tableName = database.FireCommand(str);
            int count = 0;
            while (tableName.Read() && count < 10)
            {
                string userId = (string)tableName["user_id"];

                result.Add(userId);
                count++;
            }
            database.CloseConnection();

            return result;
        }

        public static List<string> CheckForVerified(IUser user, string pr2name)
        {
            List<string> result = new List<string>();
            Database database = new Database("FredBotDatabase");

            string str = string.Format("SELECT * FROM fredbottable WHERE pr2_name = '{0}' AND user_id = '{1}'", pr2name, user.Id);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                string pr2Name = (string)tableName["pr2_name"];

                result.Add(pr2Name);
            }
            database.CloseConnection();

            return result;
        }

        public static ulong GetDiscordID(string pr2name)
        {
            ulong id = 0;
            Database database = new Database("FredBotDatabase");

            string str = string.Format("SELECT * FROM fredbottable WHERE pr2_name = '{0}'", pr2name);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                id = ulong.Parse(tableName["user_id"].ToString());
            }
            database.CloseConnection();
            return id;
        }

        public static void VerifyUser(IUser user, string pr2name)
        {
            Database database = new Database("FredBotDatabase");
            try
            {
                string str = string.Format("UPDATE fredbottable SET pr2_name = '{1}' WHERE user_id = {0}", user.Id, pr2name);
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

        public static void VerifyJV2(SocketUser user, string jv2ID)
        {
            Database database = new Database("FredBotDatabase");
            try
            {
                string str = string.Format("UPDATE fredbottable SET jv2_id = '{1}' WHERE user_id = {0}", user.Id, jv2ID);
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

        public static void SetLastUsed(SocketUser user, string date)
        {
            Database database = new Database("FredBotDatabase");
            try
            {
                string str = string.Format("UPDATE fredbottable SET last_used = '{1}' WHERE user_id = {0}", user.Id, date);
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

        public static string GetLastUsed(SocketUser user)
        {
            string lastUsed = "";
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                lastUsed = (string)tableName["last_used"];
            }
            database.CloseConnection();
            return lastUsed;
        }

        public static void SetBalance(SocketUser user, int bal)
        {
            Database database = new Database("FredBotDatabase");
            try
            {
                string str = string.Format("UPDATE fredbottable SET balance = '{1}' WHERE user_id = {0}", user.Id, bal);
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

        public static int GetBalance(SocketUser user)
        {
            int bal = 0;
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                bal = (int)tableName["balance"];
            }
            database.CloseConnection();
            return bal;
        }

        public static string GetPR2Name(IUser user)
        {
            string pr2Name = "";
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                pr2Name = (string)tableName["pr2_name"];
            }
            database.CloseConnection();

            return pr2Name;
        }

        public static string GetUserID(string pr2name)
        {
            string userID = "1";
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT * FROM fredbottable WHERE pr2_name = '{0}'", pr2name);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                userID = (string)tableName["user_id"];
            }
            database.CloseConnection();

            return userID;
        }

        public static string AddPrior(IUser user, string username, string type, string moderator, string reason)
        {
            Database database = new Database("FredBotDatabase");

            string str = $"INSERT INTO banlog (user_id, username, type, moderator, reason ) VALUES ('{user.Id}', \"{username}\", '{type}', \"{moderator}\", \"{reason}\")";
            MySqlDataReader table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }

        public static string GetCase(string caseN)
        {
            string caseInfo = "";
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT * FROM banlog WHERE `case` = '{0}'", caseN);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                string type = (string)tableName["type"];
                string userId = (string)tableName["user_id"];
                string username = (string)tableName["username"];
                string moderator = (string)tableName["moderator"];
                string reason = (string)tableName["reason"];

                caseInfo = $"**Case {caseN}\n    Type: **{type}\n    **User: **({userId}) {username}\n    **Moderator: **{moderator}\n    **Reason: **{reason}";
            }
            database.CloseConnection();
            return caseInfo;
        }

        public static List<string> Modlogs(IUser user)
        {
            List<string> modLogs = new List<string>();
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT * FROM banlog WHERE user_id = '{0}'", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                int caseN = (int)tableName["case"];
                string type = (string)tableName["type"];
                string userId = (string)tableName["user_id"];
                string username = (string)tableName["username"];
                string moderator = (string)tableName["moderator"];
                string reason = (string)tableName["reason"];

                modLogs.Add(item: $"**Case {caseN}\n    Type: **{type}\n    **User: **({userId}){username}\n    **Moderator: **{moderator}\n    **Reason: **{reason}");
            }
            database.CloseConnection();
            return modLogs;
        }

        public static void UpdateReason(string caseN, string reason)
        {
            Database database = new Database("FredBotDatabase");
            try
            {
                string str = string.Format("UPDATE banlog SET reason = \"{1}\" WHERE `case` = {0}", caseN, reason);
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

        public static long CaseCount()
        {
            long cases = 0;
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT COUNT(*) FROM banlog");
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                cases = (long)tableName["COUNT(*)"];
            }
            database.CloseConnection();
            return cases;
        }

        public static List<string> Warnings(IUser user = null)
        {
            List<string> warnings = new List<string>();
            Database database = new Database("FredBotDatabase");
            if (user == null)
            {
                string str = string.Format("SELECT * FROM banlog WHERE type = 'Warn'");
                MySqlDataReader tableName = database.FireCommand(str);
                while (tableName.Read())
                {
                    int caseN = (int)tableName["case"];
                    string type = (string)tableName["type"];
                    string userId = (string)tableName["user_id"];
                    string username = (string)tableName["username"];
                    string moderator = (string)tableName["moderator"];
                    string reason = (string)tableName["reason"];

                    warnings.Add(item: $"**Case {caseN}**\n    **User: **({userId}){username}\n    **Moderator: **{moderator}\n    **Reason: **{reason}");
                }
            }
            else
            {
                string str = string.Format("SELECT * FROM banlog WHERE type = 'Warn' AND user_id = '{0}'", user.Id);
                MySqlDataReader tableName = database.FireCommand(str);
                while (tableName.Read())
                {
                    int caseN = (int)tableName["case"];
                    string type = (string)tableName["type"];
                    string userId = (string)tableName["user_id"];
                    string username = (string)tableName["username"];
                    string moderator = (string)tableName["moderator"];
                    string reason = (string)tableName["reason"];

                    warnings.Add(item: $"**Case {caseN}**\n    **Moderator: **{moderator}\n    **Reason: **{reason}");
                }
            }
            database.CloseConnection();
            return warnings;
        }

        public static long WarnCount(IUser user)
        {
            long warnCount = 0;
            Database database = new Database("FredBotDatabase");
            string str = string.Format("SELECT COUNT(*) FROM banlog WHERE type = 'Warn' AND user_id = '{0}'", user.Id);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                warnCount = (long)tableName["COUNT(*)"];
            }
            database.CloseConnection();
            return warnCount;
        }

        public static void ClearWarn(IUser user)
        {
            Database database = new Database("FredBotDatabase");
            try
            {
                string str = string.Format("DELETE FROM banlog WHERE type = 'Warn' AND user_id = '{0}'", user.Id);
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
