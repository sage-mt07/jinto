# アーキテクチャ責務早見表

## 📋 概要
このドキュメントは、Kafka.Ksql.Linqライブラリの各層・モジュールの責務を整理した早見表です。

## 🏗️ 全体アーキテクチャ

```
┌─────────────────────────────────────────────────────────────┐
│                     Application Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │  KafkaContext   │  │ ContextBuilder  │  │   Options   │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────┐
│                       Core Layer                           │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐ │
│  │ Abstractions    │  │   Modeling      │  │ Validation  │ │
│  │ • IKafkaContext │  │ • EntityModel   │  │ • Rules     │ │
│  │ • IEntitySet    │  │ • ModelBuilder  │  │ • Errors    │ │
│  └─────────────────┘  └─────────────────┘  └─────────────┘ │
└─────────────────────────────────────────────────────────────┘
                                ↓
┌─────────────────────────────────────────────────────────────┐
│                     Infrastructure                         │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌────────┐ │
│  │ Messaging   │ │Serialization│ │    Query    │ │StateStore│
│  │ • Producer  │ │ • Avro      │ │ • Pipeline  │ │• RocksDB│ │
│  │ • Consumer  │ │ • Schema    │ │ • Builder   │ │• Window │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 📁 層別責務一覧

### 🔧 **Application Layer** (`src/Application/`)
> **責務**: 開発者向けAPI提供、設定管理、スキーマ自動登録

| クラス | 責務 | 重要度 |
|--------|------|--------|
| `KafkaContext` | スキーマ自動登録、Producer/Consumer統合 | ⭐⭐⭐ |
| `KsqlContextBuilder` | Fluent API設定、コンテキスト構築 | ⭐⭐ |
| `KsqlContextOptions` | 設定値管理、バリデーション | ⭐⭐ |
| `AvroSchemaInfoExtensions` | スキーマSubject名生成 | ⭐ |

**特徴**:
- ✅ **Fail-Fast**: スキーマ登録失敗時は即座にアプリ終了
- ✅ **自動化**: OnModelCreating → スキーマ登録 → 接続確認
- ✅ **EF風API**: DbContextライクな開発体験

---

### 🎯 **Core Layer** (`src/Core/`)
> **責務**: ビジネスロジック抽象化、型安全性、依存関係制御

#### **Abstractions** (`src/Core/Abstractions/`)
| インターフェース | 責務 | 実装場所 |
|------------------|------|----------|
| `IKafkaContext` | コンテキスト操作の抽象化 | Application |
| `IEntitySet<T>` | CRUD操作の統一API | Query/Core |
| `ISerializationManager<T>` | シリアライゼーション抽象化 | Serialization |
| `EntityModel` | エンティティメタデータ管理 | Core |

#### **Modeling** (`src/Core/Modeling/`)
| クラス | 責務 |
|--------|------|
| `ModelBuilder` | Fluent API + 属性ベース設定 |
| `EntityModelBuilder<T>` | 個別エンティティ設定 |
| `ValidationResult` | モデル検証結果 |

#### **Window** (`src/Core/Window/`)
| クラス | 責務 |
|--------|------|
| `WindowedEntitySet<T>` | ウィンドウ集約操作 |
| `WindowCollection<T>` | 複数ウィンドウ管理 |

**設計原則**:
- 🚫 **依存制限**: Infrastructure層への依存禁止
- ✅ **純粋関数**: 副作用なし、テスタブル
- ✅ **型安全**: ジェネリクス活用

---

### 📨 **Messaging Layer** (`src/Messaging/`)
> **責務**: Kafka通信、Producer/Consumer管理、エラーハンドリング

#### **Producer** (`src/Messaging/Producers/`)
| クラス | 責務 | 特徴 |
|--------|------|------|
| `KafkaProducerManager` | Producer生成・管理 | 型安全、Pool削除 |
| `KafkaProducer<T>` | 型安全メッセージ送信 | バッチ対応 |
| `DlqProducer` | DLQ送信、エラー処理 | 障害追跡 |

#### **Consumer** (`src/Messaging/Consumers/`)
| クラス | 責務 | 特徴 |
|--------|------|------|
| `KafkaConsumerManager` | Consumer生成・管理 | 事前確定、キャッシュ |
| `KafkaConsumer<T,K>` | 型安全メッセージ消費 | ストリーミング対応 |

#### **Error Handling**
```csharp
// エラー処理チェーン例
eventSet
  .OnError(ErrorAction.Retry)    // エラー時リトライ
  .Map(order => ProcessOrder(order))  // 業務ロジック
  .WithRetry(3, TimeSpan.FromSeconds(1)); // リトライ設定
