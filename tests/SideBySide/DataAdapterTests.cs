namespace SideBySide;

public class DataAdapterTests : IClassFixture<DatabaseFixture>, IDisposable
{
		public DataAdapterTests(DatabaseFixture database)
		{
			m_connection = database.Connection;
			m_connection.Open();

#if BASELINE
			// not sure why this is necessary
			m_connection.Execute("drop table if exists data_adapter;");
#endif

			m_connection.Execute(@"
create temporary table data_adapter(
	id bigint not null primary key auto_increment,
	int_value int null,
	text_value text null
);
insert into data_adapter(int_value, text_value) values
(null, null),
(0, ''),
(1, 'one');
");
		}

		public void Dispose()
		{
			m_connection.Close();
		}

		[Fact]
		public void UseDataAdapter()
		{
			using var command = new SingleStoreCommand("SELECT 1", m_connection);
			using var da = new SingleStoreDataAdapter();
			using var ds = new DataSet();
			da.SelectCommand = command;
			da.Fill(ds);
			Assert.Single(ds.Tables);
			Assert.Single(ds.Tables[0].Rows);
			Assert.Single(ds.Tables[0].Rows[0].ItemArray);
			TestUtilities.AssertIsOne(ds.Tables[0].Rows[0][0]);
		}

		[Fact]
		public void UseDataAdapterMySqlConnectionConstructor()
		{
			using var command = new SingleStoreCommand("SELECT 1", m_connection);
			using var da = new SingleStoreDataAdapter(command);
			using var ds = new DataSet();
			da.Fill(ds);
			TestUtilities.AssertIsOne(ds.Tables[0].Rows[0][0]);
		}

		[Fact]
		public void UseDataAdapterStringMySqlConnectionConstructor()
		{
			using var da = new SingleStoreDataAdapter("SELECT 1", m_connection);
			using var ds = new DataSet();
			da.Fill(ds);
			TestUtilities.AssertIsOne(ds.Tables[0].Rows[0][0]);
		}

		[Fact]
		public void UseDataAdapterStringStringConstructor()
		{
			using var da = new SingleStoreDataAdapter("SELECT 1", AppConfig.ConnectionString);
			using var ds = new DataSet();
			da.Fill(ds);
			TestUtilities.AssertIsOne(ds.Tables[0].Rows[0][0]);
		}

		[Fact]
		public void Fill()
		{
			using var da = new SingleStoreDataAdapter("select * from data_adapter", m_connection);
			using var ds = new DataSet();
			da.Fill(ds, "data_adapter");

			Assert.Single(ds.Tables);
			Assert.Equal(3, ds.Tables[0].Rows.Count);

			Assert.Equal(1L, ds.Tables[0].Rows[0]["id"]);
			Assert.Equal(2L, ds.Tables[0].Rows[1]["id"]);
			Assert.Equal(3L, ds.Tables[0].Rows[2]["id"]);

			Assert.Equal(DBNull.Value, ds.Tables[0].Rows[0]["int_value"]);
			Assert.Equal(0, ds.Tables[0].Rows[1]["int_value"]);
			Assert.Equal(1, ds.Tables[0].Rows[2]["int_value"]);

			Assert.Equal(DBNull.Value, ds.Tables[0].Rows[0]["text_value"]);
			Assert.Equal("", ds.Tables[0].Rows[1]["text_value"]);
			Assert.Equal("one", ds.Tables[0].Rows[2]["text_value"]);
		}

		[Fact]
		public void LoadDataTable()
		{
			using var command = new SingleStoreCommand("SELECT * FROM data_adapter", m_connection);
			using var dr = command.ExecuteReader();
			var dt = new DataTable();
			dt.Load(dr);
			dr.Close();

			Assert.Equal(3, dt.Rows.Count);

			Assert.Equal(1L, dt.Rows[0]["id"]);
			Assert.Equal(2L, dt.Rows[1]["id"]);
			Assert.Equal(3L, dt.Rows[2]["id"]);

			Assert.Equal(DBNull.Value, dt.Rows[0]["int_value"]);
			Assert.Equal(0, dt.Rows[1]["int_value"]);
			Assert.Equal(1, dt.Rows[2]["int_value"]);

			Assert.Equal(DBNull.Value, dt.Rows[0]["text_value"]);
			Assert.Equal("", dt.Rows[1]["text_value"]);
			Assert.Equal("one", dt.Rows[2]["text_value"]);
		}

		[SkippableFact(Baseline = "Throws FormatException: Input string was not in a correct format")]
		public void InsertWithDataSet()
		{
			using (var ds = new DataSet())
			using (var da = new SingleStoreDataAdapter("SELECT * FROM data_adapter", m_connection))
			{
				da.Fill(ds);

				da.InsertCommand = new SingleStoreCommand("INSERT INTO data_adapter (int_value, text_value) VALUES (@int, @text)", m_connection);

				da.InsertCommand.Parameters.Add(new("@int", DbType.Int32));
				da.InsertCommand.Parameters.Add(new("@text", DbType.String));

				da.InsertCommand.Parameters[0].Direction = ParameterDirection.Input;
				da.InsertCommand.Parameters[1].Direction = ParameterDirection.Input;

				da.InsertCommand.Parameters[0].SourceColumn = "int_value";
				da.InsertCommand.Parameters[1].SourceColumn = "text_value";

				var dt = ds.Tables[0];
				var dr = dt.NewRow();
				dr["int_value"] = 4;
				dr["text_value"] = "four";
				dt.Rows.Add(dr);

				using var ds2 = ds.GetChanges();
				da.Update(ds2);

				ds.Merge(ds2);
				ds.AcceptChanges();
			}

			using var cmd2 = new SingleStoreCommand("SELECT id, int_value, text_value FROM data_adapter", m_connection);
			using var dr2 = cmd2.ExecuteReader();
			Assert.True(dr2.Read());
			Assert.Equal(1L, dr2[0]);

			Assert.True(dr2.Read());
			Assert.Equal(2L, dr2[0]);

			Assert.True(dr2.Read());
			Assert.Equal(3L, dr2[0]);

			Assert.True(dr2.Read());
			Assert.Equal(4L, dr2[0]);
			Assert.Equal(4, dr2[1]);
			Assert.Equal("four", dr2[2]);
		}

		[Fact]
		public void BatchUpdate()
		{
			using (var ds = new DataSet())
			using (var da = new SingleStoreDataAdapter("SELECT * FROM data_adapter", m_connection))
			{
				da.Fill(ds);

				da.UpdateCommand = new SingleStoreCommand("UPDATE data_adapter SET int_value=@int, text_value=@text WHERE id=@id", m_connection)
				{
					Parameters =
					{
						new("@int", SingleStoreDbType.Int32) { Direction = ParameterDirection.Input, SourceColumn = "int_value" },
						new("@text", SingleStoreDbType.String) { Direction = ParameterDirection.Input, SourceColumn = "text_value" },
						new("@id", SingleStoreDbType.Int64) { Direction = ParameterDirection.Input, SourceColumn = "id" },
					},
					UpdatedRowSource = UpdateRowSource.None,
				};

				da.UpdateBatchSize = 10;

				var dt = ds.Tables[0];
				dt.Rows[0][1] = 2;
				dt.Rows[0][2] = "two";
				dt.Rows[1][1] = 3;
				dt.Rows[1][2] = "three";
				dt.Rows[2][1] = 4;
				dt.Rows[2][2] = "four";

				da.Update(ds);
			}

			Assert.Equal(new[] { "two", "three", "four" }, m_connection.Query<string>("SELECT text_value FROM data_adapter ORDER BY id"));
		}


		[Fact]
		public void BatchInsert()
		{
			using (var ds = new DataSet())
			using (var da = new SingleStoreDataAdapter("SELECT * FROM data_adapter", m_connection))
			{
				da.Fill(ds);

				da.InsertCommand = new SingleStoreCommand("INSERT INTO data_adapter(int_value, text_value) VALUES(@int, @text);", m_connection)
				{
					Parameters =
					{
						new("@int", SingleStoreDbType.Int32) { Direction = ParameterDirection.Input, SourceColumn = "int_value" },
						new("@text", SingleStoreDbType.String) { Direction = ParameterDirection.Input, SourceColumn = "text_value" },
					},
					UpdatedRowSource = UpdateRowSource.None,
				};

				da.UpdateBatchSize = 10;

				var dt = ds.Tables[0];
				dt.Rows.Add(0, 2, "two");
				dt.Rows.Add(0, 3, "three");
				dt.Rows.Add(0, 4, "four");

				da.Update(ds);
			}

			Assert.Equal(new[] { null, "", "one", "two", "three", "four" }, m_connection.Query<string>("SELECT text_value FROM data_adapter ORDER BY id"));
		}
		readonly SingleStoreConnection m_connection;
	}
