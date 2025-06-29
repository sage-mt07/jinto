# Advanced Rules（詳細設計と運用ルール）

## 1. 本ドキュメントの位置付け

本ドキュメントは「getting-started.md」に記載された設計原則および構成ルールを前提とし、Kafka.Ksql.Linq OSSの**実装詳細・高度な設計思想・内部処理の挙動**を明文化するものです。

DSLや属性の基本的な使い方、アーキテクチャの理解を終えた上級開発者・運用担当者が、さらに深く制御や拡張を行うための参照資料として機能します。

---

## 2. クラス設計と可視性ポリシー

### 2.1 internal/public の役割整理

- APIとして外部に公開すべき型・拡張ポイント：`public`
- DSL内部の処理ロジック・変換パイプライン・State管理クラスなど：`internal`
- テストは公開インターフェース経由で実施、具象クラス直アクセス禁止

### 2.2 拡張ポイント

- `.OnError()` `.WithRetry()` `.WithManualCommit()` は `IQueryable` 拡張で構成
- Window関連のDSLは `.Window(x)` 拡張として `IQueryable<POCO>` に統合

---

## 3. 型変換とスキーマ登録戦略（Avro連携）

### 3.1 POCO → Avro スキーマ変換

- POCOに付与された属性（[DecimalPrecision], [DateTimeFormat], [Key] 等）を読み取り、Avroスキーマを動的生成
- `SchemaRegistry.AutoRegisterSchemas = true` の場合、Kafka初回送信時に自動登録

### 3.2 変換時のマッピング規則

| POCO型                          | Avro型                                 | 備考             |
| ------------------------------ | ------------------------------------- | -------------- |
| `decimal` + [DecimalPrecision] | `bytes` + logicalType=decimal         | 精度・スケール付きで定義   |
| `DateTime`, `DateTimeOffset`   | `long` + logicalType=timestamp-millis | UTCに変換         |
| `string`, `Guid`               | `string`                              | Guidは文字列化      |
| `byte[]`                       | `bytes`                               | Avroのbinaryに対応 |

### 3.3 スキーマレジストリの運用

- CI/CDパイプラインに統合し、スキーマ互換チェックをビルド時に実行
- `FORWARD` / `BACKWARD` / `FULL` の互換モードは明示指定
- 登録失敗時のフィードバックは詳細ログ出力（--verbose）で確認可能

---

## 4. Finalトピック生成とWindow処理のタイマー駆動

### 4.2 Finalトピックの命名と作成およびRocksDBとの関係

- `{EntityName}_{Window}min_final` を基本命名規則とする

- 各Windowごとに1つのStateStore（RocksDBインスタンス）が構築され、ウィンドウ確定タイミングでその内容がFinalトピックに出力される

- StateStoreはアプリ内の状態保持に使われ、集計済みの結果は `WindowFinalizationManager` によりローカル→Kafkaへ出力される

- Final用のRocksDBは `rocksdb/final/{Entity}_{Window}min_Store/` に作成される（通常のStateStoreとは別ディレクトリ）

- キャッシュ（EnableCache）がONの場合、最新状態をメモリ保持するためファイルサイズは減少傾向にあるが、OFFの場合は全状態を永続化するためファイルサイズが大きくなる傾向がある

- アプリ起動時に `EnsureWindowFinalTopicsExistAsync` により全トピック事前作成

- `{EntityName}_{Window}min_final` を基本命名規則とする

- 各Windowごとに1つのStateStore（RocksDBインスタンス）が構築され、ウィンドウ確定タイミングでその内容がFinalトピックに出力される

- StateStoreはアプリ内の状態保持に使われ、集計済みの結果は `WindowFinalizationManager` によりローカル→Kafkaへ出力される

- アプリ起動時に `EnsureWindowFinalTopicsExistAsync` により全トピック事前作成

### 4.3 Final出力の特徴とGap対応

- TickがなくてもWindow終了時刻に自動出力されることで、“Gap”（空白期間）を補完し、時系列の連続性を保つ
- Gapとは、トピックにイベントが流れない時間帯においてもウィンドウ処理が時間軸上で欠損しないようにするための、明示的な“空の足”データを指します
- Finalデータは `WindowedResult` POCOをAvro化し、別トピックに出力
- 例：`orders_5min_final` トピックに `OrderCandle` 出力

---

## 5. DLQ設計とエラーハンドリング

### 5.1 DLQの設計思想

- すべてのエラーは `ErrorAction.DLQ` により集約的にDLQトピックへ送信可能
- DLQトピックは1系統（例：`system.dlq`）を共通で使用
- メッセージには `sourceTopic`, `errorCode`, `exception` などのメタ情報付与

### 5.2 DLQ構成例

```json
"DlqConfiguration": {
  "RetentionMs": 5000,
  "NumPartitions": 3,
  "ReplicationFactor": 2,
  "EnableAutoCreation": true
}
```

> ※ RetentionMs のデフォルト値は 5000（5秒）です。これでは短すぎるケースも多いため、必要に応じて明示的に設定を行ってください。

---

## 6. ストリーム/テーブルの自動判定と明示オーバーライド

- `GroupBy`, `Aggregate`, `Window` を含むLINQ式はテーブルと判定
- `AsStream()`, `AsTable()` は判定ロジックを上書き
- 判定結果は `.Explain()` や `ILogger` に出力可能（開発支援）

---

## 7. CI/CDおよび検証モード

- `ValidationMode: Strict` によりDSL構文とPOCO定義を初期化時に厳格チェック
- CI環境では構文検証モードを利用し、Kafka未接続状態でDSL整合性確認
- 初期化失敗はビルド失敗とみなす

---

## 8. デフォルト構成と運用ルール

### 8.1 RocksDBの配置と構成

- StateStoreはローカルファイルとして `rocksdb/{Entity}_{Window}min_Store/` に配置される
- アプリケーション実行ディレクトリ内に階層構造で保存（実体はleveldb/rocksdbによる）
- コンパクションポリシー：デフォルトで `compact` モードが有効

### 8.2 Kafka関連のデフォルト設定

- パーティション数：設定がなければ `1`
- ReplicationFactor：設定がなければ `1`
- GroupId：`KsqlDsl_{EntityName}` が自動割当（手動設定可能）
- AutoOffsetReset：`Latest` がデフォルト

---

## 9. 用語定義と今後の拡張予定

- `WindowFinalizationManager`: Window終了時刻に自動出力を行う内部クラス
- `IManualCommitMessage<T>`: 手動コミット対応メッセージ
- `WithRetry`, `OnError`: DSLの拡張ポイント

今後追加予定：

- RetryBackoff, DeadLetterRetry, Topic間リレーション設計
- クエリのExplain/Previewモード

