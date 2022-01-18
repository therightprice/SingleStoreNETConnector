namespace SideBySide;

public class ParameterTests
{
	[Theory]
	[InlineData(DbType.Byte, MySqlDbType.UByte)]
	[InlineData(DbType.SByte, MySqlDbType.Byte)]
	[InlineData(DbType.Int16, MySqlDbType.Int16)]
	[InlineData(DbType.UInt16, MySqlDbType.UInt16)]
	[InlineData(DbType.Int64, MySqlDbType.Int64)]
	[InlineData(DbType.Single, MySqlDbType.Float)]
	[InlineData(DbType.Double, MySqlDbType.Double)]
	[InlineData(DbType.Guid, MySqlDbType.Guid)]
	public void DbTypeToMySqlDbType(DbType dbType, MySqlDbType mySqlDbType)
	{
		var parameter = new SingleStoreParameter { DbType = dbType };
		Assert.Equal(dbType, parameter.DbType);
		Assert.Equal(mySqlDbType, parameter.MySqlDbType);

		parameter = new SingleStoreParameter { MySqlDbType = mySqlDbType };
		Assert.Equal(mySqlDbType, parameter.MySqlDbType);
		Assert.Equal(dbType, parameter.DbType);
	}

	[Theory]
	[InlineData(new[] { DbType.StringFixedLength, DbType.AnsiStringFixedLength }, new[] { MySqlDbType.String })]
	[InlineData(new[] { DbType.Int32 }, new[] { MySqlDbType.Int32, MySqlDbType.Int24 })]
	[InlineData(new[] { DbType.UInt32 }, new[] { MySqlDbType.UInt32, MySqlDbType.UInt24 })]
	[InlineData(new[] { DbType.UInt64 }, new[] { MySqlDbType.UInt64, MySqlDbType.Bit })]
	[InlineData(new[] { DbType.DateTime }, new[] { MySqlDbType.DateTime, MySqlDbType.Timestamp })]
	[InlineData(new[] { DbType.Date }, new[] { MySqlDbType.Date, MySqlDbType.Newdate })]
#if !BASELINE
	[InlineData(new[] { DbType.Int32 }, new[] { MySqlDbType.Int32, MySqlDbType.Year })]
	[InlineData(new[] { DbType.Binary }, new[] { MySqlDbType.Blob, MySqlDbType.Binary, MySqlDbType.TinyBlob, MySqlDbType.MediumBlob, MySqlDbType.LongBlob, MySqlDbType.Geometry })]
	[InlineData(new[] { DbType.String, DbType.AnsiString, DbType.Xml },
		new[] { MySqlDbType.VarChar, MySqlDbType.VarString, MySqlDbType.Text, MySqlDbType.TinyText, MySqlDbType.MediumText, MySqlDbType.LongText, MySqlDbType.JSON, MySqlDbType.Enum, MySqlDbType.Set })]
	[InlineData(new[] { DbType.Decimal, DbType.Currency }, new[] { MySqlDbType.NewDecimal, MySqlDbType.Decimal })]
#else
	[InlineData(new[] { DbType.Decimal, DbType.Currency }, new[] { MySqlDbType.Decimal, MySqlDbType.NewDecimal })]
#endif
	public void DbTypesToMySqlDbTypes(DbType[] dbTypes, MySqlDbType[] mySqlDbTypes)
	{
		foreach (var dbType in dbTypes)
		{
			var parameter = new SingleStoreParameter { DbType = dbType };
			Assert.Equal(dbType, parameter.DbType);
			Assert.Equal(mySqlDbTypes[0], parameter.MySqlDbType);
		}

		foreach (var mySqlDbType in mySqlDbTypes)
		{
			var parameter = new SingleStoreParameter { MySqlDbType = mySqlDbType };
			Assert.Equal(mySqlDbType, parameter.MySqlDbType);
			Assert.Equal(dbTypes[0], parameter.DbType);
		}
	}

