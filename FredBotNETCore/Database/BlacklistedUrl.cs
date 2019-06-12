using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class BlacklistedUrl
    {
        public int ID { get; set; }
        public long GuildID { get; set; }
        public string Url { get; set; }

        public static void Add(ulong guildId, string url)
        {
            Database database = new Database();

            string str = $"INSERT INTO blacklistedurls ( guild_id, url ) VALUES ('{guildId}', '{url}' )";
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static List<BlacklistedUrl> Get(ulong guildId, string url = null)
        {
            List<BlacklistedUrl> urls = new List<BlacklistedUrl>();
            Database database = new Database();

            string str;
            if (url == null)
            {
                str = string.Format("SELECT * FROM blacklistedurls WHERE guild_id = '{0}'", guildId);
            }
            else
            {
                str = string.Format("SELECT * FROM blacklistedurls WHERE guild_id = '{0}' AND url = '{1}'", guildId, url);
            }
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                BlacklistedUrl blacklistedUrl = new BlacklistedUrl()
                {
                    ID = (int)tableName["id"],
                    GuildID = (long)tableName["guild_id"],
                    Url = (string)tableName["url"]
                };

                urls.Add(blacklistedUrl);
            }

            database.CloseConnection();

            return urls;
        }

        public static void Remove(ulong guildId, string url)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("DELETE FROM blacklistedurls WHERE guild_id = '{0}' AND url = '{1}'", guildId, url);
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
