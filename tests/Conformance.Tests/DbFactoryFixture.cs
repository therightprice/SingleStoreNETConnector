using System;
using System.Data.Common;
using AdoNet.Specification.Tests;
using SingleStoreConnector;

namespace Conformance.Tests;

public class DbFactoryFixture : IDbFactoryFixture
	{
		public DbFactoryFixture()
		{
			ConnectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "Server=localhost;User Id=root;Password=pass;SSL Mode=None";
		}

		public string ConnectionString { get; }
		public DbProviderFactory Factory => MySqlConnectorFactory.Instance;
	}
