## 📚 ドキュメント構成ガイド

このOSSでは、利用者のレベルや目的に応じて複数のドキュメントを用意しています。

### 🧑‍🏫 初級〜中級者向け（Kafkaに不慣れな方）
| ドキュメント | 内容概要 |
|--|--|
| `docs/sqlserver-to-kafka-guide.md` | SQL Server経験者向け：Kafkaベースの開発導入ガイド |
| `docs/getting-started.md` | はじめての方向け：基本構成と動作確認手順（※作成中） |

### 🛠️ 上級開発者向け（DSL実装や拡張が目的の方）
| ドキュメント | 内容概要 |
|--|--|
| `docs/dev_guide.md` | OSSへの機能追加・実装フローと開発ルール |
| `docs/namespaces/*.md` | 各Namespace（Core / Messaging 等）の役割と構造 |
| `docs/manual_commit.md` | 明示的なコミット制御の設計と利用例 |

### 🏗️ アーキテクト・運用担当者向け（構造や制約を把握したい方）
| ドキュメント | 内容概要 |
|--|--|
| `docs/docs_advanced_rules.md` | 運用設計上の制約、設計判断の背景と意図 |
| `docs/docs_configuration_reference.md` | appsettings.json などの構成ファイルとマッピング解説 |
| `docs/architecture_overview.md` | 全体アーキテクチャ構造と各層の責務定義 |

### 🧊 設計思想・理論基盤（全レベル対象）
| ドキュメント | 内容概要 |
|--|--|
| `docs/oss_design_combined.md` | OSSの設計方針・仕様全体の確定版ドキュメント |
| `docs/architecture_principles.md` | 設計哲学・思想・命名規則（※作成予定） |

---

📎 **補足**：  
テスト進行状況 → `implement_status.md`  
DSL構文仕様 → `querybuilder_kyouka.md`  
API仕様 → `api_reference.md`, `api_public_methods.md`



## AI作業補助
このプロジェクトでは、AIエージェントがコード生成・テスト補助を行います。  
設計方針・命名ルール・出力形式などは [instructions.md](./instructions.md) を参照してください。

このOSSはAIと人間の協働により開発されています。以下のドキュメントを参照してください。
このプロジェクトは「AIの能力は使う人の知性に比例する」という信念のもと構築されています。