```

**設計特徴**:
- ✅ **型安全**: `IKafkaProducer<T>`、`IKafkaConsumer<T,K>`
- ✅ **Pool削除**: 直接管理でシンプル化
- ✅ **エラー回復**: Circuit Breaker、Exponential Backoff

---

### 🔄 **Serialization Layer** (`src/Serialization/`)
> **責務**: Avroスキーマ生成・登録、シリアライゼーション

#### **Schema Management**
| クラス | 責務 |
|--------|------|
| `AvroSchemaRegistrationService` | 全スキーマ一括登録 |
| `AvroSchemaBuilder` | 型からスキーマ生成 |
| `UnifiedSchemaGenerator` | Key/Value統合生成 |
| `AvroSerializationManager<T>` | 型安全シリアライザ管理 |

#### **Cache & Performance**
| クラス | 責務 |
|--------|------|
| `AvroSerializerCache` | シリアライザキャッシュ |
| `AvroSchemaVersionManager` | スキーマバージョン管理 |

**Key Features**:
- 🏃 **自動生成**: POCO → Avroスキーマ自動変換
- 📋 **属性対応**: `[KafkaIgnore]`, `[AvroTimestamp]`
- 💾 **キャッシュ**: パフォーマンス最適化
- 🔄 **バージョニング**: 互換性チェック

---

### 🔍 **Query Layer** (`src/Query/`)
> **責務**: LINQ→KSQL変換、クエリ実行パイプライン

#### **Pipeline** (`src/Query/Pipeline/`)
| クラス | 責務 |
|--------|------|
| `QueryExecutionPipeline` | LINQ式→派生オブジェクト作成→クエリ実行 |
| `DerivedObjectManager` | CREATE STREAM/TABLE AS管理 |
| `DDLQueryGenerator` | CREATE文生成 |
| `DMLQueryGenerator` | SELECT文生成 |
| `StreamTableAnalyzer` | Stream/Table型推論 |

#### **Builders** (`src/Query/Builders/`)
| Builder | 生成するKSQL |
|---------|--------------|
| `SelectBuilder` | `WHERE 条件` |
| `ProjectionBuilder` | `SELECT 列1, 列2` |
| `GroupByBuilder` | `GROUP BY キー` |
| `JoinBuilder` | `JOIN テーブル ON 条件` |
| `WindowBuilder` | `WINDOW TUMBLING(...)` |

**実行例**:
```csharp
// LINQ
orders.Where(o => o.Amount > 1000)
      .Select(o => new { o.CustomerId, o.Amount })
      .ToListAsync();

// 生成されるKSQL
CREATE STREAM orders_stream_123 AS 
SELECT CustomerId, Amount 
FROM orders 
WHERE Amount > 1000;

SELECT * FROM orders_stream_123;
```

---

### 🗄️ **StateStore Layer** (`src/StateStore/`)
> **責務**: ローカル状態管理、ウィンドウ処理、KTable同期

#### **Core**
| クラス | 責務 |
|--------|------|
| `StateStoreManager` | ストア生成・管理 |
| `RocksDbStateStore<K,V>` | RocksDB永続化 |
| `WindowedEntitySet<T>` | ウィンドウ付きEntitySet |

#### **Integration**
| クラス | 責務 |
|--------|------|
| `TopicStateStoreBinding<T>` | Topic↔StateStore双方向同期 |
| `StateStoreBindingManager` | バインディング管理・ヘルスチェック |
| `ReadyStateMonitor` | Lag監視・Ready状態判定 |

**Ready State監視**:
```csharp
var binding = await manager.CreateBindingAsync(stateStore, consumer, entityModel);

