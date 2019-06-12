using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class MutedUser
    {
        public int ID { get; set; }
        public long GuildID { get; set; }
        public long UserID { get; set; }

        public static void Add(ulong guildId, ulong userId)
        {
            Database database = new Database();

            string str = $"INSERT INTO mutedusers ( guild_id, user_id ) VALUES ('{guildId}', '{userId}' )";
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static List<MutedUser> Get(ulong guildId, ulong userId)
        {
            List<MutedUser> mutedUsers = new List<MutedUser>();
            Database database = new Database();

            string str = string.Format("SELECT * FROM mutedusers WHERE guild_id = '{0}' AND user_id = '{1}'", guildId, userId);
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                MutedUser mutedUser = new MutedUser()
                {
                    ID = (int)tableName["id"],
                    GuildID = (long)tableName["guild_id"],
                    UserID = (long)tableName["user_id"]
                };

                mutedUsers.Add(mutedUser);
            }

            database.CloseConnection();

            return mutedUsers;
        }

        public static void Remove(ulong guildId, ulong userId)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("DELETE FROM mutedusers WHERE guild_id = '{0}' AND user_id = '{1}'", guildId, userId);
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
