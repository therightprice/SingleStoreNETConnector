using System.Globalization;
using System.Text.RegularExpressions;
using SingleStoreConnector.Utilities;

namespace SingleStoreConnector;

/// <summary>
/// <see cref="SingleStoreDecimal"/> represents a MySQL <c>DECIMAL</c> value that is too large to fit in a .NET <see cref="decimal"/>.
/// </summary>
public readonly struct SingleStoreDecimal
{
	/// <summary>
	/// Gets the value of this <see cref="SingleStoreDecimal"/> as a <see cref="decimal"/>.
	/// </summary>
	/// <remarks>This method will throw an <see cref="OverflowException"/> if the value is too large to be represented.</remarks>
	public decimal Value => decimal.Parse(m_value, CultureInfo.InvariantCulture);

	/// <summary>
	/// Gets the value of this <see cref="SingleStoreDecimal"/> as a <see cref="double"/>.
	/// </summary>
	/// <remarks>The return value may have lost precision.</remarks>
	public double ToDouble() => double.Parse(m_value, CultureInfo.InvariantCulture);

	/// <summary>
	/// Gets the original value of this <see cref="SingleStoreDecimal"/> as a <see cref="string"/>.
	/// </summary>
	public override string ToString() => m_value;

	internal SingleStoreDecimal(string value)
	{
		if (s_pattern.Match(value) is { Success: true } match)
		{
			var wholeLength = match.Groups[1].Length;
			var fractionLength = match.Groups[3].Value.TrimEnd('0').Length;

			var isWithinLengthLimits = wholeLength + fractionLength <= 65 && fractionLength <= 30;
			var isNegativeZero = value[0] == '-' && match.Groups[1].Value == "0" && fractionLength == 0;
			if (isWithinLengthLimits && !isNegativeZero)
			{
				m_value = value;
				return;
			}
		}

		throw new FormatException("Could not parse the value as a SingleStoreDecimal: {0}".FormatInvariant(value));
	}

	private static readonly Regex s_pattern = new(@"^-?([1-9][0-9]*|0)(\.([0-9]+))?$");

	private readonly string m_value;
}
