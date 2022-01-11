namespace SingleStoreConnector.Logging;

/// <summary>
/// Creates loggers that do nothing.
/// </summary>
public sealed class NoOpLoggerProvider : ISingleStoreConnectorLoggerProvider
{
	/// <summary>
	/// Returns a <see cref="NoOpLogger"/>.
	/// </summary>
	public ISingleStoreConnectorLogger CreateLogger(string name) => NoOpLogger.Instance;
}
