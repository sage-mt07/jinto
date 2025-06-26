// src/Application/KsqlContext.cs - 簡素化・統合版
// 重複削除、未実装参照削除、Monitoring初期化除去済み

using Kafka.Ksql.Linq.Configuration;
using Kafka.Ksql.Linq.Core.Abstractions;
using Kafka.Ksql.Linq.Core.Context;
using Kafka.Ksql.Linq.Messaging.Consumers;
using Kafka.Ksql.Linq.Messaging.Contracts;
using Kafka.Ksql.Linq.Messaging.Internal;
using Kafka.Ksql.Linq.Serialization.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kafka.Ksql.Linq.Application;

/// <summary>
/// 簡素化統合KafkaContext
/// 設計理由: Pool削除、直接管理、EF風API
/// </summary>
public abstract class KafkaContext : KafkaContextCore
{
    // 簡素化Manager（Pool削除版）
    private readonly KafkaProducerManager _producerManager;
    private readonly KafkaConsumerManager _consumerManager;

    protected KafkaContext() : base()
    {
        _producerManager = new KafkaProducerManager(
            Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
            null   // LoggerFactory削除
        );

        _consumerManager = new KafkaConsumerManager(
            Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
            null   // LoggerFactory削除
        );
    }

    protected KafkaContext(KafkaContextOptions options) : base(options)
    {
        _producerManager = new KafkaProducerManager(
            Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
            null   // LoggerFactory削除
        );

        _consumerManager = new KafkaConsumerManager(
            Microsoft.Extensions.Options.Options.Create(new KsqlDslOptions()),
            null   // LoggerFactory削除
        );
    }

    /// <summary>
    /// Core層EventSet実装（簡素化版）
    /// </summary>
    protected override IEntitySet<T> CreateEntitySet<T>(EntityModel entityModel)
    {
        return new EventSetWithSimplifiedServices<T>(this, entityModel);
    }

    // 簡素化Manager取得
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
            _producerManager?.Dispose();
            _consumerManager?.Dispose();
        }

        base.Dispose(disposing);
    }

    public override string ToString()
    {
        return $"{base.ToString()} [簡素化統合]";
    }
}
internal class EventSetWithSimplifiedServices<T> : EventSet<T> where T : class
{
    private readonly KafkaContext _kafkaContext;

    public EventSetWithSimplifiedServices(KafkaContext context, EntityModel entityModel, IErrorSink? errorSink = null)
        : base(context, entityModel, errorSink)
    {
        _kafkaContext = context;
    }

    public EventSetWithSimplifiedServices(KafkaContext context, EntityModel entityModel, System.Linq.Expressions.Expression expression, IErrorSink? errorSink = null)
        : base(context, entityModel, errorSink)
    {
        _kafkaContext = context;
    }

    /// <summary>
    /// ✅ NEW: 簡素化版Kafka継続受信のGetAsyncEnumerator実装（エラー修正版）
    /// try-catch yield問題を解決：例外処理を別メソッドに分離
    /// </summary>
    public override async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var consumerManager = _kafkaContext.GetConsumerManager();

        while (!cancellationToken.IsCancellationRequested)
        {
            // 受信処理を別メソッドに分離してtry-catch yield問題を回避
            var receiveResult = await SafeReceiveFromSimplifiedKafkaAsync(consumerManager, cancellationToken);

            // キャンセル要求時は終了
            if (receiveResult.ShouldBreak)
                yield break;

            // 正常受信時のみyield
            if (receiveResult.Item != null)
            {
                yield return receiveResult.Item;
            }
            else
            {
                // データなし時の待機（簡素化版では固定100ms）
                await Task.Delay(100, cancellationToken);
            }
        }
    }

    /// <summary>
    /// try-catch yield問題回避のための受信結果ラッパー
    /// </summary>
    private record ReceiveResult(T? Item, bool ShouldBreak);

    /// <summary>
    /// 安全な簡素化Kafka受信処理（例外処理を分離）
    /// </summary>
    private async Task<ReceiveResult> SafeReceiveFromSimplifiedKafkaAsync(KafkaConsumerManager consumerManager, CancellationToken cancellationToken)
    {
        try
        {
            var receivedItem = await ReceiveFromSimplifiedKafkaAsync(consumerManager, cancellationToken);
            return new ReceiveResult(receivedItem, false);
        }
        catch (OperationCanceledException)
        {
            // キャンセル時は終了フラグを立てる
            return new ReceiveResult(null, true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"簡素化統合: Kafka継続受信失敗 - {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// 簡素化KafkaConsumerManagerからの受信
    /// </summary>
    private async Task<T?> ReceiveFromSimplifiedKafkaAsync(KafkaConsumerManager consumerManager, CancellationToken cancellationToken)
    {
        try
        {
            // 簡素化版実装: 基本的な受信処理
            // TODO: 実際のKafkaConsumerManager.ReceiveAsync<T>()呼び出し

            // 仮実装での動的メソッド呼び出し
            var method = consumerManager.GetType().GetMethod("ReceiveAsync");
            if (method?.IsGenericMethodDefinition == true)
            {
                var genericMethod = method.MakeGenericMethod(typeof(T));
                var parameters = new object[] { GetTopicName(), cancellationToken };

                if (genericMethod.Invoke(consumerManager, parameters) is Task task)
                {
                    await task;
                    var resultProperty = task.GetType().GetProperty("Result");
                    return (T?)resultProperty?.GetValue(task);
                }
            }

            // メソッドが見つからない場合のフォールバック
            return null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"簡素化Manager受信失敗: {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// ✅ Core抽象化実装：Producer機能（簡素化）
    /// </summary>
    protected override async Task SendEntityAsync(T entity, CancellationToken cancellationToken)
    {
        try
        {
            var producerManager = _kafkaContext.GetProducerManager();
            await producerManager.SendAsync(entity, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"簡素化統合: Entity送信失敗 - {typeof(T).Name}", ex);
        }
    }

    public override string ToString()
    {
        return $"EventSetWithSimplifiedServices<{typeof(T).Name}> - Pool削除・継続受信対応";
    }
}
