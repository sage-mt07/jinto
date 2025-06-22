using KsqlDsl.Serialization.Avro.Core;
using KsqlDsl.Core.Abstractions;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace KsqlDsl.Tests.Serialization;

public class AvroSerializerTests
{
    private class Sample { }

    [Fact]
    public void Constructor_AllowsNullLoggerFactory()
    {
        var serializer = new AvroSerializer<Sample>(null);
        var field = typeof(AvroSerializer<Sample>).GetField("_logger", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.NotNull(field.GetValue(serializer));
    }

    [Fact]
    public void Serialize_ThrowsNotSupported()
    {
        var serializer = new AvroSerializer<Sample>();
        Assert.Throws<NotSupportedException>(() => serializer.Serialize(new Sample(), new MemoryStream()));
    }

    [Fact]
    public void Dispose_SetsDisposedFlag()
    {
        var serializer = new AvroSerializer<Sample>();
        serializer.Dispose();
        var field = typeof(AvroSerializer<Sample>).GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.True((bool)field.GetValue(serializer)!);
    }
}
