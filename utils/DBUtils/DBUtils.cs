
using MySqlConnector;
using System.IO;
namespace Utils.SqlConn;

public class DBUtils
{
    // connect to db
    public static MySqlConnection GetDBConnection()
    {
        var config = File.ReadAllText($"/home/narek/Documents/C#/lab10/configs/config.json"); // config.json look like {"DATABASE_URI" : "Server=server;Port=port;Database=database;User=username;Password=password;"}

        dynamic jsonConfig = Newtonsoft.Json.JsonConvert.DeserializeObject(config);

        string databaseURI = jsonConfig.DATABASE_URI;

        return DBMySQLUtils.GetDBConnection(databaseURI);
    }
}