// Ready状態まで待機
await binding.WaitUntilReadyAsync(TimeSpan.FromMinutes(5));

// Lag情報取得
var info = binding.GetReadyStateInfo();
Console.WriteLine($"Lag: {info.TotalLag}, Ready: {info.IsReady}");
```

---

### 🪟 **Window Finalization** (`src/Window/`)
> **責務**: ウィンドウ確定処理、確定足生成

| クラス | 責務 |
|--------|------|
| `WindowFinalizationManager` | 確定処理スケジューリング |
| `WindowProcessor<T>` | エンティティ別ウィンドウ処理 |
| `WindowFinalConsumer` | 確定足消費・RocksDB保存 |

**確定足フロー**:
```
Raw Events → Window Buffer → Grace Period → Finalization → Final Topic
    ↓              ↓             ↓              ↓            ↓
  orders      orders_5min    +3sec wait    Aggregate    orders_final
```

---

## 🔄 データフロー概要

### 📤 **Producer側フロー**
```
POCO Entity → AvroSerializer → KafkaProducer → Kafka Topic
     ↑              ↑               ↑             ↑
EntityModel   SchemaRegistry   ProducerManager  Bootstrap
```

### 📥 **Consumer側フロー**  
```
Kafka Topic → KafkaConsumer → AvroDeserializer → POCO Entity
     ↑              ↑               ↑               ↑
Subscription   ConsumerManager  SchemaRegistry   Business Logic
```

### 🔍 **Query側フロー**
```
LINQ Expression → QueryPipeline → DDL Generation → ksqlDB → Results
       ↑               ↑              ↑            ↑         ↑
   Where/Select   StreamAnalyzer   CREATE STREAM   SQL      List<T>
```

---

## ⚡ 設計原則

### 🎯 **Core Design Principles**
- **型安全性**: 全てのAPIで型安全性を保証
- **Fail-Fast**: 初期化時エラーは即座に終了
- **自動化**: スキーマ登録・DDL生成を自動化
- **一方向依存**: Core ← Infrastructure の依存方向

### 🔧 **実装方針**
- **Pool削除**: 複雑性削減、直接管理
- **属性駆動**: POCO + Attribute でメタデータ定義
- **EF風API**: 親しみやすいDbContext風インターフェース
- **レイヤー分離**: 各層の責務明確化

### 📊 **パフォーマンス**
- **キャッシュ活用**: Serializer・Schema・Store全てでキャッシュ
- **非同期処理**: すべてのI/O操作を非同期化
- **リソース管理**: IDisposable実装、適切なリソース解放

---

## 🚀 使用例

### 基本的な送受信
```csharp
// Context作成
var context = new MyKafkaContext();

// 送信
await context.Orders.AddAsync(new Order { Id = 1, Amount = 5000 });

// 受信
await foreach (var order in context.Orders)
{
    Console.WriteLine($"Order: {order.Id}, Amount: {order.Amount}");
}
```

### LINQ クエリ
```csharp
// 高額注文のみフィルタ
var highValueOrders = await context.Orders
    .Where(o => o.Amount > 10000)
    .Select(o => new { o.CustomerId, o.Amount })
    .ToListAsync();
```

### ウィンドウ集約
```csharp
// 5分間ウィンドウでの集約
var salesSummary = await context.Orders
    .Window(5)  // 5分ウィンドウ
    .GroupByAggregate(
        o => o.CustomerId,
        g => new { 
            CustomerId = g.Key, 
            TotalAmount = g.Sum(o => o.Amount),
            OrderCount = g.Count()
        }
    )
    .ToListAsync();
```

---

## 📚 関連ドキュメント

- [Getting Started](../README.md) - 基本的な使い方
- [Configuration Guide](configuration.md) - 設定詳細
- [Query Examples](query_examples.md) - LINQ例集
- [Error Handling](error_handling.md) - エラー処理
- [Performance Tips](performance.md) - パフォーマンス最適化