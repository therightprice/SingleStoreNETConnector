using SingleStoreConnector.Core;

namespace SingleStoreConnector;

public sealed class SingleStoreBatchCommand :
#if NET6_0_OR_GREATER
	DbBatchCommand,
#endif
	IMySqlCommand
{
	public SingleStoreBatchCommand()
		: this(null)
	{
	}

	public SingleStoreBatchCommand(string? commandText)
	{
		CommandText = commandText ?? "";
		CommandType = CommandType.Text;
	}

#if NET6_0_OR_GREATER
	public override string CommandText { get; set; }
#else
	public string CommandText { get; set; }
#endif
#if NET6_0_OR_GREATER
	public override CommandType CommandType { get; set; }
#else
	public CommandType CommandType { get; set; }
#endif
#if NET6_0_OR_GREATER
	public override int RecordsAffected =>
#else
	public int RecordsAffected =>
#endif
		0;

#if NET6_0_OR_GREATER
	public new SingleStoreParameterCollection Parameters =>
#else
	public SingleStoreParameterCollection Parameters =>
#endif
		m_parameterCollection ??= new();

#if NET6_0_OR_GREATER
	protected override DbParameterCollection DbParameterCollection => Parameters;
#endif

	bool IMySqlCommand.AllowUserVariables => false;

	CommandBehavior IMySqlCommand.CommandBehavior => Batch!.CurrentCommandBehavior;

	SingleStoreParameterCollection? IMySqlCommand.RawParameters => m_parameterCollection;

	SingleStoreAttributeCollection? IMySqlCommand.RawAttributes => null;

	SingleStoreConnection? IMySqlCommand.Connection => Batch?.Connection;

	long IMySqlCommand.LastInsertedId => m_lastInsertedId;

	PreparedStatements? IMySqlCommand.TryGetPreparedStatements() => null;

	void IMySqlCommand.SetLastInsertedId(long lastInsertedId) => m_lastInsertedId = lastInsertedId;

	SingleStoreParameterCollection? IMySqlCommand.OutParameters { get; set; }

	SingleStoreParameter? IMySqlCommand.ReturnParameter { get; set; }

	ICancellableCommand IMySqlCommand.CancellableCommand => Batch!;

	internal SingleStoreBatch? Batch { get; set; }

	SingleStoreParameterCollection? m_parameterCollection;
	long m_lastInsertedId;
}
