# ✏️ カバレッジ進行状況

下表は `task/implement-diff.txt` の未実装または部分実装項目について、現在のカバレッジ状況を整理したものです。

| 機能カテゴリ | 機能 | 状況 | 対応タスク名 | 備考 |
| --- | --- | --- | --- | --- |
| Topics | Fluent APIによるトピック設定 | ❌ 未実装 | topic_fluent_api_extension | Partitions/Replication設定未対応 |
| Topics | パーティショニング戦略設定 | ❌ 未実装 | topic_fluent_api_extension | | 
| Topics | ISRの最小数設定 | ❌ 未実装 | topic_fluent_api_extension | |
| Streams | Window DSL機能 | ❌ 未実装 | window_dsl_feature | TumblingWindow等 |
| Streams | 購読モードの固定化制御 | ⏳ 部分実装 | subscription_mode_fixed | UseManualCommitの実行時切替未実装 |
| Tables | LATEST_BY_OFFSET / EARLIEST_BY_OFFSET | ✅ 実装済 | ksql_offset_aggregates | ProjectionBuilderで変換完了 |
| Tables | 複数ウィンドウ定義とアクセス | ❌ 未実装 | multi_window_access | |
| Tables | HasTopic()メソッド | ❌ 未実装 | has_topic_api_extension | |
| Tables | WindowStart / WindowEndプロパティ | ❌ 未実装 | window_start_end_support | |
| Query & Subscription | 手動コミット購読処理の型分岐 | ⏳ 不完全 | manual_commit_extension | Ack操作・型分岐不足 |
| Query & Subscription | 購読処理の完全実装 | ❌ 未実装 | manual_commit_extension | CommitAsync等未実装 |
| Query & Subscription | yield型ForEachAsyncでのtry-catch | ❌ 未実装 | foreach_trycatch_support | |
| Special Types | char型警告 | ❌ 未実装 | special_type_handling | 警告処理なし |
| Special Types | short型自動int変換 | ❌ 未実装 | special_type_handling | 変換処理不明確 |
| Error Handling | チェーン可能なエラー処理DSL | ⏳ 部分実装 | chainable_error_dsl | OnError→Map→Retryの連鎖未完成 |
| Error Handling | デシリアライズエラーポリシー | ❌ 未実装 | deserialization_error_policy | |
| Error Handling | ModelBuilderでのDLQ設定 | ⏳ 部分実装 | dlq_configuration_support | TopicAttributeには存在 |
