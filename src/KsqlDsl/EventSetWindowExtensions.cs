using Kafka.Ksql.Linq.Query.Abstractions;
using System;

namespace Kafka.Ksql.Linq;

/// <summary>
/// Window DSL extension methods for EventSet
/// </summary>
internal static class EventSetWindowExtensions
{
    internal static IEventSet<T> Window<T>(this IEventSet<T> source, WindowDef windowDef) where T : class
    {
        // This method is only for expression translation and should not be executed
        throw new NotSupportedException("Window is for expression translation only.");
    }
}

/// <summary>
/// Represents a chainable window definition used only for LINQ expression generation.
/// </summary>
public class WindowDef
{
    public WindowDef TumblingWindow() => this;
    public WindowDef HoppingWindow() => this;
    public WindowDef SessionWindow() => this;
    public WindowDef Size(TimeSpan ts) => this;
    public WindowDef AdvanceBy(TimeSpan ts) => this;
    public WindowDef Gap(TimeSpan ts) => this;
    public WindowDef Retention(TimeSpan ts) => this;
    public WindowDef GracePeriod(TimeSpan ts) => this;
    public WindowDef EmitFinal() => this;
}

public static class TumblingWindow
{
    public static WindowDef Of(TimeSpan size) => new WindowDef().TumblingWindow().Size(size);
    public static WindowDef OfMinutes(int minutes) => Of(TimeSpan.FromMinutes(minutes));
}

public static class HoppingWindow
{
    public static WindowDef Of(TimeSpan size) => new WindowDef().HoppingWindow().Size(size);
    public static WindowDef OfMinutes(int minutes) => Of(TimeSpan.FromMinutes(minutes));
}

public static class SessionWindow
{
    public static WindowDef Of(TimeSpan gap) => new WindowDef().SessionWindow().Gap(gap);
    public static WindowDef OfMinutes(int minutes) => Of(TimeSpan.FromMinutes(minutes));
}
