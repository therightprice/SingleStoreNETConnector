## About

SingleStoreConnector is an [ADO.NET](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/) data provider for [SingleStore](https://www.singlestore.com/).

## How to Use

```csharp
// set these values correctly for your database server
var builder = new SingleStoreConnectionStringBuilder
{
	Server = "your-server",
	UserID = "database-user",
	Password = "P@ssw0rd!",
	Database = "database-name",
};

// open a connection asynchronously
using var connection = new SingleStoreConnection(builder.ConnectionString);
await connection.OpenAsync();

// create a DB command and set the SQL statement with parameters
using var command = connection.CreateCommand();
command.CommandText = @"SELECT * FROM orders WHERE order_id = @OrderId;";
command.Parameters.AddWithValue("@OrderId", orderId);

// execute the command and read the results
using var reader = await command.ExecuteReaderAsync();
while (reader.Read())
{
	var id = reader.GetInt32("order_id");
	var date = reader.GetDateTime("order_date");
	// ...
}
```

## Key Features

* Full support for async I/O
* High performance
* Supports .NET Framework, .NET Core, and .NET 5.0+

## Main Types

The main types provided by this library are:

* `SingleStoreConnection` (implementation of `DbConnection`)
* `SingleStoreCommand` (implementation of `DbCommand`)
* `SingleStoreDataReader` (implementation of `DbDataReader`)
* `SingleStoreBulkCopy`
* `SingleStoreBulkLoader`
* `SingleStoreConnectionStringBuilder`
* `SingleStoreConnectorFactory`
* `SingleStoreDataAdapter`
* `SingleStoreException`
* `SingleStoreTransaction` (implementation of `DbTransaction`)

## Feedback

SingleStoreConnector is released as open source under the [MIT license](https://github.com/memsql/SingleStoreNETConnector/blob/master/LICENSE). Bug reports and contributions are welcome at [the GitHub repository](https://github.com/memsql/SingleStoreNETConnector/).
