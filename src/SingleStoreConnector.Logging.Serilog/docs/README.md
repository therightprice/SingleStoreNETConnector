## About

This package integrates SingleStoreConnector logging with [Serilog](https://www.nuget.org/packages/Serilog/).

## How to Use

Add the following line of code to your application startup routine (before any `SingleStoreConnector` objects have been used):

```csharp
SingleStoreConnectorLogManager.Provider = new SerilogLoggerProvider();
```
