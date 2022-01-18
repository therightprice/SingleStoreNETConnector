using System.Transactions;

namespace SideBySide;

public class TransactionScopeTests : IClassFixture<DatabaseFixture>
{
	public TransactionScopeTests(DatabaseFixture database)
	{
		m_database = database;
	}

	public static IEnumerable<object[]> ConnectionStrings = new[]
	{
#if BASELINE
		new object[] { "" },
#else
		new object[] { "UseXaTransactions=False" },
		new object[] { "UseXaTransactions=True" },
#endif
	};

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void EnlistTransactionWhenClosed(string connectionString)
	{
		using (new TransactionScope())
		using (var connection = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString))
		{
#if !BASELINE
			Assert.Throws<InvalidOperationException>(() => connection.EnlistTransaction(System.Transactions.Transaction.Current));
#else
			Assert.Throws<NullReferenceException>(() => connection.EnlistTransaction(System.Transactions.Transaction.Current));
#endif
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void EnlistSameTransaction(string connectionString)
	{
		using (new TransactionScope())
		using (var connection = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString))
		{
			connection.Open();
			connection.EnlistTransaction(System.Transactions.Transaction.Current);
			connection.EnlistTransaction(System.Transactions.Transaction.Current);
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void EnlistTwoTransactions(string connectionString)
	{
		using var connection = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString);
		connection.Open();

		using var transaction1 = new CommittableTransaction();
		using var transaction2 = new CommittableTransaction();
		connection.EnlistTransaction(transaction1);
		Assert.Throws<SingleStoreException>(() => connection.EnlistTransaction(transaction2));
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void BeginTransactionInScope(string connectionString)
	{
		using var transactionScope = new TransactionScope();
		using var connection = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString);
		connection.Open();
		Assert.Throws<InvalidOperationException>(() => connection.BeginTransaction());
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void BeginTransactionThenEnlist(string connectionString)
	{
		using var connection = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString);
		connection.Open();

		using var dbTransaction = connection.BeginTransaction();
		using var transaction = new CommittableTransaction();
		Assert.Throws<InvalidOperationException>(() => connection.EnlistTransaction(transaction));
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void CommitOneTransactionWithTransactionScope(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		using (var transactionScope = new TransactionScope())
		{
			using var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString);
			conn.Open();
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);");

			transactionScope.Complete();
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new[] { 1, 2 }, values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void CommitOneTransactionWithEnlistTransaction(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		using (var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString))
		{
			conn.Open();
			using var transaction = new CommittableTransaction();
			conn.EnlistTransaction(transaction);
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);");
			transaction.Commit();
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new[] { 1, 2 }, values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void RollBackOneTransactionWithTransactionScope(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		using (var transactionScope = new TransactionScope())
		{
			using var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString);
			conn.Open();
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);");
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new int[0], values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void RollBackOneTransactionWithEnlistTransaction(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		using (var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString))
		{
			conn.Open();
			using var transaction = new CommittableTransaction();
			conn.EnlistTransaction(transaction);
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);");
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new int[0], values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void ThrowExceptionInTransaction(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		try
		{
			using var transactionScope = new TransactionScope();
			using var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString);
			conn.Open();
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);");

			throw new ApplicationException();
		}
		catch (ApplicationException)
		{
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new int[0], values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void ThrowExceptionAfterCompleteInTransaction(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		try
		{
			using var transactionScope = new TransactionScope();
			using var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString);
			conn.Open();
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);");

			transactionScope.Complete();

			throw new ApplicationException();
		}
		catch (ApplicationException)
		{
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new[] { 1, 2 }, values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void AutoEnlistFalseWithCommit(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		using (var transactionScope = new TransactionScope())
		using (var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";auto enlist=false;" + connectionString))
		{
			conn.Open();
			using var dbTransaction = conn.BeginTransaction();
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);", transaction: dbTransaction);

			dbTransaction.Commit();
			transactionScope.Complete();
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new[] { 1, 2 }, values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void AutoEnlistFalseWithoutCommit(string connectionString)
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test;
			create table transaction_scope_test(value integer not null);");

		using (var transactionScope = new TransactionScope())
		using (var conn = new SingleStoreConnection(AppConfig.ConnectionString + ";auto enlist=false;" + connectionString))
		{
			conn.Open();
			using var dbTransaction = conn.BeginTransaction();
			conn.Execute("insert into transaction_scope_test(value) values(1), (2);", transaction: dbTransaction);

#if BASELINE
			// With Connector/NET a SingleStoreTransaction can't roll back after TransactionScope has been completed;
			// workaround is to explicitly dispose it first. In SingleStoreConnector (with AutoEnlist=false) they have
			// independent lifetimes.
			dbTransaction.Dispose();
#endif
			transactionScope.Complete();
		}

		var values = m_database.Connection.Query<int>(@"select value from transaction_scope_test order by value;").ToList();
		Assert.Equal(new int[0], values);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void UsingSequentialConnectionsInOneTransactionDoesNotDeadlock(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		var transactionOptions = new TransactionOptions
		{
			IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
			Timeout = TransactionManager.MaximumTimeout
		};
		using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
			}

			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}

			scope.Complete();
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three", "new value", "five", "six" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void UsingSequentialConnectionsInOneTransactionDoesNotDeadlockWithoutComplete(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		var transactionOptions = new TransactionOptions
		{
			IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
			Timeout = TransactionManager.MaximumTimeout
		};
		using (new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
			}

			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void UsingSequentialConnectionsInOneTransactionWithoutAutoEnlistDoesNotDeadlock(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";AutoEnlist=false;" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		using (var transaction = new CommittableTransaction())
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
			}

			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}

