using MySql.Data.MySqlClient;

namespace FlyVip.Web.Data;

public static class DatabaseConnection
{
    private const uint DefaultPort = 3306;

    private const uint PoolCapPerInstance = 10;

    public static string From(IConfiguration configuration)
    {
        if (configuration["DB_HOST"] is not { Length: > 0 } host)
        {
            return configuration.GetConnectionString("Default")!;
        }

        return new MySqlConnectionStringBuilder
        {
            Server = host,
            Port = configuration.GetValue<uint?>("DB_PORT") ?? DefaultPort,
            UserID = configuration["DB_USER"],
            Password = configuration["DB_PASS"],
            Database = configuration["DB_NAME"],
            SslMode = MySqlSslMode.Required,
            MaximumPoolSize = PoolCapPerInstance,
        }.ConnectionString;
    }
}
