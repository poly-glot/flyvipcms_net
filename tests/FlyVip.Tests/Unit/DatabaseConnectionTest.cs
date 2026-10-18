using FlyVip.Web.Data;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace FlyVip.Tests.Unit;

public sealed class DatabaseConnectionTest
{
    private const string LocalDefault = "Server=127.0.0.1;Port=13306;Database=flyvip;User Id=root;Password=local";

    [Fact]
    public void DbHostBuildsTlsConnectionWithPoolCap()
    {
        var configuration = Configure(("ConnectionStrings:Default", LocalDefault), ("DB_HOST", "db.example.test"), ("DB_PORT", "4406"), ("DB_USER", "flyvipcms-net"), ("DB_PASS", "p;ss=\"w'rd"), ("DB_NAME", "flyvipcms_net"));

        var connection = new MySqlConnectionStringBuilder(DatabaseConnection.From(configuration));

        Assert.Equal("db.example.test", connection.Server);
        Assert.Equal(4406u, connection.Port);
        Assert.Equal("flyvipcms-net", connection.UserID);
        Assert.Equal("p;ss=\"w'rd", connection.Password);
        Assert.Equal("flyvipcms_net", connection.Database);
        Assert.Equal(MySqlSslMode.Required, connection.SslMode);
        Assert.Equal(10u, connection.MaximumPoolSize);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void PortDefaultsTo3306(string? port)
    {
        var configuration = Configure(("DB_HOST", "db.example.test"), ("DB_USER", "u"), ("DB_PASS", "p"), ("DB_NAME", "n"), ("DB_PORT", port));

        Assert.Equal(3306u, new MySqlConnectionStringBuilder(DatabaseConnection.From(configuration)).Port);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WithoutDbHostTheDefaultConnectionStringIsUsed(string? host)
    {
        var configuration = Configure(("ConnectionStrings:Default", LocalDefault), ("DB_HOST", host), ("DB_USER", "ignored"));

        Assert.Equal(LocalDefault, DatabaseConnection.From(configuration));
    }

    private static IConfiguration Configure(params (string Key, string? Value)[] settings) =>
        new ConfigurationBuilder().AddInMemoryCollection(settings.Select(setting => KeyValuePair.Create(setting.Key, setting.Value))).Build();
}
