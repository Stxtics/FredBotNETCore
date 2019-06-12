using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class AllowedChannel
    {
        public int ID { get; set; }
        public long GuildID { get; set; }
        public long ChannelID { get; set; }

        public static void Add(ulong guildId, ulong channelId)
        {
            Database database = new Database();

            string str = $"INSERT INTO allowedchannels ( guild_id, channel_id ) VALUES ('{guildId}', '{channelId}' )";
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static List<AllowedChannel> Get(ulong guildId, ulong? channelId = null)
        {
            List<AllowedChannel> channels = new List<AllowedChannel>();
            Database database = new Database();

            string str;
            if (channelId == null)
            {
                str = string.Format("SELECT * FROM allowedchannels WHERE guild_id = '{0}'", guildId);
            }
            else
            {
                str = string.Format("SELECT * FROM allowedchannels WHERE guild_id = '{0}' AND channel_id = '{1}'", guildId, channelId);
            }
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                AllowedChannel channel = new AllowedChannel()
                {
                    ID = (int)tableName["id"],
                    GuildID = (long)tableName["guild_id"],
                    ChannelID = (long)tableName["channel_id"]
                };

                channels.Add(channel);
            }

            database.CloseConnection();

            return channels;
        }

        public static void Remove(ulong guildId, ulong channelId)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("DELETE FROM allowedchannels WHERE guild_id = '{0}' AND channel_id = '{1}'", guildId, channelId);
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
