namespace SingleStoreConnector;

/// <summary>
/// Provides context for the <see cref="SingleStoreConnection.ProvidePasswordCallback"/> delegate.
/// </summary>
public sealed class SingleStoreProvidePasswordContext
{
	/// <summary>
	/// The server to which SingleStoreConnector is connecting. This is a host name from the <see cref="SingleStoreConnectionStringBuilder.Server"/> option.
	/// </summary>
	public string Server { get; }

	/// <summary>
	/// The server port. This corresponds to <see cref="SingleStoreConnectionStringBuilder.Port"/>.
	/// </summary>
	public int Port { get; }

	/// <summary>
	/// The user ID being used for authentication. This corresponds to <see cref="SingleStoreConnectionStringBuilder.UserID"/>.
	/// </summary>
	public string UserId { get; }

	/// <summary>
	/// The optional initial database; this value may be the empty string. This corresponds to <see cref="SingleStoreConnectionStringBuilder.Database"/>.
	/// </summary>
	public string Database { get; }

	internal SingleStoreProvidePasswordContext(string server, int port, string userId, string database)
	{
		Server = server;
		Port = port;
		UserId = userId;
		Database = database;
	}
}
