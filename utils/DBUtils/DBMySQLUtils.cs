using MySqlConnector;

namespace Utils.SqlConn;

public class DBMySQLUtils
{
    public static MySqlConnection GetDBConnection(string connectionString)
    {

        return new MySqlConnection(connectionString); // no comment
    }
}