using Microsoft.Data.Sqlite;

namespace SwiftAPI.Data
{
    public interface ISqliteConnectionFactory
    {
        SqliteConnection CreateConnection();
    }
}
