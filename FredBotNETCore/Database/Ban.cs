using Discord.WebSocket;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class Ban
    {
        public int ID { get; set; }
        public long GuildID { get; set; }
        public int Case { get; set; }
        public long UserID { get; set; }
        public string Username { get; set; }
        public string Type { get; set; }
        public long ModeratorID { get; set; }
        public string Moderator { get; set; }
        public string Reason { get; set; }

        public static void Add(Ban ban)
        {
            Database database = new Database();

            string str = $"INSERT INTO banlog (guild_id, `case`, user_id, username, `type`, moderator_id, moderator, reason ) VALUES ( {ban.GuildID}, {ban.Case}, {ban.UserID}, \"{ban.Username}\", '{ban.Type}', {ban.ModeratorID}, \"{ban.Moderator}\", \"{ban.Reason}\" )";
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static Ban GetCase(string type, string value)
        {
            Database database = new Database();
            Ban ban = new Ban();
            string str = string.Format("SELECT * FROM banlog WHERE {0} = '{1}' LIMIT 1", type, value);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                ban.ID = (int)tableName["id"];
                ban.GuildID = (long)tableName["guild_id"];
                ban.Case = (int)tableName["case"];
                ban.UserID = (long)tableName["user_id"];
                ban.Username = (string)tableName["username"];
                ban.Type = (string)tableName["type"];
                ban.ModeratorID = (long)tableName["moderator_id"];
                ban.Moderator = (string)tableName["moderator"];
                ban.Reason = (string)tableName["reason"];
            }
            database.CloseConnection();

            return ban;
        }

        public static List<Ban> GetPriors(ulong guildId, SocketUser user)
        {
            List<Ban> priors = new List<Ban>();
            Database database = new Database();
            string str = string.Format("SELECT * FROM banlog WHERE guild_id = '{0}' AND user_id = '{1}'", guildId, user.Id);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                Ban ban = new Ban()
                {
                    ID = (int)tableName["id"],
                    GuildID = (long)tableName["guild_id"],
                    Case = (int)tableName["case"],
                    UserID = (long)tableName["user_id"],
                    Username = (string)tableName["username"],
                    Type = (string)tableName["type"],
                    ModeratorID = (long)tableName["moderator_id"],
                    Moderator = (string)tableName["moderator"],
                    Reason = (string)tableName["reason"]
                };
                priors.Add(ban);
            }
            database.CloseConnection();
            return priors;
        }

        public static void SetValue(string caseN, ulong guildId, string type, string value)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("UPDATE banlog SET {2} = \"{3}\" WHERE `case` = '{0}' AND guild_id = '{1}' LIMIT 1", caseN, guildId, type, value);
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

        public static int CaseCount(ulong guildId)
        {
            int cases = 0;
            Database database = new Database();
            string str = string.Format("SELECT COUNT(*) FROM banlog WHERE guild_id = '{0}'", guildId);
            MySqlDataReader tableName = database.FireCommand(str);
            while (tableName.Read())
            {
                cases = int.Parse(((long)tableName["COUNT(*)"]).ToString());
            }
            database.CloseConnection();
            return cases;
        }

        public static List<Ban> Warnings(ulong guildId, SocketUser user = null)
        {
            List<Ban> warnings = new List<Ban>();
            Database database = new Database();
            if (user == null)
            {
                string str = string.Format("SELECT * FROM banlog WHERE type = 'Warn' AND guild_id = '{0}'", guildId);
                MySqlDataReader tableName = database.FireCommand(str);
                while (tableName.Read())
                {
                    Ban ban = new Ban()
                    {
                        ID = (int)tableName["id"],
                        GuildID = (long)tableName["guild_id"],
                        Case = (int)tableName["case"],
                        UserID = (long)tableName["user_id"],
                        Username = (string)tableName["username"],
                        Type = (string)tableName["type"],
                        ModeratorID = (long)tableName["moderator_id"],
                        Moderator = (string)tableName["moderator"],
                        Reason = (string)tableName["reason"]
                    };

                    warnings.Add(ban);
                }
            }
            else
            {
                string str = string.Format("SELECT * FROM banlog WHERE type = 'Warn' AND guild_id = '{1}' AND user_id = '{0}'", user.Id, guildId);
                MySqlDataReader tableName = database.FireCommand(str);
                while (tableName.Read())
                {
                    Ban ban = new Ban()
                    {
                        ID = (int)tableName["id"],
                        GuildID = (long)tableName["guild_id"],
                        Case = (int)tableName["case"],
                        UserID = (long)tableName["user_id"],
                        Username = (string)tableName["username"],
                        Type = (string)tableName["type"],
                        ModeratorID = (long)tableName["moderator_id"],
                        Moderator = (string)tableName["moderator"],
                        Reason = (string)tableName["reason"]
                    };

                    warnings.Add(ban);
                }
            }
            database.CloseConnection();

            return warnings;
        }
    }
}
