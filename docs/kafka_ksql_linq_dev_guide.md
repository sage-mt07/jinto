# Kafka.Ksql.Linq 開発者ガイド

## 🎯 新機能開発前の必読事項

**重複実装を避けるため、新機能開発前に必ずこのガイドを確認してください。**

---

## 📋 既存機能マップ

### Producer/Consumer 管理
```
📍 場所: src/Messaging/
🔧 既存機能:
  ✅ 型安全Producer/Consumer (KafkaProducerManager, KafkaConsumerManager)
  ✅ バッチ送信機能 (SendBatchAsync)
  ✅ エラーリトライ機能 (ResilientAvroSerializerManager)
  ✅ 購読パターン (SubscribeAsync, ConsumeAsync)

❌ 追加実装不要: 新しいProducer/Consumerマネージャー
✅ 拡張可能: 新しいシリアライゼーション形式
```

### スキーマ管理
```
📍 場所: src/Serialization/Avro/
🔧 既存機能:
  ✅ 自動スキーマ登録 (AvroSchemaRegistrationService)
  ✅ スキーマ生成 (UnifiedSchemaGenerator)
  ✅ バージョン管理 (AvroSchemaVersionManager)
  ✅ 互換性チェック (CheckCompatibilityAsync)

❌ 追加実装不要: 新しいスキーマ管理システム
✅ 拡張可能: 新しいスキーマ形式対応
```

### StateStore 管理
```
📍 場所: src/StateStore/
🔧 既存機能:
  ✅ RocksDB統合 (RocksDbStateStore)
  ✅ Window処理 (WindowedEntitySet)
  ✅ Ready状態監視 (ReadyStateMonitor)
  ✅ トピック同期 (TopicStateStoreBinding)

❌ 追加実装不要: 新しいStateStoreマネージャー
✅ 拡張可能: 新しいストレージバックエンド
```

### LINQ to KSQL 変換
```
📍 場所: src/Query/
🔧 既存機能:
  ✅ 式木解析 (StreamTableAnalyzer)
  ✅ KSQL生成 (DDLQueryGenerator, DMLQueryGenerator)
  ✅ Builder パターン (SelectBuilder, GroupByBuilder, JoinBuilder)
  ✅ JOIN操作 (UnifiedJoinResult, JoinableEntitySet)

❌ 追加実装不要: 新しいクエリビルダー
✅ 拡張可能: 新しいLINQ演算子対応
```

### エラーハンドリング
```
📍 場所: src/Core/Abstractions/
🔧 既存機能:
  ✅ リトライポリシー (ErrorHandlingPolicy)
  ✅ 回路ブレーカー (CircuitBreakerHandler)
  ✅ エラーメトリクス (ErrorMetrics)
  ✅ カスタムハンドラー (OnError拡張メソッド)

❌ 追加実装不要: 新しいエラーハンドリングシステム
✅ 拡張可能: 新しいエラーアクション種別
```

---

## 🏗️ アーキテクチャ層と責務

```
┌─────────────────┐
│   Application   │ ← KafkaContext, Builder, Options
├─────────────────┤
│      Core       │ ← 抽象化、EntityModel、Validation
├─────────────────┤
│   Messaging     │ ← Producer/Consumer、Configuration
├─────────────────┤
│ Serialization   │ ← Avro、Schema Registry連携
├─────────────────┤
│     Query       │ ← LINQ to KSQL変換、Pipeline
├─────────────────┤
│   StateStore    │ ← RocksDB、Window、Monitoring
└─────────────────┘
```

**依存関係ルール:**
- 上位層 → 下位層のみ依存可能
- 同一層内での相互依存は禁止
- Core層は他層に依存しない

---

## 🔧 新機能追加時の判定フロー

### 1. Producer/Consumer関連の新機能
```
❓ 質問: 型安全性は保たれていますか？
  YES → KafkaProducerManager/KafkaConsumerManagerを拡張
  NO  → 新しいマネージャーが必要

❓ 質問: エラーハンドリングは既存パターンで対応可能？
  YES → ErrorHandlingPolicyを拡張
  NO  → 新しいエラーアクション追加
```

### 2. スキーマ関連の新機能
```
❓ 質問: Avro以外のシリアライゼーション形式？
  YES → 新しいSerializationManagerが必要
  NO  → AvroSerializationManagerを拡張

❓ 質問: Schema Registry以外のスキーマ管理？
  YES → 新しいRegistrationServiceが必要
  NO  → AvroSchemaRegistrationServiceを拡張
```

### 3. クエリ関連の新機能
```
❓ 質問: 新しいLINQ演算子のサポート？
  YES → 対応するBuilderクラスを拡張
  NO  → 既存のQuery Pipelineを確認

❓ 質問: 新しいJOIN種別？
  YES → JoinBuilderを拡張
  NO  → 既存のJOIN実装を確認
```

---

## 📝 実装パターン

### エンティティ追加パターン
```csharp
// 1. Entity定義
[Topic("orders")]
public class Order
{
    [Key]
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}

// 2. Context追加
public class MyKafkaContext : KafkaContext
{
    public IEntitySet<Order> Orders => Set<Order>();
}

// 3. 使用例
await context.Orders.AddAsync(new Order { ... });
var orders = await context.Orders.ToListAsync();
```

### エラーハンドリングパターン
```csharp
// 既存のErrorHandlingPolicyを使用
await context.Orders
    .OnError(ErrorAction.Retry)
    .WithRetry(3, TimeSpan.FromSeconds(1))
    .ToListAsync();
```

### StateStore連携パターン
```csharp
// 既存のWindow拡張を使用
var windowedOrders = context.Orders.Window(5); // 5分ウィンドウ
await windowedOrders.ToListAsync();
```

---

## 🚀 開発完了後の更新手順

1. **機能マップの更新**
   - 新機能を適切なセクションに追加
   - 既存機能への影響を記載

2. **判定フローの更新**
   - 新しい判定分岐が必要な場合は追加
   - 既存フローの調整

3. **実装パターンの追加**
   - 新しいパターンが生まれた場合は例を追加

---

## 🔍 重複チェックリスト

新機能開発前に以下を確認：

- [ ] 同じ責務のManagerクラスは既に存在しないか？
- [ ] 似たようなBuilder/Extensionメソッドは既にないか？
- [ ] 同じエラーハンドリングパターンは実装済みではないか？
- [ ] 既存のConfiguration/Optionsで設定可能ではないか？
- [ ] 類似のSerializationManagerは既にないか？

---

## 📚 参考情報

- **設計原則**: EF Core風のAPI、型安全性、非同期ファースト
- **コーディング規約**: nullable reference types有効、ConfigureAwait(false)使用
- **テスト方針**: 単体テスト必須、統合テストでE2E確認

---

*最終更新: [新機能開発完了時に更新]*