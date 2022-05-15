using Xunit;

namespace InsaneGenius.Utilities.Tests;

public class FormatTests
{
    [Theory]
    [InlineData(0, "0B")]
    [InlineData(1, "1B")]
    [InlineData(-1, "-1B")]
    [InlineData(512, "512B")]
    [InlineData(4096, "4KiB")]
    [InlineData(5000, "4.90KiB")]
    [InlineData(256 * Format.MiB, "256MiB")]
    [InlineData(Format.KiB, "1KiB")]
    [InlineData(Format.MiB, "1MiB")]
    [InlineData(Format.GiB, "1GiB")]
    [InlineData(Format.TiB, "1TiB")]
    [InlineData(Format.PiB, "1PiB")]
    [InlineData(Format.EiB, "1EiB")]
    [InlineData(int.MinValue, "-2GiB")]
    [InlineData(int.MaxValue, "2GiB")]
    [InlineData(long.MinValue + 1, "-8EiB")]
    [InlineData(long.MaxValue, "8EiB")]
    public void BytestoKibi(long value, string output)
    {
        string kibi = Format.BytesToKibi(value);
        Assert.Equal(kibi, output);
    }

    [Theory]
    [InlineData(0, "0B")]
    [InlineData(1, "1B")]
    [InlineData(-1, "-1B")]
    [InlineData(512, "512B")]
    [InlineData(4096, "4.10KB")]
    [InlineData(5000, "5KB")]
    [InlineData(256 * Format.MB, "256MB")]
    [InlineData(Format.KB, "1KB")]
    [InlineData(Format.MB, "1MB")]
    [InlineData(Format.GB, "1GB")]
    [InlineData(Format.TB, "1TB")]
    [InlineData(Format.PB, "1PB")]
    [InlineData(Format.EB, "1EB")]
    [InlineData(int.MinValue, "-2.10GB")]
    [InlineData(int.MaxValue, "2.10GB")]
    [InlineData(long.MinValue + 1, "-9.20EB")]
    [InlineData(long.MaxValue, "9.20EB")]
    public void BytestoKilo(long value, string output)
    {
        string kilo = Format.BytesToKilo(value);
        Assert.Equal(kilo, output);
    }
}