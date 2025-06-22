using KsqlDsl.Application;
using KsqlDsl.Core.Abstractions;
using KsqlDsl.Core.Context;
using KsqlDsl.Serialization.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using KsqlDsl.Tests;

namespace KsqlDsl.Tests.Application;

public class KafkaContextExtraTests
{
    private class TestContext : KafkaContext
    {
        public TestContext() : base() { }
        public IReadOnlyDictionary<Type, AvroEntityConfiguration> CallConvert(Dictionary<Type, EntityModel> models)
            => PrivateAccessor.InvokePrivate<IReadOnlyDictionary<Type, AvroEntityConfiguration>>(this,
                "ConvertToAvroConfigurations",
                new[] { typeof(Dictionary<Type, EntityModel>) },
                args: new object?[] { models });
    }

    private class Sample { [Key] public int Id { get; set; } }

    private static EntityModel CreateModel() => new()
    {
        EntityType = typeof(Sample),
        TopicAttribute = new TopicAttribute("t"),
        AllProperties = typeof(Sample).GetProperties(),
        KeyProperties = new[] { typeof(Sample).GetProperty(nameof(Sample.Id))! }
    };

    [Fact]
    public void ConvertToAvroConfigurations_MapsModels()
    {
        var ctx = new TestContext();
        var dict = new Dictionary<Type, EntityModel> { [typeof(Sample)] = CreateModel() };
        var result = ctx.CallConvert(dict);
        Assert.Single(result);
        var cfg = result[typeof(Sample)];
        Assert.Equal("t", cfg.TopicName);
        Assert.Single(cfg.KeyProperties!);
    }
}
