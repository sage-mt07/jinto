using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Modeling;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.ModelBuilderTests;

public class SpecialTypeHandlingTests
{
    private class CharEntity
    {
        [Key]
        public int Id { get; set; }
        public char Code { get; set; }
    }

    private class ShortEntity
    {
        [Key]
        public int Id { get; set; }
        public short Quantity { get; set; }
    }

    [Fact]
    public void CharProperty_ProducesWarning()
    {
        var builder = new ModelBuilder(ValidationMode.Loose);
        builder.Entity<CharEntity>();

        var model = builder.GetEntityModel<CharEntity>()!;
        Assert.Contains(model.ValidationResult!.Warnings, w => w.Contains("char"));
    }

    [Fact]
    public void ShortProperty_MappedAsInteger()
    {
        var generator = new Kafka.Ksql.Linq.Query.Pipeline.DDLQueryGenerator(new Kafka.Ksql.Linq.Query.Pipeline.SelectBuilder(), new Kafka.Ksql.Linq.Query.Pipeline.WhereBuilder(), new Kafka.Ksql.Linq.Query.Pipeline.GroupByBuilder());
        var type = typeof(short);
        var method = typeof(Kafka.Ksql.Linq.Query.Pipeline.DDLQueryGenerator).GetMethod("MapToKsqlType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var result = (string)method.Invoke(generator, new object[] { type })!;
        Assert.Equal("INTEGER", result);
    }
}
