# Kafka.Ksql.Linq OSSガイド

このOSSはAIと人間の協働により開発されています。以下のドキュメントを参照してください。

- [開発者ガイド（機能マップ・アーキテクチャ）](docs/dev_guide.md)
- [アーキテクチャ責務早見表](docs/arch_overview.md)
- [Query Builder品質観点表（鏡花視点）](docs/querybuilder_kyouka.md)
- [設計議論・廃止機能ログ（Amagi Protocol）](docs/design_decision_log.md)

## モジュール一覧（概要）
- [Core](src/Core): DSL全体の共通構造とモデル定義
- [Messaging](src/Messaging): Kafka入出力とDLQ制御
- [Query](src/Query): LINQ to KSQLの構文変換とDSL構築
- [StateStore](src/StateStore): KTable用RocksDB連携管理

## 関連資料
- [AIエージェントと分担](agents.md)
- [構成図と命名規則](docs/namespaces)
- [開発タスクと出力ログ](tasks/)

このプロジェクトは「AIの能力は使う人の知性に比例する」という信念のもと構築されています。
