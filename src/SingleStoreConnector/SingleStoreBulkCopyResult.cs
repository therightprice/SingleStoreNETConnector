namespace SingleStoreConnector;

/// <summary>
/// Represents the result of a <see cref="SingleStoreBulkCopy"/> operation.
/// </summary>
public sealed class SingleStoreBulkCopyResult
{
	/// <summary>
	/// The warnings, if any. Users of <see cref="SingleStoreBulkCopy"/> should check that this collection is empty to avoid
	/// potential data loss from failed data type conversions.
	/// </summary>
	public IReadOnlyList<SingleStoreError> Warnings { get; }

	/// <summary>
	/// The number of rows that were inserted during the bulk copy operation.
	/// </summary>
	public int RowsInserted { get; }

	internal SingleStoreBulkCopyResult(IReadOnlyList<SingleStoreError> warnings, int rowsInserted)
	{
		Warnings = warnings;
		RowsInserted = rowsInserted;
	}
}
