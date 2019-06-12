using Discord.WebSocket;
using MySql.Data.MySqlClient;
using System;

namespace FredBotNETCore.Database
{
    public class Guild
    {
        public long GuildID { get; set; }
        public string GuildName { get; set; }
        public string Prefix { get; set; }
        public long? LogChannel { get; set; }
        public long? BanlogChannel { get; set; }
        public long? NotificationsChannel { get; set; }

        public static bool Exists(SocketGuild guild)
        {
            Database database = new Database();

            string str = string.Format("SELECT * FROM guilds WHERE guild_id = '{0}' LIMIT 1", guild.Id);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                long guildId = (long)tableName["guild_id"];

                if (guildId != 0)
                {
                    database.CloseConnection();
                    return true;
                }
            }

            return false;
        }

        public static void Add(SocketGuild guild)
        {
            Database database = new Database();

            string str = string.Format("INSERT INTO guilds (guild_id, guild_name ) VALUES ('{0}', \"{1}\")", guild.Id, guild.Name);
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static Guild Get(SocketGuild dGuild)
        {
            Database database = new Database();
            string str = string.Format("SELECT * FROM guilds WHERE guild_id = '{0}'", dGuild.Id);
            MySqlDataReader tableName = database.FireCommand(str);
            Guild guild = new Guild();
            while (tableName.Read())
            {
                guild.GuildID = (long)tableName["guild_id"];
                guild.GuildName = (string)tableName["guild_name"];
                guild.Prefix = (string)tableName["prefix"];
                if (DBNull.Value.Equals(tableName["log_channel"]))
                {
                    guild.LogChannel = null;
                }
                else
                {
                    guild.LogChannel = (long)tableName["log_channel"];
                }
                if (DBNull.Value.Equals(tableName["banlog_channel"]))
                {
                    guild.BanlogChannel = null;
                }
                else
                {
                    guild.BanlogChannel = (long)tableName["banlog_channel"];
                }
                if (DBNull.Value.Equals(tableName["notifications_channel"]))
                {
                    guild.NotificationsChannel = null;
                }
                else
                {
                    guild.NotificationsChannel = (long)tableName["notifications_channel"];
                }
            }
            database.CloseConnection();
            return guild;
        }

        public static void SetLogChannel(SocketGuild guild, ulong id)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("UPDATE guilds SET log_channel = '{1}' WHERE guild_id = {0} LIMIT 1", guild.Id, id);
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

        public static void SetBanlogChannel(SocketGuild guild, ulong id)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("UPDATE guilds SET banlog_channel = '{1}' WHERE guild_id = {0} LIMIT 1", guild.Id, id);
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

        public static void SetNotificationsChannel(SocketGuild guild, ulong id)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("UPDATE guilds SET notifications_channel = '{1}' WHERE guild_id = {0} LIMIT 1", guild.Id, id);
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

        public static void SetPrefix(SocketGuild guild, char p)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("UPDATE guilds SET prefix = \"{1}\" WHERE guild_id = {0} LIMIT 1", guild.Id, p);
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
