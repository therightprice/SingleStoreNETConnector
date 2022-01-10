namespace SingleStoreConnector;

/// <summary>
/// <see cref="MySqlInfoMessageEventArgs"/> contains the data supplied to the <see cref="MySqlInfoMessageEventHandler"/> event handler.
/// </summary>
public sealed class MySqlInfoMessageEventArgs : EventArgs
{
	internal MySqlInfoMessageEventArgs(IReadOnlyList<SingleStoreError> errors) => Errors = errors;

	/// <summary>
	/// The list of errors being reported.
	/// </summary>
	public IReadOnlyList<SingleStoreError> Errors { get; }
}

/// <summary>
/// Defines the event handler for <see cref="SingleStoreConnection.InfoMessage"/>.
/// </summary>
/// <param name="sender">The sender. This is the associated <see cref="SingleStoreConnection"/>.</param>
/// <param name="args">The <see cref="MySqlInfoMessageEventArgs"/> containing the errors.</param>
public delegate void MySqlInfoMessageEventHandler(object sender, MySqlInfoMessageEventArgs args);