			transaction.Commit();
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three", "new value", "five", "six" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void UsingSequentialConnectionsInOneTransactionWithoutAutoEnlistDoesNotDeadlockWithRollback(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";AutoEnlist=false;" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		using (var transaction = new CommittableTransaction())
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
			}

			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}

			transaction.Rollback();
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void UsingSequentialConnectionsInOneTransactionReusesPhysicalConnection(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";AutoEnlist=false;" + connectionString;
		using var transaction = new CommittableTransaction();
		using var connection1 = new SingleStoreConnection(connectionString);
		connection1.Open();
		connection1.EnlistTransaction(transaction);
		var sessionId1 = connection1.ServerThread;

		using var connection2 = new SingleStoreConnection(connectionString);
		connection2.Open();
		Assert.NotEqual(sessionId1, connection2.ServerThread);

		connection1.Close();
		connection2.EnlistTransaction(transaction);
		Assert.Equal(sessionId1, connection2.ServerThread);
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void ReusingConnectionInOneTransactionDoesNotDeadlock(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		var transactionOptions = new TransactionOptions
		{
			IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
			Timeout = TransactionManager.MaximumTimeout
		};
		using (var scope = new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
				connection.Close();

				connection.Open();
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}

			scope.Complete();
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three", "new value", "five", "six" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void ReusingConnectionInOneTransactionDoesNotDeadlockWithoutComplete(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		var transactionOptions = new TransactionOptions
		{
			IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
			Timeout = TransactionManager.MaximumTimeout
		};
		using (new TransactionScope(TransactionScopeOption.Required, transactionOptions, TransactionScopeAsyncFlowOption.Enabled))
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
				connection.Close();

				connection.Open();
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void ReusingConnectionInOneTransactionWithoutAutoEnlistDoesNotDeadlock(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";AutoEnlist=false;" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		using (var transaction = new CommittableTransaction())
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
				connection.Close();

				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}

			transaction.Commit();
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three", "new value", "five", "six" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public void ReusingConnectionInOneTransactionWithoutAutoEnlistDoesNotDeadlockWithRollback(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";AutoEnlist=false;" + connectionString;
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"drop table if exists transaction_scope_test;
create table transaction_scope_test(rowid integer not null auto_increment primary key, value text);
insert into transaction_scope_test(value) values('one'),('two'),('three');");
		}

		using (var transaction = new CommittableTransaction())
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("insert into transaction_scope_test(value) values('four'),('five'),('six');");
				connection.Close();

				connection.Open();
				connection.EnlistTransaction(transaction);
				connection.Execute("update transaction_scope_test set value = @newValue where rowid = @id", new { newValue = "new value", id = 4 });
			}

			transaction.Rollback();
		}

		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			Assert.Equal(new[] { "one", "two", "three" }, connection.Query<string>(@"select value from transaction_scope_test order by rowid;"));
		}
	}

	[SkippableTheory(Baseline = "Different results")]
	[MemberData(nameof(ConnectionStrings))]
	public void ReusingConnectionInOneTransactionReusesPhysicalConnection(string connectionString)
	{
		connectionString = AppConfig.ConnectionString + ";" + connectionString;
		using (var transactionScope = new TransactionScope())
		{
			using (var connection = new SingleStoreConnection(connectionString))
			{
				connection.Open();
				var sessionId = connection.ServerThread;
				connection.Close();

				connection.Open();
				Assert.Equal(sessionId, connection.ServerThread);
			}
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public async Task CommandBehaviorCloseConnection(string connectionString)
	{
		using (var connection = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString))
		using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
		{
			using (var command1 = new SingleStoreCommand("SELECT 1"))
			using (var command2 = new SingleStoreCommand("SELECT 2"))
			{
				command1.Connection = connection;
				command2.Connection = connection;

				await connection.OpenAsync();
				using (var reader = await command1.ExecuteReaderAsync(CommandBehavior.CloseConnection, CancellationToken.None))
				{
					Assert.True(await reader.ReadAsync());
					Assert.Equal(1, reader.GetInt32(0));
					Assert.False(await reader.ReadAsync());
				}

				Assert.Equal(ConnectionState.Closed, connection.State);

				await connection.OpenAsync();
				using (var reader = await command2.ExecuteReaderAsync(CommandBehavior.CloseConnection, CancellationToken.None))
				{
					Assert.True(await reader.ReadAsync());
					Assert.Equal(2, reader.GetInt32(0));
					Assert.False(await reader.ReadAsync());
				}
			}
		}
	}

	[Theory]
	[MemberData(nameof(ConnectionStrings))]
	public async Task CancelExecuteNonQueryAsync(string connectionString)
	{
		using var connection = new SingleStoreConnection(AppConfig.ConnectionString + ";" + connectionString + ";AllowUserVariables=True");
		using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
		await connection.OpenAsync();

		using var command = new SingleStoreCommand("SELECT SLEEP(3) INTO @dummy", connection);
		using var tokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
		await command.ExecuteNonQueryAsync(tokenSource.Token);
	}

	[SkippableFact(Baseline = "Multiple simultaneous connections or connections with different connection strings inside the same transaction are not currently supported.")]
	public void CommitTwoTransactions()
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test_1;
			drop table if exists transaction_scope_test_2;
			create table transaction_scope_test_1(value integer not null);
			create table transaction_scope_test_2(value integer not null);");

		using (var transactionScope = new TransactionScope())
		{
			using var conn1 = new SingleStoreConnection(AppConfig.ConnectionString);
			conn1.Open();
			conn1.Execute("insert into transaction_scope_test_1(value) values(1), (2);");

			using var conn2 = new SingleStoreConnection(AppConfig.ConnectionString);
			conn2.Open();
			conn2.Execute("insert into transaction_scope_test_2(value) values(3), (4);");

			transactionScope.Complete();
		}

		var values1 = m_database.Connection.Query<int>(@"select value from transaction_scope_test_1 order by value;").ToList();
		var values2 = m_database.Connection.Query<int>(@"select value from transaction_scope_test_2 order by value;").ToList();
		Assert.Equal(new[] { 1, 2 }, values1);
		Assert.Equal(new[] { 3, 4 }, values2);
	}

	[SkippableFact(Baseline = "Multiple simultaneous connections or connections with different connection strings inside the same transaction are not currently supported.")]
	public void RollBackTwoTransactions()
	{
		m_database.Connection.Execute(@"drop table if exists transaction_scope_test_1;
			drop table if exists transaction_scope_test_2;
			create table transaction_scope_test_1(value integer not null);
			create table transaction_scope_test_2(value integer not null);");

		using (var transactionScope = new TransactionScope())
		{
			using var conn1 = new SingleStoreConnection(AppConfig.ConnectionString);
			conn1.Open();
			conn1.Execute("insert into transaction_scope_test_1(value) values(1), (2);");

			using var conn2 = new SingleStoreConnection(AppConfig.ConnectionString);
			conn2.Open();
			conn2.Execute("insert into transaction_scope_test_2(value) values(3), (4);");
		}

		var values1 = m_database.Connection.Query<int>(@"select value from transaction_scope_test_1 order by value;").ToList();
		var values2 = m_database.Connection.Query<int>(@"select value from transaction_scope_test_2 order by value;").ToList();
		Assert.Equal(new int[0], values1);
		Assert.Equal(new int[0], values2);
	}

	[Fact]
	public void TwoSimultaneousConnectionsThrowsWithNonXaTransactions()
	{
		var connectionString = AppConfig.ConnectionString;
#if !BASELINE
		connectionString += ";UseXaTransactions=False";
#endif

		using (new TransactionScope())
		{
			using var conn1 = new SingleStoreConnection(connectionString);
			conn1.Open();

			using var conn2 = new SingleStoreConnection(connectionString);
			Assert.Throws<NotSupportedException>(() => conn2.Open());
		}
	}

	[Fact]
	public void SimultaneousConnectionsWithTransactionScopeReadCommittedWithNonXaTransactions()
	{
		var connectionString = AppConfig.ConnectionString;
#if !BASELINE
		connectionString += ";UseXaTransactions=False";
#endif

		// from https://github.com/mysql-net/MySqlConnector/issues/605
		using (var connection = new SingleStoreConnection(connectionString))
		{
			connection.Open();
			connection.Execute(@"DROP TABLE IF EXISTS orders;
				CREATE TABLE `orders`(
					`id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
					`description` VARCHAR(50),
					PRIMARY KEY (`id`)
				);");
		}

		var task = Task.Run(() => UseTransaction());
		UseTransaction();
		task.Wait();

		void UseTransaction()
		{
			// This TransactionScope may be overly configured, but let's stick with the one I am actually using
			using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
				new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
				TransactionScopeAsyncFlowOption.Enabled))
			{
				using (var connection = new SingleStoreConnection(connectionString))
				using (var command = connection.CreateCommand())
				{
					command.CommandText = @"SELECT MAX(id) FROM orders FOR UPDATE;";
					connection.Open();
					command.ExecuteScalar();
				}

				using (var connection = new SingleStoreConnection(connectionString))
				using (var command = connection.CreateCommand())
				{
					command.CommandText = @"INSERT INTO orders (description) VALUES ('blabla'), ('blablabla');";
					connection.Open();
					command.ExecuteNonQuery();
				}

				transactionScope.Complete();
			}
		}
	}

	[Fact]
	public void TwoDifferentConnectionStringsThrowsWithNonXaTransactions()
	{
		var connectionString = AppConfig.ConnectionString;
#if !BASELINE
		connectionString += ";UseXaTransactions=False";
#endif

		using (new TransactionScope())
		{
			using var conn1 = new SingleStoreConnection(connectionString);
			conn1.Open();

			using var conn2 = new SingleStoreConnection(connectionString + ";MaxPoolSize=6");
			Assert.Throws<NotSupportedException>(() => conn2.Open());
		}
	}

#if !BASELINE
	[Fact]
	public void CannotMixXaAndNonXaTransactions()
	{
		using (new TransactionScope())
		{
			using var conn1 = new SingleStoreConnection(AppConfig.ConnectionString);
			conn1.Open();

			using var conn2 = new SingleStoreConnection(AppConfig.ConnectionString + ";UseXaTransactions=False");
			Assert.Throws<NotSupportedException>(() => conn2.Open());
		}
	}

	[Fact]
	public void CannotMixNonXaAndXaTransactions()
	{
		using (new TransactionScope())
		{
			using var conn1 = new SingleStoreConnection(AppConfig.ConnectionString + ";UseXaTransactions=False");
			conn1.Open();

			using var conn2 = new SingleStoreConnection(AppConfig.ConnectionString);
			Assert.Throws<NotSupportedException>(() => conn2.Open());
		}
	}
#endif

	DatabaseFixture m_database;
}

