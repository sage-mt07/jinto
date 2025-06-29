using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Kafka.Ksql.Linq;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.Extensions;

public class WindowDefExtensionClassesTests
{
    private static List<(string Name, object? Value)> GetOperations(WindowDef def)
    {
        var field = typeof(WindowDef).GetField("Operations", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (List<(string Name, object? Value)>)field.GetValue(def)!;
    }

    [Fact]
    public void HoppingWindow_Of_BuildsOperations()
    {
        var ts = TimeSpan.FromMinutes(5);
        var def = HoppingWindow.Of(ts);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("HoppingWindow", (object?)null), ("Size", (object?)ts) }, ops);
    }

    [Fact]
    public void HoppingWindow_OfMinutes_BuildsOperations()
    {
        var def = HoppingWindow.OfMinutes(2);
        var ts = TimeSpan.FromMinutes(2);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("HoppingWindow", (object?)null), ("Size", (object?)ts) }, ops);
    }

    [Fact]
    public void SessionWindow_Of_BuildsOperations()
    {
        var ts = TimeSpan.FromMinutes(7);
        var def = SessionWindow.Of(ts);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("SessionWindow", (object?)null), ("Gap", (object?)ts) }, ops);
    }

    [Fact]
    public void SessionWindow_OfMinutes_BuildsOperations()
    {
        var def = SessionWindow.OfMinutes(3);
        var ts = TimeSpan.FromMinutes(3);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("SessionWindow", (object?)null), ("Gap", (object?)ts) }, ops);
    }
}
