namespace SingleStoreConnector.Logging;

/// <summary>
/// <see cref="NoOpLogger"/> is an implementation of <see cref="ISingleStoreConnectorLogger"/> that does nothing.
/// </summary>
/// <remarks>This is the default logging implementation unless <see cref="SingleStoreConnectorLogManager.Provider"/> is set.</remarks>
public sealed class NoOpLogger : ISingleStoreConnectorLogger
{
	/// <summary>
	/// Returns <c>false</c>.
	/// </summary>
	public bool IsEnabled(SingleStoreConnectorLogLevel level) => false;

	/// <summary>
	/// Ignores the specified log message.
	/// </summary>
	public void Log(SingleStoreConnectorLogLevel level, string message, object?[]? args = null, Exception? exception = null)
	{
	}

	/// <summary>
	/// Returns a singleton instance of <see cref="NoOpLogger"/>.
	/// </summary>
	public static ISingleStoreConnectorLogger Instance { get; } = new NoOpLogger();
}
