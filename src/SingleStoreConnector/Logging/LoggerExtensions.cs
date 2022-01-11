namespace SingleStoreConnector.Logging;

internal static class LoggerExtensions
{
	public static bool IsTraceEnabled(this ISingleStoreConnectorLogger log) => log.IsEnabled(SingleStoreConnectorLogLevel.Trace);
	public static bool IsDebugEnabled(this ISingleStoreConnectorLogger log) => log.IsEnabled(SingleStoreConnectorLogLevel.Debug);
	public static bool IsInfoEnabled(this ISingleStoreConnectorLogger log) => log.IsEnabled(SingleStoreConnectorLogLevel.Info);
	public static bool IsWarnEnabled(this ISingleStoreConnectorLogger log) => log.IsEnabled(SingleStoreConnectorLogLevel.Warn);
	public static bool IsErrorEnabled(this ISingleStoreConnectorLogger log) => log.IsEnabled(SingleStoreConnectorLogLevel.Error);
	public static bool IsFatalEnabled(this ISingleStoreConnectorLogger log) => log.IsEnabled(SingleStoreConnectorLogLevel.Fatal);

	public static void Trace(this ISingleStoreConnectorLogger log, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Trace, message, args, null);
	public static void Debug(this ISingleStoreConnectorLogger log, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Debug, message, args, null);
	public static void Info(this ISingleStoreConnectorLogger log, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Info, message, args, null);
	public static void Warn(this ISingleStoreConnectorLogger log, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Warn, message, args, null);
	public static void Error(this ISingleStoreConnectorLogger log, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Error, message, args, null);
	public static void Fatal(this ISingleStoreConnectorLogger log, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Fatal, message, args, null);

	public static void Trace(this ISingleStoreConnectorLogger log, Exception exception, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Trace, message, args, exception);
	public static void Debug(this ISingleStoreConnectorLogger log, Exception exception, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Debug, message, args, exception);
	public static void Info(this ISingleStoreConnectorLogger log, Exception exception, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Info, message, args, exception);
	public static void Warn(this ISingleStoreConnectorLogger log, Exception exception, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Warn, message, args, exception);
	public static void Error(this ISingleStoreConnectorLogger log, Exception exception, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Error, message, args, exception);
	public static void Fatal(this ISingleStoreConnectorLogger log, Exception exception, string message, params object?[] args) => log.Log(SingleStoreConnectorLogLevel.Fatal, message, args, exception);
}