	[Fact]
	public void ConstructorSimple()
	{
		var parameter = new SingleStoreParameter();
#if BASELINE
		Assert.Null(parameter.ParameterName);
		Assert.Equal(MySqlDbType.Decimal, parameter.MySqlDbType);
		Assert.Equal(DbType.AnsiString, parameter.DbType);
		Assert.Null(parameter.SourceColumn);
#else
		Assert.Equal("", parameter.ParameterName);
		Assert.Equal(MySqlDbType.VarChar, parameter.MySqlDbType);
		Assert.Equal(DbType.String, parameter.DbType);
		Assert.Equal("", parameter.SourceColumn);
#endif
		Assert.False(parameter.IsNullable);
		Assert.Null(parameter.Value);
		Assert.Equal(ParameterDirection.Input, parameter.Direction);
		Assert.Equal(0, parameter.Precision);
		Assert.Equal(0, parameter.Scale);
		Assert.Equal(0, parameter.Size);
#if BASELINE
		Assert.Equal(DataRowVersion.Default, parameter.SourceVersion);
#else
		Assert.Equal(DataRowVersion.Current, parameter.SourceVersion);
#endif
	}

	[Fact]
	public void ConstructorNameValue()
	{
		var parameter = new SingleStoreParameter("@name", 1.0);
		Assert.Equal("@name", parameter.ParameterName);
		Assert.Equal(MySqlDbType.Double, parameter.MySqlDbType);
		Assert.Equal(DbType.Double, parameter.DbType);
		Assert.False(parameter.IsNullable);
		Assert.Equal(1.0, parameter.Value);
		Assert.Equal(ParameterDirection.Input, parameter.Direction);
		Assert.Equal(0, parameter.Precision);
		Assert.Equal(0, parameter.Scale);
		Assert.Equal(0, parameter.Size);
#if BASELINE
		Assert.Equal(DataRowVersion.Default, parameter.SourceVersion);
#else
		Assert.Equal(DataRowVersion.Current, parameter.SourceVersion);
#endif
#if BASELINE
		Assert.Null(parameter.SourceColumn);
#else
		Assert.Equal("", parameter.SourceColumn);
#endif
	}

	[Fact]
	public void ConstructorNameType()
	{
		var parameter = new SingleStoreParameter("@name", MySqlDbType.Double);
		Assert.Equal("@name", parameter.ParameterName);
		Assert.Equal(MySqlDbType.Double, parameter.MySqlDbType);
		Assert.Equal(DbType.Double, parameter.DbType);
		Assert.False(parameter.IsNullable);
#if BASELINE // https://bugs.mysql.com/bug.php?id=101253
		Assert.Equal(0, parameter.Value);
#else
		Assert.Null(parameter.Value);
#endif
		Assert.Equal(ParameterDirection.Input, parameter.Direction);
		Assert.Equal(0, parameter.Precision);
		Assert.Equal(0, parameter.Scale);
		Assert.Equal(0, parameter.Size);
#if BASELINE
		Assert.Equal(DataRowVersion.Default, parameter.SourceVersion);
#else
		Assert.Equal(DataRowVersion.Current, parameter.SourceVersion);
#endif
#if BASELINE
		Assert.Null(parameter.SourceColumn);
#else
		Assert.Equal("", parameter.SourceColumn);
#endif
	}

	[Fact]
	public void ConstructorNameTypeSize()
	{
		var parameter = new SingleStoreParameter("@name", MySqlDbType.Double, 4);
		Assert.Equal("@name", parameter.ParameterName);
		Assert.Equal(MySqlDbType.Double, parameter.MySqlDbType);
		Assert.Equal(DbType.Double, parameter.DbType);
		Assert.False(parameter.IsNullable);
#if BASELINE // https://bugs.mysql.com/bug.php?id=101253
		Assert.Equal(0, parameter.Value);
#else
		Assert.Null(parameter.Value);
#endif
		Assert.Equal(ParameterDirection.Input, parameter.Direction);
		Assert.Equal(0, parameter.Precision);
		Assert.Equal(0, parameter.Scale);
		Assert.Equal(4, parameter.Size);
#if BASELINE
		Assert.Equal(DataRowVersion.Default, parameter.SourceVersion);
#else
		Assert.Equal(DataRowVersion.Current, parameter.SourceVersion);
#endif
#if BASELINE
		Assert.Null(parameter.SourceColumn);
#else
		Assert.Equal("", parameter.SourceColumn);
#endif
	}

