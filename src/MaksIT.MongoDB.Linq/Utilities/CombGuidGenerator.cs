using System.Buffers.Binary;

namespace MaksIT.MongoDB.Linq.Utilities {
  public static class CombGuidGenerator {
    private const int TimestampByteLength = 6; // Number of bytes to store the timestamp
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Generates a new COMB GUID by combining a randomly generated GUID with the current UTC timestamp.
    /// A COMB (COMBined) GUID includes both randomness and a timestamp for better sortability in databases.
    /// </summary>
    /// <returns>A new COMB GUID containing a random GUID and the current UTC timestamp.</returns>
    public static Guid CreateCombGuid() {
      return CreateCombGuidWithTimestamp(Guid.NewGuid(), DateTime.UtcNow);
    }

    /// <summary>
    /// Generates a new COMB GUID by combining a specified GUID with the current UTC timestamp.
    /// This allows the caller to use a specific GUID while embedding the current time for better sortability.
    /// </summary>
    /// <param name="baseGuid">The base GUID to combine with the current UTC timestamp.</param>
    /// <returns>A new COMB GUID combining the provided GUID with the current UTC timestamp.</returns>
    public static Guid CreateCombGuid(Guid baseGuid) {
      return CreateCombGuidWithTimestamp(baseGuid, DateTime.UtcNow);
    }

    /// <summary>
    /// Generates a new COMB GUID by combining a randomly generated GUID with a specified timestamp.
    /// Useful for creating GUIDs that incorporate a specific time, such as for historical data.
    /// </summary>
    /// <param name="timestamp">The timestamp to embed in the GUID.</param>
    /// <returns>A new COMB GUID combining a random GUID with the specified timestamp.</returns>
    public static Guid CreateCombGuid(DateTime timestamp) {
      return CreateCombGuidWithTimestamp(Guid.NewGuid(), timestamp);
    }

    /// <summary>
    /// Generates a new COMB GUID by combining a specified GUID and timestamp.
    /// This method provides full control over both the GUID and timestamp components.
    /// </summary>
    /// <param name="baseGuid">The base GUID to combine with the provided timestamp.</param>
    /// <param name="timestamp">The timestamp to embed in the GUID.</param>
    /// <returns>A new COMB GUID combining the provided GUID and timestamp.</returns>
    public static Guid CreateCombGuidWithTimestamp(Guid baseGuid, DateTime timestamp) {
      Span<byte> guidBytes = stackalloc byte[16];
      baseGuid.TryWriteBytes(guidBytes);

      // Write the timestamp into the last 6 bytes of the GUID
      Span<byte> timestampBytes = guidBytes.Slice(10, TimestampByteLength);
      WriteTimestampBytes(timestampBytes, timestamp);

      return new Guid(guidBytes);
    }

    /// <summary>
    /// Extracts the embedded timestamp from a COMB GUID.
    /// Allows retrieval of the timestamp component from a previously generated COMB GUID.
    /// </summary>
    /// <param name="combGuid">The COMB GUID from which to extract the timestamp.</param>
    /// <returns>The DateTime embedded in the provided COMB GUID.</returns>
    public static DateTime ExtractTimestamp(Guid combGuid) {
      Span<byte> guidBytes = stackalloc byte[16];
      combGuid.TryWriteBytes(guidBytes);

      // Extract the timestamp bytes from the last 6 bytes of the GUID
      ReadOnlySpan<byte> timestampBytes = guidBytes.Slice(10, TimestampByteLength);
      return ReadTimestampFromBytes(timestampBytes);
    }

    /// <summary>
    /// Converts a DateTime to a series of bytes and writes it to the specified destination.
    /// Only the last 6 bytes of the Unix time in milliseconds are used to conserve space.
    /// </summary>
    /// <param name="destination">The destination span where the timestamp bytes will be written.</param>
    /// <param name="timestamp">The DateTime value to convert and write as bytes.</param>
    private static void WriteTimestampBytes(Span<byte> destination, DateTime timestamp) {
      long unixTimeMilliseconds = ConvertToUnixTimeMilliseconds(timestamp);
      Span<byte> unixTimeBytes = stackalloc byte[8];
      BinaryPrimitives.WriteInt64BigEndian(unixTimeBytes, unixTimeMilliseconds);
      unixTimeBytes.Slice(2, TimestampByteLength).CopyTo(destination); // Use only the last 6 bytes
    }

    /// <summary>
    /// Reads and converts a series of bytes back into a DateTime value.
    /// The source bytes represent the last 6 bytes of the Unix time in milliseconds.
    /// </summary>
    /// <param name="source">The source span containing the timestamp bytes to convert.</param>
    /// <returns>The DateTime value derived from the provided bytes.</returns>
    private static DateTime ReadTimestampFromBytes(ReadOnlySpan<byte> source) {
      Span<byte> unixTimeBytes = stackalloc byte[8];
      source.CopyTo(unixTimeBytes.Slice(2, TimestampByteLength)); // Copy the source to the last 6 bytes
      unixTimeBytes.Slice(0, 2).Clear(); // Zero the first 2 bytes
      long unixTimeMilliseconds = BinaryPrimitives.ReadInt64BigEndian(unixTimeBytes);
      return ConvertFromUnixTimeMilliseconds(unixTimeMilliseconds);
    }

    /// <summary>
    /// Converts a DateTime to Unix time in milliseconds since the Unix epoch (January 1, 1970).
    /// This format is commonly used for storing and transmitting time as an integer.
    /// </summary>
    /// <param name="timestamp">The DateTime to convert to Unix time in milliseconds.</param>
    /// <returns>The Unix time in milliseconds representing the given DateTime.</returns>
    private static long ConvertToUnixTimeMilliseconds(DateTime timestamp) {
      return (long)(timestamp.ToUniversalTime() - UnixEpoch).TotalMilliseconds;
    }

    /// <summary>
    /// Converts Unix time in milliseconds to a DateTime object.
    /// Useful for interpreting integer time values as readable dates.
    /// </summary>
    /// <param name="milliseconds">The Unix time in milliseconds to convert.</param>
    /// <returns>A DateTime object representing the given Unix time in milliseconds.</returns>
    private static DateTime ConvertFromUnixTimeMilliseconds(long milliseconds) {
      return UnixEpoch.AddMilliseconds(milliseconds);
    }
  }
}
