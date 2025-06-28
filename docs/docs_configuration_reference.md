## ⚙️ KsqlDsl appsettings.json 構成仕様

Kafka.Ksql.Linq では、`appsettings.json` を通じて柔軟なDSL設定が可能です。以下はその構成要素と意味です。

---

### 📐 基本構造

```json
{
  "KsqlDsl": {
    "ValidationMode": "Strict|Relaxed",
    "Common": { /* 共通設定 */ },
    "Topics": { /* トピック別設定 */ },
    "SchemaRegistry": { /* スキーマレジストリ設定 */ },
    "Entities": [ /* エンティティ／StateStore 設定 */ ],
    "DlqTopicName": "dead.letter.queue",
    "DlqConfiguration": { /* DLQ トピック設定 */ },
    "DeserializationErrorPolicy": "Skip|Retry|DLQ",
    "ReadFromFinalTopicByDefault": false
  }
}
```

---

### 🧱 2.1 Common（共通Kafka設定）

| 項目 | 説明 |
|------|------|
| `BootstrapServers` | Kafkaブローカーの接続先 |
| `ClientId` | 接続クライアント識別子 |
| `RequestTimeoutMs` | Kafka操作タイムアウト（ms） |
| `MetadataMaxAgeMs` | メタデータの最大有効期間（ms） |
| `SecurityProtocol` | `Plaintext` / `SaslPlaintext` など |
| `SaslMechanism` | 認証方式（例：`Plain`, `ScramSha256`） |
| `SaslUsername`, `SaslPassword` | SASL認証情報 |
| `SslCaLocation` | CA証明書ファイルパス |
| `SslCertificateLocation` | クライアント証明書ファイルパス |
| `SslKeyLocation` | 秘密鍵ファイルパス |
| `SslKeyPassword` | 秘密鍵パスワード |
| `AdditionalProperties` | 追加Kafka設定（key-value） |

```json
"Common": {
  "BootstrapServers": "localhost:9092",
  "ClientId": "ksql-dsl-client",
  "RequestTimeoutMs": 30000,
  "MetadataMaxAgeMs": 300000,
  "SecurityProtocol": "Plaintext",
  "SaslMechanism": "Plain",
  "SaslUsername": "user",
  "SaslPassword": "pass",
  "SslCaLocation": "/path/ca.pem",
  "SslCertificateLocation": "/path/cert.pem",
  "SslKeyLocation": "/path/key.pem",
  "SslKeyPassword": "secret",
  "AdditionalProperties": {}
}
```

---

### 📦 2.2 Topics（トピックごとの詳細設定）

```json
"Topics": {
  "my-topic": {
    "Producer": {
      "Acks": "All",
      "CompressionType": "Snappy",
      "EnableIdempotence": true,
      "MaxInFlightRequestsPerConnection": 1,
      "LingerMs": 5,
      "BatchSize": 16384,
      "DeliveryTimeoutMs": 120000,
      "RetryBackoffMs": 100,
      "Retries": 2147483647,
      "BufferMemory": 33554432,
      "Partitioner": null
    },
    "Consumer": {
      "GroupId": "my-group",
      "AutoOffsetReset": "Latest",
      "EnableAutoCommit": true,
      "AutoCommitIntervalMs": 5000,
      "SessionTimeoutMs": 30000,
      "HeartbeatIntervalMs": 3000,
      "MaxPollIntervalMs": 300000,
      "MaxPollRecords": 500,
      "FetchMinBytes": 1,
      "FetchMaxWaitMs": 500,
      "FetchMaxBytes": 52428800,
      "PartitionAssignmentStrategy": null,
      "IsolationLevel": "ReadUncommitted"
    },
    "Creation": {
      "NumPartitions": 1,
      "ReplicationFactor": 1,
      "Configs": {},
      "EnableAutoCreation": false
    }
  }
}
```

| Producer設定 | 説明 |
|------------------|------|
| `Acks` | 書き込み応答の強度設定（例：`All`, `1`） |
| `CompressionType` | 圧縮方式（`Snappy`, `Gzip`, `Lz4`など） |
| `EnableIdempotence` | 冪等性設定（重複防止） |
| `MaxInFlightRequestsPerConnection` | 同時送信要求上限 |
| `LingerMs` | バッチ送信待機時間（ms） |
| `BatchSize` | バッチ書き込み単位（byte） |
| `DeliveryTimeoutMs` | 配信タイムアウト（ms） |
| `RetryBackoffMs` | リトライ待機時間（ms） |
| `Retries` | 最大リトライ回数 |
| `BufferMemory` | 送信バッファサイズ（byte） |
| `Partitioner` | パーティショナー指定 |
| `AdditionalProperties` | 追加Producer設定 |

