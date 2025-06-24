// src/KafkaContext.cs - Core層統合完全版
using Kafka.Ksql.Linq.Configuration;
using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Context;
using Kafka.Ksql.Linq.Messaging.Consumers;
using Kafka.Ksql.Linq.Serialization.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq;
/// <summary>
/// Core層統合KafkaContext
/// 設計理由：Core抽象化を継承し、上位層機能を統合
/// </summary>
public abstract class KafkaContext : KafkaContextCore
{
    private readonly KafkaProducerManager _producerManager;
    private readonly KafkaConsumerManager _consumerManager;

    protected KafkaContext() : base()
    {
        _producerManager = new KafkaProducerManager(
            Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
            null);

        _consumerManager = new KafkaConsumerManager(
            Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
            null);
    }

    protected KafkaContext(KafkaContextOptions options) : base(options)
    {
        _producerManager = new KafkaProducerManager(
          Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
          null);

        _consumerManager = new KafkaConsumerManager(
            Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
            null);
    }

    /// <summary>
    /// Core層EventSet実装（上位層機能統合）
    /// </summary>
    protected override IEntitySet<T> CreateEntitySet<T>(EntityModel entityModel)
    {
        return new EventSetWithServices<T>(this, entityModel);
    }

    // Core層統合API
    internal KafkaProducerManager GetProducerManager() => _producerManager;
    internal KafkaConsumerManager GetConsumerManager() => _consumerManager;

    /// <summary>
    /// EntityModel の情報を AvroEntityConfiguration へ変換する。
    /// </summary>
    /// <param name="entityModels">変換対象のモデル一覧</param>
    /// <returns>変換後の AvroEntityConfiguration マップ</returns>
    /// <remarks>
    /// テストからリフレクションで呼び出されるためアクセスレベルを
    /// <c>protected</c> に変更する。
    /// </remarks>
    protected IReadOnlyDictionary<Type, AvroEntityConfiguration> ConvertToAvroConfigurations(
        Dictionary<Type, EntityModel> entityModels)
    {
        var avroConfigs = new Dictionary<Type, AvroEntityConfiguration>();

        foreach (var kvp in entityModels)
        {
            var entityModel = kvp.Value;
            var avroConfig = new AvroEntityConfiguration(entityModel.EntityType)
            {
                TopicName = entityModel.TopicAttribute?.TopicName,
                KeyProperties = entityModel.KeyProperties
            };

            avroConfigs[kvp.Key] = avroConfig;
        }

        return avroConfigs;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _producerManager.Dispose();
            _consumerManager.Dispose();


        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        // 上位層サービスの非同期破棄
        _producerManager.Dispose();
        _consumerManager.Dispose();

        await base.DisposeAsyncCore();
    }
    public override string ToString()
    {
        return $"{base.ToString()} [Core層統合]";
    }
}


/// <summary>
/// 上位層サービス統合EventSet
/// 設計理由：Core抽象化を実装し、Producer/Consumer機能を提供
/// </summary>
internal class EventSetWithServices<T> : EventSet<T> where T : class
{
    private readonly KafkaContext _kafkaContext;

    public EventSetWithServices(KafkaContext context, EntityModel entityModel)
        : base(context, entityModel)
    {
        _kafkaContext = context;
    }

    public EventSetWithServices(KafkaContext context, EntityModel entityModel, System.Linq.Expressions.Expression expression)
        : base(context, entityModel)
    {
        _kafkaContext = context;
    }

    /// <summary>
    /// Core抽象化実装：Producer機能
    /// </summary>
    protected override async Task SendEntityAsync(T entity, CancellationToken cancellationToken)
    {
        try
        {
            var producerManager = _kafkaContext.GetProducerManager();

            var context = new KafkaMessageContext
            {
                MessageId = Guid.NewGuid().ToString(),
                Tags = new Dictionary<string, object>
                {
                    ["entity_type"] = typeof(T).Name,
                    ["method"] = "Core.SendEntityAsync"
                }
            };

            await producerManager.SendAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Core層統合: Entity送信失敗 - {typeof(T).Name}", ex);
        }
    }
}

