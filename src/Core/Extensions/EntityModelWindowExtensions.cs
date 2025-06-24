using Kafka.Ksql.Linq.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Core.Extensions;

public static class EntityModelWindowExtensions
{
    /// <summary>
    /// [AvroTimestamp]属性を持つプロパティを取得
    /// </summary>
    public static PropertyInfo? GetTimestampProperty(this EntityModel entityModel)
    {
        return entityModel.AllProperties
            .FirstOrDefault(p => p.GetCustomAttribute<AvroTimestampAttribute>() != null);
    }

    /// <summary>
    /// エンティティがウィンドウ処理に対応しているかチェック
    /// </summary>
    public static bool IsWindowCapable(this EntityModel entityModel)
    {
        return entityModel.GetTimestampProperty() != null;
    }

    /// <summary>
    /// ウィンドウ設定が存在するかチェック
    /// </summary>
    public static bool HasWindowConfiguration(this EntityModel entityModel)
    {
        return entityModel.ValidationResult?.Warnings
            .Any(w => w.Contains("Window configuration")) == true;
    }

    /// <summary>
    /// タイムスタンププロパティの検証
    /// </summary>
    public static ValidationResult ValidateWindowSupport(this EntityModel entityModel)
    {
        var result = new ValidationResult { IsValid = true };

        var timestampProperty = entityModel.GetTimestampProperty();
        if (timestampProperty == null)
        {
            result.IsValid = false;
            result.Errors.Add($"Entity {entityModel.EntityType.Name} requires [AvroTimestamp] attribute on a DateTime property for window operations");
            return result;
        }

        // タイムスタンププロパティの型チェック
        var propType = timestampProperty.PropertyType;
        var validTypes = new[] { typeof(DateTime), typeof(DateTime?), typeof(DateTimeOffset), typeof(DateTimeOffset?) };

        if (!validTypes.Contains(propType))
        {
            result.IsValid = false;
            result.Errors.Add($"Property {timestampProperty.Name} with [AvroTimestamp] must be DateTime or DateTimeOffset type, but was {propType.Name}");
        }

        return result;
    }
}