| Consumer設定 | 説明 |
|------------------|------|
| `GroupId` | コンシューマーグループID |
| `AutoOffsetReset` | `Latest` or `Earliest` |
| `EnableAutoCommit` | 自動コミット可否 |
| `AutoCommitIntervalMs` | 自動コミット間隔(ms) |
| `SessionTimeoutMs` | セッションタイムアウト(ms) |
| `HeartbeatIntervalMs` | ハートビート送信間隔(ms) |
| `MaxPollIntervalMs` | 最大ポーリング間隔(ms) |
| `MaxPollRecords` | 最大ポーリングレコード数 |
| `FetchMinBytes` | フェッチ最小バイト数 |
| `FetchMaxWaitMs` | フェッチ最大待機(ms) |
| `FetchMaxBytes` | フェッチ最大バイト数 |
| `PartitionAssignmentStrategy` | パーティション割当戦略 |
| `IsolationLevel` | アイソレーションレベル |
| `AdditionalProperties` | 追加Consumer設定 |

---

### 🧬 2.3 SchemaRegistry（スキーマレジストリ設定）

```json
"SchemaRegistry": {
  "Url": "http://localhost:8081",
  "MaxCachedSchemas": 1000,
  "RequestTimeoutMs": 30000,
  "BasicAuthUserInfo": "user:pass",
  "BasicAuthCredentialsSource": "UserInfo",
  "AutoRegisterSchemas": true,
  "LatestCacheTtlSecs": 300,
  "SslCaLocation": "/path/ca.pem",
  "SslKeystoreLocation": "/path/keystore.p12",
  "SslKeystorePassword": "secret",
  "SslKeyPassword": "secret",
  "AdditionalProperties": {}
}
```

| 項目 | 説明 |
|------|------|
| `Url` | スキーマレジストリURL |
| `MaxCachedSchemas` | クライアント側でキャッシュする最大スキーマ数 |
| `RequestTimeoutMs` | リクエストタイムアウト(ms) |
| `BasicAuthUserInfo` | Basic認証用クレデンシャル（形式：`user:pass`） |
| `BasicAuthCredentialsSource` | `UserInfo` or `SaslInherit` |
| `AutoRegisterSchemas` | スキーマを自動登録するかどうか |
| `LatestCacheTtlSecs` | 最新スキーマキャッシュTTL(sec) |
| `SslCaLocation` | CA証明書パス |
| `SslKeystoreLocation` | キーストア(PKCS#12)パス |
| `SslKeystorePassword` | キーストアパスワード |
| `SslKeyPassword` | 秘密鍵パスワード |
| `AdditionalProperties` | 追加設定 |

---

### 🏪 2.4 Entities（StateStore定義）

```json
"Entities": [
  {
    "Entity": "OrderEntity",
    "SourceTopic": "orders",
    "StoreType": "RocksDb",
    "EnableCache": true,
    "Windows": [5, 15, 60],
    "StoreName": "orders_store"
  }
]
```

| 項目 | 説明 |
|------|------|
| `Entity` | 対象POCOクラス名 |
| `SourceTopic` | 入力元となるKafkaトピック名 |
| `StoreType` | ストレージ方式（例：`RocksDb`） |
| `EnableCache` | キャッシュ有効化（bool） |
| `Windows` | タンブリングウィンドウサイズ（整数：分単位） |
| `StoreName` | StateStore名（省略時は自動生成） |

---

### 🛡️ 2.5 ValidationMode

| 値 | 説明 |
|-----|------|
| `Strict` | 起動時に全構成値を検証。欠落や不整合があるとエラー（推奨） |
| `Relaxed` | 柔軟に読み込む。テスト・一時構成に使用可 |

```json
"ValidationMode": "Strict"
```

---

### 💌 2.6 DLQ 設定

```json
"DlqTopicName": "dead.letter.queue",
"DlqConfiguration": {
  "RetentionMs": 5000,
  "NumPartitions": 1,
  "ReplicationFactor": 1,
  "EnableAutoCreation": true,
  "AdditionalConfigs": {
    "cleanup.policy": "delete"
  }
}
```

| 項目 | 説明 |
|------|------|
| `DlqTopicName` | DLQ用トピック名 |
| `RetentionMs` | メッセージ保持時間(ms) |
| `NumPartitions` | パーティション数 |
| `ReplicationFactor` | レプリケーション係数 |
| `EnableAutoCreation` | 自動作成を行うか |
| `AdditionalConfigs` | 追加トピック設定 |

---

### ⚙️ 2.7 その他オプション

| 項目 | 説明 |
|------|------|
| `DeserializationErrorPolicy` | `Skip` / `Retry` / `DLQ` のエラーハンドリング方針 |
| `ReadFromFinalTopicByDefault` | Finalトピックを既定で参照するか |


