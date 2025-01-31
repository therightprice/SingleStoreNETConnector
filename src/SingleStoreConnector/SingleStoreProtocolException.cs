using System.Runtime.Serialization;
using SingleStoreConnector.Utilities;

namespace SingleStoreConnector;

/// <summary>
/// <see cref="SingleStoreProtocolException"/> is thrown when there is an internal protocol error communicating with MySQL Server.
/// </summary>
[Serializable]
public sealed class SingleStoreProtocolException : InvalidOperationException
{
	/// <summary>
	/// Creates a new <see cref="SingleStoreProtocolException"/> for an out-of-order packet.
	/// </summary>
	/// <param name="expectedSequenceNumber">The expected packet sequence number.</param>
	/// <param name="packetSequenceNumber">The actual packet sequence number.</param>
	/// <returns>A new <see cref="SingleStoreProtocolException"/>.</returns>
	internal static SingleStoreProtocolException CreateForPacketOutOfOrder(int expectedSequenceNumber, int packetSequenceNumber) =>
		new SingleStoreProtocolException("Packet received out-of-order. Expected {0}; got {1}.".FormatInvariant(expectedSequenceNumber, packetSequenceNumber));

	private SingleStoreProtocolException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private SingleStoreProtocolException(string message)
		: base(message)
	{
	}
}
