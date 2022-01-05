# Async SingleStore Connector for .NET and .NET Core

This is an [ADO.NET](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/) data
provider for [SingleStore](https://www.singlestore.com/). It provides implementations of
`DbConnection`, `DbCommand`, `DbDataReader`, `DbTransaction`—the classes
needed to query and update databases from managed code.

### License

This library is [MIT-licensed](LICENSE) and may be freely distributed with commercial software.
Commercial software that uses Connector/NET may have to purchase a [commercial license](https://www.mysql.com/about/legal/licensing/oem/)
from Oracle.

## Building

Install the latest [.NET Core](https://www.microsoft.com/net/core).

To build and run the tests, clone the repo and execute:

```
dotnet restore
dotnet test tests\MySqlConnector.Tests
```

To run the side-by-side tests, see [the instructions](tests/README.md).

## Goals

The goals of this project are:

1. **.NET Standard support:** It must run on the full .NET Framework and all platforms supported by .NET Core.
2. **Async:** All operations must be truly asynchronous whenever possible.
3. **High performance:** Avoid unnecessary allocations and copies when reading data.
4. **Lightweight:** Only the core of ADO.NET is implemented, not EF or Designer types.
5. **Managed:** Managed code only, no native code.
6. **Independent:** This is a clean-room reimplementation of the [MySQL Protocol](https://dev.mysql.com/doc/internals/en/client-server-protocol.html), not based on Connector/NET.

Cloning the full API of Connector/NET is not a goal of this project, although
it will try not to be gratuitously incompatible. For typical scenarios, [migrating to this package](https://mysqlconnector.net/tutorials/migrating-from-connector-net/) should
be easy.

## License

This library is licensed under the [MIT License](LICENSE).
