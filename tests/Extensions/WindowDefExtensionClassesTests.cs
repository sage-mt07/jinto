using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Ksql = Kafka.Ksql.Linq;
using Xunit;

namespace Kafka.Ksql.Linq.Tests.Extensions;

public class WindowDefExtensionClassesTests
{
    private static List<(string Name, object? Value)> GetOperations(Ksql.WindowDef def)
    {
        var field = typeof(Ksql.WindowDef).GetField("Operations", BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (List<(string Name, object? Value)>)field.GetValue(def)!;
    }

    [Fact]
    public void HoppingWindow_Of_BuildsOperations()
    {
        var ts = TimeSpan.FromMinutes(5);
        var def = Ksql.HoppingWindow.Of(ts);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("HoppingWindow", (object?)null), ("Size", (object?)ts) }, ops);
    }

    [Fact]
    public void HoppingWindow_OfMinutes_BuildsOperations()
    {
        var def = Ksql.HoppingWindow.OfMinutes(2);
        var ts = TimeSpan.FromMinutes(2);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("HoppingWindow", (object?)null), ("Size", (object?)ts) }, ops);
    }

    [Fact]
    public void SessionWindow_Of_BuildsOperations()
    {
        var ts = TimeSpan.FromMinutes(7);
        var def = Ksql.SessionWindow.Of(ts);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("SessionWindow", (object?)null), ("Gap", (object?)ts) }, ops);
    }

    [Fact]
    public void SessionWindow_OfMinutes_BuildsOperations()
    {
        var def = Ksql.SessionWindow.OfMinutes(3);
        var ts = TimeSpan.FromMinutes(3);
        var ops = GetOperations(def);
        Assert.Equal(new[] { ("SessionWindow", (object?)null), ("Gap", (object?)ts) }, ops);
    }
}