	[Fact]
	public void ConstructorNameTypeSizeSourceColumn()
	{
		var parameter = new SingleStoreParameter("@name", MySqlDbType.Int32, 4, "source");
		Assert.Equal("@name", parameter.ParameterName);
		Assert.Equal(MySqlDbType.Int32, parameter.MySqlDbType);
		Assert.Equal(DbType.Int32, parameter.DbType);
		Assert.False(parameter.IsNullable);
#if BASELINE // https://bugs.mysql.com/bug.php?id=101253
		Assert.Equal(0, parameter.Value);
#else
		Assert.Null(parameter.Value);
#endif
		Assert.Equal(ParameterDirection.Input, parameter.Direction);
		Assert.Equal(0, parameter.Precision);
		Assert.Equal(0, parameter.Scale);
		Assert.Equal(4, parameter.Size);
#if BASELINE
		Assert.Equal(DataRowVersion.Default, parameter.SourceVersion);
#else
		Assert.Equal(DataRowVersion.Current, parameter.SourceVersion);
#endif
		Assert.Equal("source", parameter.SourceColumn);
	}

	[Fact]
	public void ConstructorEverything()
	{
#if !NET452
		var parameter = new SingleStoreParameter("@name", MySqlDbType.Float, 4, ParameterDirection.Output, true, 1, 2, "source", DataRowVersion.Original, 3.0);
		Assert.Equal(1, parameter.Precision);
		Assert.Equal(2, parameter.Scale);
#else
		// The .NET 4.5.2 tests use the .NET 4.5 library, which does not support Precision and Scale (they were added in .NET 4.5.1)
		var parameter = new SingleStoreParameter("@name", MySqlDbType.Float, 4, ParameterDirection.Output, true, 0, 0, "source", DataRowVersion.Original, 3.0);
#endif
		Assert.Equal("@name", parameter.ParameterName);
		Assert.Equal(MySqlDbType.Float, parameter.MySqlDbType);
		Assert.Equal(DbType.Single, parameter.DbType);
		Assert.Equal(3.0, parameter.Value);
		Assert.True(parameter.IsNullable);
		Assert.Equal(ParameterDirection.Output, parameter.Direction);
		Assert.Equal(4, parameter.Size);
		Assert.Equal(DataRowVersion.Original, parameter.SourceVersion);
		Assert.Equal("source", parameter.SourceColumn);
	}

	[Fact]
	public void CloneParameterName()
	{
		var parameter = new SingleStoreParameter { ParameterName = "test" };
		var clone = parameter.Clone();
		Assert.Equal(parameter.ParameterName, clone.ParameterName);
	}

	[Fact]
	public void CloneDbType()
	{
		var parameter = new SingleStoreParameter { DbType = DbType.Int64 };
		var clone = parameter.Clone();
		Assert.Equal(parameter.DbType, clone.DbType);
	}

	[Fact]
	public void CloneMySqlDbType()
	{
		var parameter = new SingleStoreParameter { MySqlDbType = MySqlDbType.MediumText };
		var clone = parameter.Clone();
		Assert.Equal(parameter.MySqlDbType, clone.MySqlDbType);
	}

	[Fact]
	public void CloneDirection()
	{
		var parameter = new SingleStoreParameter { Direction = ParameterDirection.InputOutput };
		var clone = parameter.Clone();
		Assert.Equal(parameter.Direction, clone.Direction);
	}

	[SkippableFact(Baseline = "https://bugs.mysql.com/bug.php?id=92734")]
	public void CloneIsNullable()
	{
		var parameter = new SingleStoreParameter { IsNullable = true };
		var clone = parameter.Clone();
		Assert.Equal(parameter.IsNullable, clone.IsNullable);
	}

