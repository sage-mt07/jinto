# API Reference (Draft)

この文書は `Kafka.Ksql.Linq` OSS における公開 DSL/API と主要コンポーネントの概要を整理したものです。今後の設計ドキュメントや実装コード、テストコードへの参照基盤として利用します。

## 既定値と実装状況の参照

- 既定値一覧は [defaults.md](defaults.md) を参照してください。
- 実装カバレッジ状況は [implement_status.md](implement_status.md) に記載されています。

## Context クラスとベースインタフェース

| API | 説明 | 対象レイヤ | 実装状態 |
|-----|------|------------|---------|
| `IKsqlContext` | コンテキスト操作の抽象インタフェース | Context | ✅ |
| `KafkaContextCore` | `IKsqlContext` 実装の基底クラス | Context | ✅ |
| `KsqlContext` | Kafka連携を統合した抽象コンテキスト | Context | ✅ |
| `KsqlContextBuilder` | `KsqlContextOptions` 構築用ビルダー | Application | ✅ |
| `KsqlContextOptions` | スキーマレジストリ等の設定保持 | Application | ✅ |
| `IEventSet<T>` | LINQ/Streaming操作の共通インタフェース | Stream/Table | ✅ |
| `IManualCommitMessage<T>` | 手動コミットメッセージ | Subscription | ✅ |

## LINQ 風 DSL 一覧

| DSL メソッド | 説明 | 戻り値型 | 対象レイヤ | 実装状態 |
|---------------|------|-----------|------------|---------|
| `.Where(predicate)` | 条件フィルタ | `IEventSet<T>` | Stream/Table | ✅ |
| `.Window(int \| TimeSpan)` | タイムウィンドウ指定 | `IQueryable<T>` | Stream | ✅ |
| `.GroupBy(...)` | グループ化および集約 | `IEventSet<IGrouping<TKey, T>>` | Stream/Table | ✅ |
| `.OnError(ErrorAction)` | エラー処理方針指定 | `EventSet<T>` | Stream | ✅ |
| `.WithRetry(int)` | リトライ設定 | `EventSet<T>` | Stream | ✅ |
| `.WithManualCommit()` | 手動コミットモード切替 | `IEntityBuilder<T>` | Subscription | ✅ |

- `ToList`/`ToListAsync` は Pull Query として実行されます【F:src/Query/Pipeline/DMLQueryGenerator.cs†L27-L34】。
- `WithManualCommit()` を指定しない `ForEachAsync()` は自動コミット動作となります【F:docs/manual_commit.md†L1-L23】。
- `OnError(ErrorAction.DLQ)` を指定すると DLQ トピックへ送信されます【F:docs/oss_design_combined.md†L580-L599】。

これらの戻り値型を把握することで、DSLチェーンにおける次の操作を判断しやすくなります。特に `OnError()` や `WithRetry()` は `EventSet<T>` を返すため、続けて `IEventSet` 系メソッドを利用できます。

## 属性 (Attribute) 定義

| 属性 | 役割 | 実装状態 |
|------|------|---------|
| `TopicAttribute` | トピック構成指定 | ✅ |
| `KeyAttribute` | キー項目指定 | ✅ |
| `KsqlTableAttribute` | テーブル情報指定 | ✅ |
| `AvroTimestampAttribute` | Avro タイムスタンプ列指定 | ✅ |
| `DecimalPrecisionAttribute` | Decimal 精度指定 | ✅ |
| `RetryAttribute` | (予定) リトライポリシー指定 | ⏳ |
| `KsqlColumnAttribute` | (予定) 列名マッピング | ⏳ |

`WithDeadLetterQueue()` は過去の設計で提案されましたが、現在は `OnError(ErrorAction.DLQ)` に置き換えられています。

## 構成オプションとビルダー

| API | 説明 | 実装状態 |
|-----|------|---------|
| `KsqlDslOptions` | DLQ 設定や ValidationMode など DSL 全体の構成を保持 | ✅ |
| `ModelBuilder` | POCO から `EntityModel` を構築するビルダー | ✅ |
| `KafkaAdminService` | DLQ トピック作成などの管理操作 | ✅ |

`KsqlDslOptions.DlqTopicName` は既定で `"dead.letter.queue"` です【F:src/Configuration/KsqlDslOptions.cs†L31-L34】。

## エラーハンドリング

| API / Enum | 説明 | 実装状態 |
|------------|------|---------|
| `ErrorAction` (Skip/Retry/DLQ) | エラー時の基本アクション | ✅ |
| `ErrorHandlingPolicy` | リトライ回数やカスタムハンドラ設定を保持 | ✅ |
| `ErrorHandlingExtensions` | `.OnError()` `.WithRetryWhen()` 等の拡張 | ✅ |
| `DlqProducer` / `DlqEnvelope` | DLQ 送信処理 | ✅ |
| `DlqTopicConfiguration` | DLQ トピックの保持期間等を指定 | ✅ |

## 状態監視・内部機構

| API | 説明 | 実装状態 |
|-----|------|---------|
| `ReadyStateMonitor` | トピック同期状態の監視 | ✅ |
| `StateStoreBinding` | Kafka トピックと StateStore の双方向バインディング | ✅ |
| `SchemaRegistryClient` | スキーマ管理クライアント | ✅ |

## 各 API の備考

- `IEventSet<T>.WithRetry()` の実装例は `EventSetErrorHandlingExtensions.cs` にあります【F:src/EventSetErrorHandlingExtensions.cs†L120-L156】。
- `OnError` の拡張は同ファイルで提供されています【F:src/EventSetErrorHandlingExtensions.cs†L14-L37】。
- 手動コミットの利用例は [manual_commit.md](manual_commit.md) を参照してください。

## 実装・設計整合性一覧

以下は現時点での主要機能の実装状態です（詳細は [implement_status.md](implement_status.md) 参照）。

| 機能 | 状況 | 備考 |
|------|------|------|
| OnError → Map → Retry | ✅ | `EventSetErrorHandlingExtensions.cs` で確認済 |
| DLQ設定（ModelBuilder） | ⏳ | `TopicAttribute` 定義はある |
| HasTopic API | ✅ | `EntityBuilderTopicExtensions` |
| ManualCommit切替 | ✅ | `ForEachAsync` 型分岐対応 |
| char/shortサポート | ✅ | int変換＆警告出力 |

