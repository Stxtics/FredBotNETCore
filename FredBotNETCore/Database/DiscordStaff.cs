using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class DiscordStaff
    {
        public int ID { get; set; }
        public long GuildID { get; set; }
        public string StaffID { get; set; }

        public static void Add(ulong guildId, string staffId)
        {
            Database database = new Database();

            string str = $"INSERT INTO discordstaff ( guild_id, staff_id ) VALUES ('{guildId}', '{staffId}' )";
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static List<DiscordStaff> Get(ulong guildId, string staffId = null)
        {
            List<DiscordStaff> staff = new List<DiscordStaff>();
            Database database = new Database();

            string str;
            if (staffId == null)
            {
                str = string.Format("SELECT * FROM discordstaff WHERE guild_id = '{0}'", guildId);
            }
            else
            {
                str = string.Format("SELECT * FROM discordstaff WHERE guild_id = '{0}' AND staff_id = '{1}'", guildId, staffId);
            }
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                DiscordStaff discordStaff = new DiscordStaff()
                {
                    ID = (int)tableName["id"],
                    GuildID = (long)tableName["guild_id"],
                    StaffID = (string)tableName["staff_id"]
                };

                staff.Add(discordStaff);
            }

            database.CloseConnection();

            return staff;
        }

        public static void Remove(ulong guildId, string staffId)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("DELETE FROM discordstaff WHERE guild_id = '{0}' AND staff_id = '{1}'", guildId, staffId);
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
