using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class JoinableRole
    {
        public int ID { get; set; }
        public long GuildID { get; set; }
        public long RoleID { get; set; }
        public string Description { get; set; }

        public static void Add(ulong guildId, ulong roleId, string description)
        {
            Database database = new Database();

            string str = $"INSERT INTO joinableroles ( guild_id, role_id, description ) VALUES ('{guildId}', '{roleId}', \"{description}\" )";
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static List<JoinableRole> Get(ulong guildId, ulong? roleId = null)
        {
            List<JoinableRole> roles = new List<JoinableRole>();
            Database database = new Database();

            string str;
            if (roleId == null)
            {
                str = string.Format("SELECT * FROM joinableroles WHERE guild_id = '{0}'", guildId);
            }
            else
            {
                str = string.Format("SELECT * FROM joinableroles WHERE guild_id = '{0}' AND role_id = '{1}'", guildId, roleId);
            }
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                JoinableRole role = new JoinableRole()
                {
                    ID = (int)tableName["id"],
                    GuildID = (long)tableName["guild_id"],
                    RoleID = (long)tableName["role_id"],
                    Description = (string)tableName["description"]
                };
                roles.Add(role);
            }

            database.CloseConnection();

            return roles;
        }

        public static void Remove(ulong guildId, ulong roleId)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("DELETE FROM joinableroles WHERE guild_id = '{0}' AND role_id = '{1}'", guildId, roleId);
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
