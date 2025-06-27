# 実装カバレッジ状況

`task/implement-diff.txt` の未対応項目を基準に、現在の実装状況と担当タスクを一覧化する。

| 機能 | 状況 | 対応タスク名 | 備考 |
|---|---|---|---|
| OnError → Map → Retry | ✅ 実装済 | - | `EventSetErrorHandlingExtensions.cs` で確認済 |
| LATEST_BY_OFFSET / EARLIEST_BY_OFFSET | ✅ 実装済 | ksql_offset_aggregates | ProjectionBuilder, WindowAggregatedEntitySet 対応 |
| DLQ設定（ModelBuilder） | ⏳ 部分実装 | dlq_configuration_support | `TopicAttribute` 定義はある |
| HasTopic API | ❌ 未実装 | has_topic_api_extension | ModelBuilder拡張が未着手 |
| ManualCommit切替 | ⏳ 不完全 | manual_commit_extension | 分岐・Ack操作なし |
| char/shortサポート | ❌ 未実装 | special_type_handling | 警告・自動変換未実装 |
