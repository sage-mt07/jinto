using KsqlDsl.Core.Abstractions;
using KsqlDsl.Core.Modeling;
using System;
using System.Linq;
using Xunit;

namespace KsqlDsl.Tests.Core;

public class ModelBuilderTests
{
    [Topic("sample")]
    private class Sample
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public void AddAndGetEntityModel_Works()
    {
        var builder = new ModelBuilder();
        builder.AddEntityModel<Sample>();
        var model = builder.GetEntityModel<Sample>();
        Assert.NotNull(model);
        Assert.True(model!.HasKeys());
        Assert.Equal("sample", model.GetTopicName());
    }

    [Fact]
    public void GetAllEntityModels_ReturnsDictionary()
    {
        var builder = new ModelBuilder();
        builder.AddEntityModel<Sample>();
        var all = builder.GetAllEntityModels();
        Assert.Single(all);
        Assert.True(all.ContainsKey(typeof(Sample)));
    }

    [Fact]
    public void ValidateAllModels_ReturnsTrueForValidModel()
    {
        var builder = new ModelBuilder();
        builder.AddEntityModel<Sample>();
        Assert.True(builder.ValidateAllModels());
    }

    [Fact]
    public void GetModelSummary_ContainsEntity()
    {
        var builder = new ModelBuilder();
        builder.AddEntityModel<Sample>();
        var summary = builder.GetModelSummary();
        Assert.Contains("Sample", summary);
    }

    [Fact]
    public void PrivateMethods_Work()
    {
        var builder = new ModelBuilder();
        builder.AddEntityModel<Sample>();
        var model = builder.GetEntityModel<Sample>();
        bool validType = PrivateAccessor.InvokePrivate<bool>(builder, "IsValidPropertyType", new[] { typeof(Type) }, args: new object[] { typeof(int) });
        Assert.True(validType);
        var validation = PrivateAccessor.InvokePrivate<ValidationResult>(builder, "ValidateEntityModel", new[] { typeof(Type), typeof(EntityModel) }, args: new object?[] { typeof(Sample), model });
        Assert.True(validation.IsValid);
        var created = PrivateAccessor.InvokePrivate<EntityModel>(builder, "CreateEntityModelFromType", new[] { typeof(Type) }, args: new object[] { typeof(Sample) });
        Assert.Equal(typeof(Sample), created.EntityType);
    }
}
