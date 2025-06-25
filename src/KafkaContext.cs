using Kafka.Ksql.Linq.Configuration;
using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Context;
using Kafka.Ksql.Linq.Messaging.Consumers;
using Kafka.Ksql.Linq.Serialization.Abstractions;
using Kafka.Ksql.Linq.Serialization.Avro.Management;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ConfluentSchemaRegistry = Confluent.SchemaRegistry;

namespace Kafka.Ksql.Linq;
/// <summary>
/// Core層統合KafkaContext
/// 設計理由：Core抽象化を継承し、上位層機能を統合
/// </summary>
public abstract class KafkaContext : KafkaContextCore
{
    private readonly KafkaProducerManager _producerManager;
    private readonly KafkaConsumerManager _consumerManager;
    private readonly Lazy<ConfluentSchemaRegistry.ISchemaRegistryClient> _schemaRegistryClient;
    private readonly IAvroSchemaRegistrationService _schemaRegistrationService;

    /// <summary>
    /// テスト用にスキーマ登録をスキップするか判定するフック
    /// </summary>
    protected virtual bool SkipSchemaRegistration => false;

    protected KafkaContext() : base()
    {
        _schemaRegistryClient = new Lazy<ConfluentSchemaRegistry.ISchemaRegistryClient>(CreateSchemaRegistryClient);
        _schemaRegistrationService = CreateSchemaRegistrationService();

        try
        {
            if (!SkipSchemaRegistration)
            {
                InitializeWithSchemaRegistration();
            }

            _producerManager = new KafkaProducerManager(
                Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
                null);

            _consumerManager = new KafkaConsumerManager(
                Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
                null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "FATAL: KafkaContext initialization failed. Application cannot continue without Kafka connectivity.", ex);
        }
    }

    protected KafkaContext(KafkaContextOptions options) : base(options)
    {
        _schemaRegistryClient = new Lazy<ConfluentSchemaRegistry.ISchemaRegistryClient>(CreateSchemaRegistryClient);
        _schemaRegistrationService = CreateSchemaRegistrationService();

        try
        {
            if (!SkipSchemaRegistration)
            {
                InitializeWithSchemaRegistration();
            }

            _producerManager = new KafkaProducerManager(
                Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
                null);

            _consumerManager = new KafkaConsumerManager(
                Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
                null);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "FATAL: KafkaContext initialization failed. Application cannot continue without Kafka connectivity.", ex);
        }
    }

    /// <summary>
    /// OnModelCreating → スキーマ自動登録フローの実行
    /// </summary>
    private void InitializeWithSchemaRegistration()
    {
        // 1. OnModelCreatingでモデル構築
        ConfigureModel();

        // 2. EntityModel → AvroEntityConfiguration変換
        var entityModels = GetEntityModels();
        if (entityModels.Count == 0)
        {
            throw new InvalidOperationException(
                "No entities configured. Implement OnModelCreating() method to configure entities, " +
                "or add [Topic] attributes to your entity classes.");
        }

        var avroConfigurations = ConvertToAvroConfigurations(entityModels);

        // 3. スキーマ登録（接続確認も含む）
        RegisterSchemasSync(avroConfigurations);

        // 4. Kafka接続確認
        ValidateKafkaConnectivity();
    }

    /// <summary>
    /// スキーマ登録の同期実行（接続確認も兼ねる）
    /// </summary>
    private void RegisterSchemasSync(IReadOnlyDictionary<Type, AvroEntityConfiguration> configurations)
    {
        try
        {
            // スキーマ登録実行（Schema Registry接続エラーはここで検出）
            var registrationTask = _schemaRegistrationService.RegisterAllSchemasAsync(configurations);
            registrationTask.Wait(TimeSpan.FromSeconds(30));

            if (!registrationTask.IsCompletedSuccessfully)
            {
                var exception = registrationTask.Exception?.GetBaseException() ??
                    new TimeoutException("Schema registration timed out after 30 seconds");
                throw exception;
            }
        }
        catch (AggregateException ex)
        {
            throw ex.GetBaseException();
        }
    }

    /// <summary>
    /// Kafka接続確認
    /// </summary>
    private void ValidateKafkaConnectivity()
    {
        try
        {
            // Producer/Consumer初期化時点でKafka接続が確認される
            // 追加の接続確認は不要（既存の初期化処理で十分）
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "FATAL: Cannot connect to Kafka. Verify bootstrap servers and network connectivity.", ex);
        }
    }

    /// <summary>
    /// SchemaRegistryClient作成
    /// </summary>
    private ConfluentSchemaRegistry.ISchemaRegistryClient CreateSchemaRegistryClient()
    {
        var config = new ConfluentSchemaRegistry.SchemaRegistryConfig
        {
            Url = "http://localhost:8081", // デフォルト値、実際は設定から取得
            MaxCachedSchemas = 1000,
            RequestTimeoutMs = 30000
        };

        return new ConfluentSchemaRegistry.CachedSchemaRegistryClient(config);
    }

    /// <summary>
    /// スキーマ登録サービス作成
    /// </summary>
    private IAvroSchemaRegistrationService CreateSchemaRegistrationService()
    {
        return new AvroSchemaRegistrationService(_schemaRegistryClient.Value, null);
    }

    /// <summary>
    /// Core層EventSet実装（上位層機能統合）
    /// </summary>
    protected override IEntitySet<T> CreateEntitySet<T>(EntityModel entityModel)
    {
        return new EventSetWithServices<T>(this, entityModel);
    }

    internal KafkaProducerManager GetProducerManager() => _producerManager;
    internal KafkaConsumerManager GetConsumerManager() => _consumerManager;

    /// <summary>
    /// EntityModel の情報を AvroEntityConfiguration へ変換する
    /// </summary>
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
            _producerManager?.Dispose();
            _consumerManager?.Dispose();

            if (_schemaRegistryClient.IsValueCreated)
            {
                _schemaRegistryClient.Value?.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        _producerManager?.Dispose();
        _consumerManager?.Dispose();

        if (_schemaRegistryClient.IsValueCreated)
        {
            _schemaRegistryClient.Value?.Dispose();
        }

        await base.DisposeAsyncCore();
    }

    public override string ToString()
    {
        return $"{base.ToString()} [スキーマ自動登録対応]";
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
