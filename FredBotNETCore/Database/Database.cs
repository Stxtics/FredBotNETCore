using MySql.Data.MySqlClient;
using System.IO;

namespace FredBotNETCore.Database
{
    public class Database
    {
        private readonly string server = File.ReadAllText(Path.Combine(Extensions.downloadPath, "ServerIP.txt"));
        private const string database = "FredBotDatabase";
        private const string username = "stxtics";
        private readonly string password = File.ReadAllText(Path.Combine(Extensions.downloadPath, "DatabasePassword.txt"));
        private readonly MySqlConnection dbConnection;

        public Database()
        {
            MySqlConnectionStringBuilder stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = server,
                UserID = username,
                Password = password,
                Database = database,
                Port = 35898,
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
    }
}
