## About

This package integrates SingleStoreConnector logging with the Microsoft.Extensions.Logging framework.

## How to Use

Add the following line of code to `Program.cs` method (before any `SingleStoreConnector` objects have been used):

```csharp
app.Services.UseMySqlConnectorLogging();
```

Alternatively, obtain an `ILoggerFactory` through dependency injection and add:

```csharp
MySqlConnectorLogManager.Provider = new MicrosoftExtensionsLoggingLoggerProvider(loggerFactory);
```
