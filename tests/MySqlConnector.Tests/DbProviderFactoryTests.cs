#if BASELINE
using MySql.Data.MySqlClient;
using MySqlConnectorFactory = MySql.Data.MySqlClient.MySqlClientFactory;
#endif
using Xunit;

namespace SingleStoreConnector.Tests;

public class DbProviderFactoryTests
{
	[Fact]
	public void CreatesExpectedTypes()
	{
		Assert.IsType<SingleStoreConnection>(MySqlConnectorFactory.Instance.CreateConnection());
		Assert.IsType<MySqlConnectionStringBuilder>(MySqlConnectorFactory.Instance.CreateConnectionStringBuilder());
		Assert.IsType<SingleStoreCommand>(MySqlConnectorFactory.Instance.CreateCommand());
		Assert.IsType<MySqlCommandBuilder>(MySqlConnectorFactory.Instance.CreateCommandBuilder());
		Assert.IsType<SingleStoreDataAdapter>(MySqlConnectorFactory.Instance.CreateDataAdapter());
		Assert.IsType<MySqlParameter>(MySqlConnectorFactory.Instance.CreateParameter());
	}

	[Fact]
	public void CanCreateDataSourceEnumerator() => Assert.False(MySqlConnectorFactory.Instance.CanCreateDataSourceEnumerator);

	[Fact]
	public void Singleton()
	{
		var factory1 = MySqlConnectorFactory.Instance;
		var factory2 = MySqlConnectorFactory.Instance;
		Assert.True(object.ReferenceEquals(factory1, factory2));
	}
}
