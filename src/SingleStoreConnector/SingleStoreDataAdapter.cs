namespace SingleStoreConnector;

public sealed class SingleStoreDataAdapter : DbDataAdapter
{
	public SingleStoreDataAdapter()
	{
		GC.SuppressFinalize(this);
	}

	public SingleStoreDataAdapter(SingleStoreCommand selectCommand)
		: this()
	{
		SelectCommand = selectCommand;
	}

	public SingleStoreDataAdapter(string selectCommandText, SingleStoreConnection connection)
		: this(new SingleStoreCommand(selectCommandText, connection))
	{
	}

	public SingleStoreDataAdapter(string selectCommandText, string connectionString)
		: this(new SingleStoreCommand(selectCommandText, new SingleStoreConnection(connectionString)))
	{
	}

	public event SingleStoreRowUpdatingEventHandler? RowUpdating;

	public event SingleStoreRowUpdatedEventHandler? RowUpdated;

	public new SingleStoreCommand? DeleteCommand
	{
		get => (SingleStoreCommand?) base.DeleteCommand;
		set => base.DeleteCommand = value;
	}

	public new SingleStoreCommand? InsertCommand
	{
		get => (SingleStoreCommand?) base.InsertCommand;
		set => base.InsertCommand = value;
	}

	public new SingleStoreCommand? SelectCommand
	{
		get => (SingleStoreCommand?) base.SelectCommand;
		set => base.SelectCommand = value;
	}

	public new SingleStoreCommand? UpdateCommand
	{
		get => (SingleStoreCommand?) base.UpdateCommand;
		set => base.UpdateCommand = value;
	}

	protected override void OnRowUpdating(RowUpdatingEventArgs value) => RowUpdating?.Invoke(this, (SingleStoreRowUpdatingEventArgs) value);

	protected override void OnRowUpdated(RowUpdatedEventArgs value) => RowUpdated?.Invoke(this, (SingleStoreRowUpdatedEventArgs) value);

	protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand? command, StatementType statementType, DataTableMapping tableMapping) => new SingleStoreRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);

	protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand? command, StatementType statementType, DataTableMapping tableMapping) => new SingleStoreRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);

	public override int UpdateBatchSize { get; set; }

	protected override void InitializeBatching() => m_batch = new();

	protected override void TerminateBatching()
	{
		m_batch?.Dispose();
		m_batch = null;
	}

	protected override int AddToBatch(IDbCommand command)
	{
		var mySqlCommand = (SingleStoreCommand) command;
		if (m_batch!.Connection is null)
		{
			m_batch.Connection = mySqlCommand.Connection;
			m_batch.Transaction = mySqlCommand.Transaction;
		}

		var count = m_batch.BatchCommands.Count;
		var batchCommand = new SingleStoreBatchCommand
		{
			CommandText = command.CommandText,
			CommandType = command.CommandType,
		};
		if (mySqlCommand.CloneRawParameters() is SingleStoreParameterCollection clonedParameters)
		{
			foreach (var clonedParameter in clonedParameters)
				batchCommand.Parameters.Add(clonedParameter!);
		}

		m_batch.BatchCommands.Add(batchCommand);
		return count;
	}

	protected override void ClearBatch() => m_batch!.BatchCommands.Clear();

	protected override int ExecuteBatch() => m_batch!.ExecuteNonQuery();

	SingleStoreBatch? m_batch;
}

public delegate void SingleStoreRowUpdatingEventHandler(object sender, SingleStoreRowUpdatingEventArgs e);

public delegate void SingleStoreRowUpdatedEventHandler(object sender, SingleStoreRowUpdatedEventArgs e);

public sealed class SingleStoreRowUpdatingEventArgs : RowUpdatingEventArgs
{
	public SingleStoreRowUpdatingEventArgs(DataRow row, IDbCommand? command, StatementType statementType, DataTableMapping tableMapping)
		: base(row, command, statementType, tableMapping)
	{
	}

	public new SingleStoreCommand? Command => (SingleStoreCommand?) base.Command!;
}

public sealed class SingleStoreRowUpdatedEventArgs : RowUpdatedEventArgs
{
	public SingleStoreRowUpdatedEventArgs(DataRow row, IDbCommand? command, StatementType statementType, DataTableMapping tableMapping)
		: base(row, command, statementType, tableMapping)
	{
	}

	public new SingleStoreCommand? Command => (SingleStoreCommand?) base.Command;
}
