using System;
using System.Reflection;

namespace KsqlDsl.Tests;

internal static class PrivateAccessor
{
    internal static object InvokePrivate(
        object target,
        string name,
        Type[] parameterTypes,
        Type[]? genericTypes = null,
        params object?[]? args)
    {
        var type = target as Type ?? target.GetType();
        var flags = BindingFlags.NonPublic | (target is Type ? BindingFlags.Static : BindingFlags.Instance);
        var method = type.GetMethod(name, flags, binder: null, types: parameterTypes, modifiers: null);
        if (method == null)
            throw new ArgumentException($"Method '{name}' with specified parameters not found on type '{type.FullName}'.");
        if (genericTypes != null && method.IsGenericMethodDefinition)
        {
            method = method.MakeGenericMethod(genericTypes);
        }
        return method.Invoke(target is Type ? null : target, args);
    }

    internal static T InvokePrivate<T>(
        object target,
        string name,
        Type[] parameterTypes,
        Type[]? genericTypes = null,
        params object?[]? args) => (T)InvokePrivate(target, name, parameterTypes, genericTypes, args)!;
}
