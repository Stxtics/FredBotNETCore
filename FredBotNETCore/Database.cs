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
        static readonly string downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "TextFiles");
        private string Table { get; set; }
        private const string server = "localhost";
        private const string database = "FredBotDatabase";
        private const string username = "root";
        private string password = new StreamReader(path: Path.Combine(downloadPath, "DatabasePassword.txt")).ReadLine();
        private MySqlConnection dbConnection;

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

            var connectionString = stringBuilder.ToString();

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

            var mySqlReader = command.ExecuteReader();

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
            var result = new List<string>();
            var database = new Database("FredBotDatabase");

            var str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var userId = (string)tableName["user_id"];

                result.Add(userId);
            }
            database.CloseConnection();
            return result;
        }

        public static string EnterUser(IUser user)
        {
            var database = new Database("FredBotDatabase");

            var str = string.Format("INSERT INTO fredbottable (user_id, username, pr2_name, jv2_id, balance, last_used ) VALUES ('{0}', '{1}', 'Not verified', '0', '{2}', '5/7/18')", user.Id, user.Username, 10);
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }

        public static List<string> GetTop()
        {
            var result = new List<string>();
            var database = new Database("FredBotDatabase");

            var str = string.Format("SELECT * FROM fredbottable ORDER BY balance desc");
            var tableName = database.FireCommand(str);
            int count = 0;
            while (tableName.Read() && count < 10)
            {
                var userId = (string)tableName["user_id"];

                result.Add(userId);
                count++;
            }
            database.CloseConnection();

            return result;
        }

        public static List<string> CheckForVerified(IUser user, string pr2name)
        {
            var result = new List<string>();
            var database = new Database("FredBotDatabase");

            var str = string.Format("SELECT * FROM fredbottable WHERE pr2_name = '{0}' AND user_id = '{1}'", pr2name, user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var pr2Name = (string)tableName["pr2_name"];

                result.Add(pr2Name);
            }
            database.CloseConnection();

            return result;
        }

        public static ulong GetDiscordID(string pr2name)
        {
            ulong id = 0;
            var database = new Database("FredBotDatabase");

            var str = string.Format("SELECT * FROM fredbottable WHERE pr2_name = '{0}'", pr2name);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                id = ulong.Parse(tableName["user_id"].ToString());
            }
            database.CloseConnection();
            return id;
        }

        public static void VerifyUser(IUser user, string pr2name)
        {
            var database = new Database("FredBotDatabase");
            try
            {
                var str = string.Format("UPDATE fredbottable SET pr2_name = '{1}' WHERE user_id = {0}", user.Id, pr2name);
                var reader = database.FireCommand(str);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void VerifyJV2(SocketUser user, string jv2ID)
        {
            var database = new Database("FredBotDatabase");
            try
            {
                var str = string.Format("UPDATE fredbottable SET jv2_id = '{1}' WHERE user_id = {0}", user.Id, jv2ID);
                var reader = database.FireCommand(str);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static void SetLastUsed(SocketUser user, string date)
        {
            var database = new Database("FredBotDatabase");
            try
            {
                var str = string.Format("UPDATE fredbottable SET last_used = '{1}' WHERE user_id = {0}", user.Id, date);
                var reader = database.FireCommand(str);
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
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            var tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                lastUsed = (string)tableName["last_used"];
            }
            database.CloseConnection();
            return lastUsed;
        }

        public static void SetBalance(SocketUser user, int bal)
        {
            var database = new Database("FredBotDatabase");
            try
            {
                var str = string.Format("UPDATE fredbottable SET balance = '{1}' WHERE user_id = {0}", user.Id, bal);
                var reader = database.FireCommand(str);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                return;
            }
        }

        public static int GetBalance(SocketUser user)
        {
            int bal = 0;
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            var tableName = database.FireCommand(str);
            while(tableName.Read())
            {
                bal = (int)tableName["balance"];
            }
            database.CloseConnection();
            return bal;
        }

        public static string GetPR2Name(IUser user)
        {
            string pr2Name = "";
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT * FROM fredbottable WHERE user_id = '{0}'", user.Id);
            var tableName = database.FireCommand(str);
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
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT * FROM fredbottable WHERE pr2_name = '{0}'", pr2name);
            var tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                userID = (string)tableName["user_id"];
            }
            database.CloseConnection();

            return userID;
        }

        public static string AddPrior(IUser user, string username, string type, string moderator, string reason)
        {
            var database = new Database("FredBotDatabase");

            var str = $"INSERT INTO banlog (user_id, username, type, moderator, reason ) VALUES ('{user.Id}', \"{username}\", '{type}', \"{moderator}\", \"{reason}\")";
            var table = database.FireCommand(str);

            database.CloseConnection();

            return null;
        }

        public static string GetCase(string caseN)
        {
            string caseInfo = "";
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT * FROM banlog WHERE `case` = '{0}'", caseN);
            var tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                var type = (string)tableName["type"];
                var userId = (string)tableName["user_id"];
                var username = (string)tableName["username"];
                var moderator = (string)tableName["moderator"];
                var reason = (string)tableName["reason"];

                caseInfo = ($"**Case {caseN}\n    Type: **{type}\n    **User: **({userId}) {username}\n    **Moderator: **{moderator}\n    **Reason: **{reason}");
            }
            database.CloseConnection();
            return caseInfo;
        }

        public static List<string> Modlogs(IUser user)
        {
            List<string> modLogs = new List<string>();
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT * FROM banlog WHERE user_id = '{0}'", user.Id);
            var tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                var caseN = (int)tableName["case"];
                var type = (string)tableName["type"];
                var userId = (string)tableName["user_id"];
                var username = (string)tableName["username"];
                var moderator = (string)tableName["moderator"];
                var reason = (string)tableName["reason"];

                modLogs.Add(item: $"**Case {caseN}\n    Type: **{type}\n    **User: **({userId}){username}\n    **Moderator: **{moderator}\n    **Reason: **{reason}");
            }
            database.CloseConnection();
            return modLogs;
        }

        public static void UpdateReason(string caseN, string reason)
        {
            var database = new Database("FredBotDatabase");
            try
            {
                var str = string.Format("UPDATE banlog SET reason = \"{1}\" WHERE `case` = {0}", caseN, reason);
                var reader = database.FireCommand(str);
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
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT COUNT(*) FROM banlog");
            var tableName = database.FireCommand(str);
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
            var database = new Database("FredBotDatabase");
            if (user == null)
            {
                var str = string.Format("SELECT * FROM banlog WHERE type = 'Warn'");
                var tableName = database.FireCommand(str);
                while (tableName.Read())
                {
                    var caseN = (int)tableName["case"];
                    var type = (string)tableName["type"];
                    var userId = (string)tableName["user_id"];
                    var username = (string)tableName["username"];
                    var moderator = (string)tableName["moderator"];
                    var reason = (string)tableName["reason"];

                    warnings.Add(item: $"**Case {caseN}**\n    **User: **({userId}){username}\n    **Moderator: **{moderator}\n    **Reason: **{reason}");
                }
            }
            else
            {
                var str = string.Format("SELECT * FROM banlog WHERE type = 'Warn' AND user_id = '{0}'", user.Id);
                var tableName = database.FireCommand(str);
                while (tableName.Read())
                {
                    var caseN = (int)tableName["case"];
                    var type = (string)tableName["type"];
                    var userId = (string)tableName["user_id"];
                    var username = (string)tableName["username"];
                    var moderator = (string)tableName["moderator"];
                    var reason = (string)tableName["reason"];

                    warnings.Add(item: $"**Case {caseN}**\n    **Moderator: **{moderator}\n    **Reason: **{reason}");
                }
            }
            database.CloseConnection();
            return warnings;
        }

        public static long WarnCount(IUser user)
        {
            long warnCount = 0;
            var database = new Database("FredBotDatabase");
            var str = string.Format("SELECT COUNT(*) FROM banlog WHERE type = 'Warn' AND user_id = '{0}'",user.Id);
            var tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                warnCount = (long)tableName["COUNT(*)"];
            }
            database.CloseConnection();
            return warnCount;
        }

        public static void ClearWarn(IUser user)
        {
            var database = new Database("FredBotDatabase");
            try
            {
                var str = string.Format("DELETE FROM banlog WHERE type = 'Warn' AND user_id = '{0}'", user.Id);
                var reader = database.FireCommand(str);
                reader.Close();
                database.CloseConnection();
                return;
            }
            catch(Exception)
            {
                database.CloseConnection();
                return;
            }
        }
    }
}
