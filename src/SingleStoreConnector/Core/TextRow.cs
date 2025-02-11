using System.Buffers.Text;
using System.Text;
using SingleStoreConnector.Protocol;
using SingleStoreConnector.Protocol.Payloads;
using SingleStoreConnector.Protocol.Serialization;
using SingleStoreConnector.Utilities;

namespace SingleStoreConnector.Core;

internal sealed class TextRow : Row
{
	public TextRow(ResultSet resultSet)
		: base(resultSet)
	{
	}

	protected override Row CloneCore() => new TextRow(ResultSet);

	protected override void GetDataOffsets(ReadOnlySpan<byte> data, int[] dataOffsets, int[] dataLengths)
	{
		var reader = new ByteArrayReader(data);
		for (var column = 0; column < dataOffsets.Length; column++)
		{
			var length = reader.ReadLengthEncodedIntegerOrNull();
			dataLengths[column] = length == -1 ? 0 : length;
			dataOffsets[column] = length == -1 ? -1 : reader.Offset;
			reader.Offset += dataLengths[column];
		}
	}

	protected override int GetInt32Core(ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition) =>
		!Utf8Parser.TryParse(data, out int value, out var bytesConsumed) || bytesConsumed != data.Length ? throw new OverflowException() : value;

	protected override object GetValueCore(ReadOnlySpan<byte> data, ColumnDefinitionPayload columnDefinition)
	{
		var isUnsigned = (columnDefinition.ColumnFlags & ColumnFlags.Unsigned) != 0;
		switch (columnDefinition.ColumnType)
		{
		case ColumnType.Tiny:
			var value = ParseInt32(data);
			if (Connection.TreatTinyAsBoolean && columnDefinition.ColumnLength == 1 && !isUnsigned)
				return value != 0;
			return isUnsigned ? (object) (byte) value : (sbyte) value;

		case ColumnType.Int24:
		case ColumnType.Long:
			return isUnsigned ? (object) ParseUInt32(data) : ParseInt32(data);

		case ColumnType.Longlong:
			return isUnsigned ? (object) ParseUInt64(data) : ParseInt64(data);

		case ColumnType.Bit:
			return ReadBit(data, columnDefinition);

		case ColumnType.String:
			if (Connection.GuidFormat == SingleStoreGuidFormat.Char36 && columnDefinition.ColumnLength / ProtocolUtility.GetBytesPerCharacter(columnDefinition.CharacterSet) == 36)
				return Utf8Parser.TryParse(data, out Guid guid, out int guid36BytesConsumed, 'D') && guid36BytesConsumed == 36 ? guid : throw new FormatException();
			if (Connection.GuidFormat == SingleStoreGuidFormat.Char32 && columnDefinition.ColumnLength / ProtocolUtility.GetBytesPerCharacter(columnDefinition.CharacterSet) == 32)
				return Utf8Parser.TryParse(data, out Guid guid, out int guid32BytesConsumed, 'N') && guid32BytesConsumed == 32 ? guid : throw new FormatException();
			goto case ColumnType.VarString;

		case ColumnType.VarString:
		case ColumnType.VarChar:
		case ColumnType.TinyBlob:
		case ColumnType.Blob:
		case ColumnType.MediumBlob:
		case ColumnType.LongBlob:
		case ColumnType.Enum:
		case ColumnType.Set:
			if (columnDefinition.CharacterSet == CharacterSet.Binary)
			{
				var guidFormat = Connection.GuidFormat;
				if ((guidFormat is SingleStoreGuidFormat.Binary16 or SingleStoreGuidFormat.TimeSwapBinary16 or SingleStoreGuidFormat.LittleEndianBinary16) && columnDefinition.ColumnLength == 16)
					return CreateGuidFromBytes(guidFormat, data);

				return data.ToArray();
			}
			return Encoding.UTF8.GetString(data);

		case ColumnType.Json:
			return Encoding.UTF8.GetString(data);

		case ColumnType.Short:
			return isUnsigned ? (object) ParseUInt16(data) : ParseInt16(data);

		case ColumnType.Date:
		case ColumnType.DateTime:
		case ColumnType.NewDate:
		case ColumnType.Timestamp:
			return ParseDateTime(data);

		case ColumnType.Time:
			return Utility.ParseTimeSpan(data);

		case ColumnType.Year:
			return ParseInt32(data);

		case ColumnType.Float:
			return !Utf8Parser.TryParse(data, out float floatValue, out var floatBytesConsumed) || floatBytesConsumed != data.Length ? throw new FormatException() : floatValue;

		case ColumnType.Double:
			return !Utf8Parser.TryParse(data, out double doubleValue, out var doubleBytesConsumed) || doubleBytesConsumed != data.Length ? throw new FormatException() : doubleValue;

		case ColumnType.Decimal:
		case ColumnType.NewDecimal:
			return Utf8Parser.TryParse(data, out decimal decimalValue, out int bytesConsumed) && bytesConsumed == data.Length ? decimalValue : throw new FormatException();

		case ColumnType.Geometry:
			return data.ToArray();

		default:
			throw new NotImplementedException("Reading {0} not implemented".FormatInvariant(columnDefinition.ColumnType));
		}
	}

	private static short ParseInt16(ReadOnlySpan<byte> data) =>
		!Utf8Parser.TryParse(data, out short value, out var bytesConsumed) || bytesConsumed != data.Length ? throw new FormatException() : value;

	private static ushort ParseUInt16(ReadOnlySpan<byte> data) =>
		!Utf8Parser.TryParse(data, out ushort value, out var bytesConsumed) || bytesConsumed != data.Length ? throw new FormatException() : value;

	private static int ParseInt32(ReadOnlySpan<byte> data) =>
		!Utf8Parser.TryParse(data, out int value, out var bytesConsumed) || bytesConsumed != data.Length ? throw new FormatException() : value;

	private static uint ParseUInt32(ReadOnlySpan<byte> data) =>
		!Utf8Parser.TryParse(data, out uint value, out var bytesConsumed) || bytesConsumed != data.Length ? throw new FormatException() : value;

	private static long ParseInt64(ReadOnlySpan<byte> data) =>
		!Utf8Parser.TryParse(data, out long value, out var bytesConsumed) || bytesConsumed != data.Length ? throw new FormatException() : value;
}