	[SkippableFact(Baseline = "https://bugs.mysql.com/bug.php?id=92734")]
	public void ClonePrecision()
	{
		var parameter = new SingleStoreParameter { Precision = 10 };
		var clone = parameter.Clone();
		Assert.Equal(parameter.Precision, clone.Precision);
	}

	[SkippableFact(Baseline = "https://bugs.mysql.com/bug.php?id=92734")]
	public void CloneScale()
	{
		var parameter = new SingleStoreParameter { Scale = 12 };
		var clone = parameter.Clone();
		Assert.Equal(parameter.Scale, clone.Scale);
	}

	[SkippableFact(Baseline = "https://bugs.mysql.com/bug.php?id=92734")]
	public void CloneSize()
	{
		var parameter = new SingleStoreParameter { Size = 8 };
		var clone = parameter.Clone();
		Assert.Equal(parameter.Size, clone.Size);
	}

	[Fact]
	public void CloneSourceColumn()
	{
		var parameter = new SingleStoreParameter { SourceColumn = "test" };
		var clone = parameter.Clone();
		Assert.Equal(parameter.SourceColumn, clone.SourceColumn);
	}

	[SkippableFact(Baseline = "https://bugs.mysql.com/bug.php?id=92734")]
	public void CloneSourceColumnNullMapping()
	{
		var parameter = new SingleStoreParameter { SourceColumnNullMapping = true };
		var clone = parameter.Clone();
		Assert.Equal(parameter.SourceColumnNullMapping, clone.SourceColumnNullMapping);
	}

	[Fact]
	public void CloneSourceVersion()
	{
		var parameter = new SingleStoreParameter { SourceVersion = DataRowVersion.Proposed };
		var clone = parameter.Clone();
		Assert.Equal(parameter.SourceVersion, clone.SourceVersion);
	}

	[Fact]
	public void CloneValue()
	{
		var parameter = new SingleStoreParameter { Value = "test" };
		var clone = parameter.Clone();
		Assert.Equal(parameter.Value, clone.Value);
	}

	[Theory]
	[InlineData(1, DbType.Int32, MySqlDbType.Int32)]
	[InlineData(1.0, DbType.Double, MySqlDbType.Double)]
	[InlineData(1.0f, DbType.Single, MySqlDbType.Float)]
	[InlineData("1", DbType.String, MySqlDbType.VarChar)]
#if BASELINE
	[InlineData('1', DbType.Object, MySqlDbType.Blob)]
#else
	[InlineData('1', DbType.String, MySqlDbType.VarChar)]
#endif
	public void SetValueInfersType(object value, DbType expectedDbType, MySqlDbType expectedMySqlDbType)
	{
		var parameter = new SingleStoreParameter { Value = value };
		Assert.Equal(expectedDbType, parameter.DbType);
		Assert.Equal(expectedMySqlDbType, parameter.MySqlDbType);
	}

	[Fact]
	public void SetValueToByteArrayInfersType()
	{
		var parameter = new SingleStoreParameter { Value = new byte[1] };
#if BASELINE
		Assert.Equal(DbType.Object, parameter.DbType);
#else
		Assert.Equal(DbType.Binary, parameter.DbType);
#endif
		Assert.Equal(MySqlDbType.Blob, parameter.MySqlDbType);
	}


	[Fact]
	public void SetValueDoesNotInferType()
	{
		var parameter = new SingleStoreParameter("@name", MySqlDbType.Int32);
		Assert.Equal(DbType.Int32, parameter.DbType);
		Assert.Equal(MySqlDbType.Int32, parameter.MySqlDbType);

		parameter.Value = 1.0;
		Assert.Equal(DbType.Int32, parameter.DbType);
		Assert.Equal(MySqlDbType.Int32, parameter.MySqlDbType);
	}

	[Fact]
	public void ResetDbType()
	{
		var parameter = new SingleStoreParameter("@name", 1);
		Assert.Equal(DbType.Int32, parameter.DbType);
		Assert.Equal(MySqlDbType.Int32, parameter.MySqlDbType);

		parameter.ResetDbType();
#if BASELINE
		Assert.Equal(MySqlDbType.Int32, parameter.MySqlDbType);
		Assert.Equal(DbType.Int32, parameter.DbType);
#else
		Assert.Equal(MySqlDbType.VarChar, parameter.MySqlDbType);
		Assert.Equal(DbType.String, parameter.DbType);
#endif

		parameter.Value = 1.0;
		Assert.Equal(DbType.Double, parameter.DbType);
		Assert.Equal(MySqlDbType.Double, parameter.MySqlDbType);
	}

