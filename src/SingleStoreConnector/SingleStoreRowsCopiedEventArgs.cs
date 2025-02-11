namespace SingleStoreConnector;

public sealed class SingleStoreRowsCopiedEventArgs : EventArgs
{
	/// <summary>
	/// Gets or sets a value that indicates whether the bulk copy operation should be aborted.
	/// </summary>
	public bool Abort { get; set; }

	/// <summary>
	/// Gets a value that returns the number of rows copied during the current bulk copy operation.
	/// </summary>
	public long RowsCopied { get; internal set; }

	internal SingleStoreRowsCopiedEventArgs()
	{
	}
}

/// <summary>
/// Represents the method that handles the <see cref="SingleStoreBulkCopy.MySqlRowsCopied"/> event of a <see cref="SingleStoreBulkCopy"/>.
/// </summary>
public delegate void SingleStoreRowsCopiedEventHandler(object sender, SingleStoreRowsCopiedEventArgs e);
