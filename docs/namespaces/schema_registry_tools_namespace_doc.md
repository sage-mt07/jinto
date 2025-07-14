# SchemaRegistryTools 詳細設計

## 🎯 責務・設計方針

### 主要責務
- **Readonly Entityメタ生成**: Schema Registryから取得したAvroスキーマを解析し、キー/バリュー型情報を含むMetaを生成
- **Mapping登録補助**: 生成したMetaを既存のMappingへ登録して、Consumer/Deserializer層が利用できるようにする

### 設計原則・制約
- **Query層から独立**: LINQ解析に依存せずスキーマ情報のみからMetaを構築する
- **単一責任**: Readonly時の特別処理は本namespaceに閉じ込め、他層に影響を与えない

### 他namespaceとの境界
- **Serialization**: `ISchemaRegistryClient` を受け取り、スキーマ取得のみを行う
- **Application**: 生成したMetaはApplication層のMappingに登録される想定

---

## 🏗️ 主要クラス構成

| ファイル | クラス | 責務 | 変更頻度 |
|---------|------|------|------|
| `SchemaRegistryMetaProvider.cs` | SchemaRegistryMetaProvider | スキーマ取得→Meta生成ユーティリティ | 🟡 |
| `SchemaRegistryMetaProvider.cs` | EntitySchemaMeta / SchemaField | メタ情報データ構造 | 🟢 |

---

## 🚀 運用フロー
1. Readonly属性を持つEntity型を指定して `SchemaRegistryMetaProvider.GetMetaFromSchemaRegistryAsync()` を呼び出す
2. Schema Registryから対象トピックの最新キー/バリュースキーマを取得
3. Avroスキーマを解析して `EntitySchemaMeta` を生成
4. Application層で `mapping.RegisterMeta(typeof(Log), meta);` のように登録
5. 以降のConsumer/DeserializerはMapping経由でこのMetaを参照

```csharp
var meta = await SchemaRegistryMetaProvider.GetMetaFromSchemaRegistryAsync(typeof(Log), registryClient);
mapping.RegisterMeta(typeof(Log), meta);
```

この流れを採用することで、Readonly EntityでもLINQ式解析を経ずに必要なメタ情報を取得できます。
