
using MySqlConnector;
using System.IO;
namespace Utils.SqlConn;

public class DBUtils
{
    // Подключается к бд
    public static MySqlConnection GetDBConnection()
    {
        var config = File.ReadAllText($"/home/narek/Documents/C#/lab10/configs/config.json");

        dynamic jsonConfig = Newtonsoft.Json.JsonConvert.DeserializeObject(config);

        string databaseURI = jsonConfig.DATABASE_URI;

        return DBMySQLUtils.GetDBConnection(databaseURI);
    }
}