	[Fact]
	public void PrecisionViaInterface()
	{
		IDbCommand command = new SingleStoreCommand();
		IDbDataParameter parameter = command.CreateParameter();
		parameter.Precision = 11;
		Assert.Equal((byte) 11, parameter.Precision);
	}

	[Fact]
	public void PrecisionViaBaseClass()
	{
		DbCommand command = new SingleStoreCommand();
		DbParameter parameter = command.CreateParameter();
		parameter.Precision = 11;
		Assert.Equal((byte) 11, parameter.Precision);
	}

	[Fact]
	public void PrecisionDirect()
	{
		SingleStoreCommand command = new SingleStoreCommand();
		SingleStoreParameter parameter = command.CreateParameter();
		parameter.Precision = 11;
		Assert.Equal((byte) 11, parameter.Precision);
	}

	[Fact]
	public void PrecisionMixed()
	{
		SingleStoreCommand command = new SingleStoreCommand();
		DbParameter parameter = command.CreateParameter();
		((IDbDataParameter) parameter).Precision = 11;
		Assert.Equal((byte) 11, ((SingleStoreParameter) parameter).Precision);
	}

	[Fact]
	public void ScaleViaInterface()
	{
		IDbCommand command = new SingleStoreCommand();
		IDbDataParameter parameter = command.CreateParameter();
		parameter.Scale = 12;
		Assert.Equal((byte) 12, parameter.Scale);
	}

	[Fact]
	public void ScaleViaBaseClass()
	{
		DbCommand command = new SingleStoreCommand();
		DbParameter parameter = command.CreateParameter();
		parameter.Scale = 12;
		Assert.Equal((byte) 12, parameter.Scale);
	}

	[Fact]
	public void ScaleDirect()
	{
		SingleStoreCommand command = new SingleStoreCommand();
		SingleStoreParameter parameter = command.CreateParameter();
		parameter.Scale = 12;
		Assert.Equal((byte) 12, parameter.Scale);
	}

	[Fact]
	public void ScaleMixed()
	{
		SingleStoreCommand command = new SingleStoreCommand();
		DbParameter parameter = command.CreateParameter();
		((IDbDataParameter) parameter).Scale = 12;
		Assert.Equal((byte) 12, ((SingleStoreParameter) parameter).Scale);
	}

	[Fact]
	public void ZeroBytes()
	{
		var csb = new SingleStoreConnectionStringBuilder(AppConfig.ConnectionString);
		using var connection = new SingleStoreConnection(csb.ConnectionString);
		connection.Open();

		connection.Execute(@"
DROP TABLE IF EXISTS zeroByteEscaping;
DROP TABLE IF EXISTS zeroByteEscapingCTAS;
CREATE TABLE zeroByteEscaping (
  `Id` INT NOT NULL,
  `Content` VARBINARY(5),
  PRIMARY KEY (`Id`)
);
INSERT INTO zeroByteEscaping VALUES(1, BINARY('\012\0\0'));
");
             
                using (var command = new SingleStoreCommand(@"CREATE TABLE zeroByteEscapingCTAS as SELECT * FROM zeroByteEscaping WHERE Content=@content", connection))
                {
                        command.Parameters.AddWithValue("@content", new byte[] {0x00, 0x31, 0x32, 0x00, 0x00});
                        Assert.False(command.IsPrepared);
                        command.ExecuteNonQuery();
                }


		using (var command = new SingleStoreCommand(@"SELECT COUNT(*) FROM `zeroByteEscapingCTAS` WHERE BINARY(`Content`) = 0x0031320000", connection))
		{
			var result = command.ExecuteScalar();
			Assert.Equal(1, (long) result!);
		}
	}
}
