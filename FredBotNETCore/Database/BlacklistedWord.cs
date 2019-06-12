using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FredBotNETCore.Database
{
    public class BlacklistedWord
    {
        public int ID { get; set; }
        public long GuildID { get; set; }
        public string Word { get; set; }

        public static void Add(ulong guildId, string word)
        {
            Database database = new Database();

            string str = $"INSERT INTO blacklistedwords ( guild_id, word ) VALUES ('{guildId}', '{word}' )";
            _ = database.FireCommand(str);

            database.CloseConnection();
        }

        public static List<BlacklistedWord> Get(ulong guildId, string word = null)
        {
            List<BlacklistedWord> words = new List<BlacklistedWord>();
            Database database = new Database();

            string str;
            if (word == null)
            {
                str = string.Format("SELECT * FROM blacklistedwords WHERE guild_id = '{0}'", guildId);
            }
            else
            {
                str = string.Format("SELECT * FROM blacklistedwords WHERE guild_id = '{0}' AND word = '{1}'", guildId, word);
            }
            MySqlDataReader tableName = database.FireCommand(str);

            while (tableName.Read())
            {
                BlacklistedWord blacklistedWord = new BlacklistedWord()
                {
                    ID = (int)tableName["id"],
                    GuildID = (long)tableName["guild_id"],
                    Word = (string)tableName["word"]
                };

                words.Add(blacklistedWord);
            }

            database.CloseConnection();

            return words;
        }

        public static void Remove(ulong guildId, string word)
        {
            Database database = new Database();
            try
            {
                string str = string.Format("DELETE FROM blacklistedwords WHERE guild_id = '{0}' AND word = '{1}'", guildId, word);
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
