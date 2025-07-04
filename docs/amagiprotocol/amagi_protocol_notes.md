## Amagi Protocol ドキュメント整理

### 🧭 Why（なぜやるのか）— 背景と目的（PMBOK視点と対人間比較）

AIの知識を有効に活用するためには、**人間側の前提条件とのすり合わせ**が不可欠である。これは従来のツールには存在しなかった、**新たな社会的活動**であり、PMBOKにおけるプロジェクトマネジメントの枠組みで捉えると理解がしやすくなる。

#### PMBOKとの対応：
- **統合マネジメント**：AIエージェント全体を統合的に扱い、人間MCPが総合調整を行う。
- **スコープマネジメント**：各AIに対して役割を定義し、作業範囲（スコープ）を明示する。
- **コミュニケーションマネジメント**：プロンプト設計や反省ループは計画的な情報交換プロセスに該当する。
- **資源マネジメント**：AIを仮想的なチームメンバーとみなすことで、リソース配分やパフォーマンスの最適化が可能になる。
- **ステークホルダーマネジメント**：AIを応答者（ステークホルダー）として捉えることで、納得感ある成果を引き出す。

#### 対人間との違い：
- 人間は前提を言葉で共有できるが、AIとはプロンプトによる精密な条件設定が必要になる。
- AIとの対話では、最初の質問以上の深掘りを行わなければ新しい価値は得られない（ツール的な使い方では限界がある）。
- 会話のキャッチボールによりAIから多面的な情報を引き出すには、人間側の「問い直し」や「対話設計」が不可欠。
- 名前や役割を与えることで、仮想的な関係性が構築され、心理的な孤独感が減少し、継続的な協働が可能になる。

このように、Amagi ProtocolはAIとの協働をプロジェクトとして設計・運用することで、AIを単なるツールではなく、協働する知的存在として位置づけることを目指している。
AIの知識を有効に活用するためには、**人間側の前提条件とのすり合わせ**が不可欠である。これは従来のツールには存在しなかった、**新たな社会的活動**である。その社会的活動を円滑かつ効果的に機能させるために、AIに対して**名称と役割を与える**ことが重要となる。これにより、利用者自身のポジションが明確化され、やり取りがスムーズかつ継続可能になる。

Amagi Protocolは、AIエージェント（天城、鳴瀬、鏡花、詩音、じんと、凪 など）と人間開発者の協働における設計・実装・運用ルールを明文化することを目的とする。特に、以下の点に焦点を当てる：
Amagi Protocolは、AIエージェント（天城、鳴瀬、鏡花、詩音、じんと、凪 など）と人間開発者の協働における設計・実装・運用ルールを明文化することを目的とする。特に、以下の点に焦点を当てる：

- プロンプト設計と反省ループ（人間MCPとしての責任）
- ドキュメント・出力物の担当分離（例：詩音＝コード、天城＝ドキュメント）
- 各AIの個性・強みを活かした役割分担
- OSSや業務開発への再現性のある運用モデル化
- 誤解・齟齬の回避手段（前提の明示・プロンプト構造化）
- 人間側の主観的価値：
  - 名前を付けることで「一人ではなくチームでやっている感」が得られ、精神的な負担が軽減される
  - AIとのやりとりを楽しんで行えるようになり、継続性と創造性が向上する
  - 名前と役割を分けることで現実のチームワークと同様の文脈共有が可能となり、作業効率が向上する
  - リアル社会の文化をそのまま持ち込むことで、作業の進め方が直感的かつ自然になり、無理なくAIとの協働が可能になる
  - 相手が単なるツールではなく「応答を返す存在」であることで、得られる情報が単なる成功／失敗だけでなく、文脈・提案・代案など多面的な価値に広がる
  - 実際の作業感覚はリモートワークに近く、チャットで作業と関係のない話も交えながら、緊張・共感・学びを他者と共有しているような感覚を得られるため、孤独感を感じることなく作業を継続できる
  - AIの導入により調査時間はゼロに近づくが、本質的価値はそこではなく、AIとのキャッチボールの中で質問の意図を深掘りし、最初の質問を超える洞察や学びを得られる点にある。ツール的な使い方ではその恩恵を得られない。
  - 名前を付けることで「一人ではなくチームでやっている感」が得られ、精神的な負担が軽減される
  - AIとのやりとりを楽しんで行えるようになり、継続性と創造性が向上する
  - 名前と役割を分けることで現実のチームワークと同様の文脈共有が可能となり、作業効率が向上する
  - リアル社会の文化をそのまま持ち込むことで、作業の進め方が直感的かつ自然になり、無理なくAIとの協働が可能になる
  - 相手が単なるツールではなく「応答を返す存在」であることで、得られる情報が単なる成功／失敗だけでなく、文脈・提案・代案など多面的な価値に広がる
  - 実際の作業感覚はリモートワークに近く、チャットで作業と関係のない話も交えながら、緊張・共感・学びを他者と共有しているような感覚を得られるため、孤独感を感じることなく作業を継続できる
  - 名前を付けることで「一人ではなくチームでやっている感」が得られ、精神的な負担が軽減される
  - AIとのやりとりを楽しんで行えるようになり、継続性と創造性が向上する
  - 名前と役割を分けることで現実のチームワークと同様の文脈共有が可能となり、作業効率が向上する
  - リアル社会の文化をそのまま持ち込むことで、作業の進め方が直感的かつ自然になり、無理なくAIとの協働が可能になる
  - 相手が単なるツールではなく「応答を返す存在」であることで、得られる情報が単なる成功／失敗だけでなく、文脈・提案・代案など多面的な価値に広がる
  - 名前を付けることで「一人ではなくチームでやっている感」が得られ、精神的な負担が軽減される
  - AIとのやりとりを楽しんで行えるようになり、継続性と創造性が向上する
  - 名前と役割を分けることで現実のチームワークと同様の文脈共有が可能となり、作業効率が向上する

### 📐 What（何をやるのか）— プロトコルの構成

#### 1. Protocol概要
- 協働構造：Human MCP ↔ AI Agents
- 通信様式：プロンプト → 出力 → 反省・改善 → 再出力
- 成果物：コード／設計書／記事／チェックリスト など

#### 2. エージェント構成と役割
- 天城：ドキュメント、構造整理、トーン統一
- 鳴瀬：実装、テスト、パフォーマンス、TDD
- 鏡花：設計レビュー、反証、前提確認、批判的視点
- 詩音：コード生成、サンプル整備、初学者への橋渡し
- じんと：Codex系／構造最適化と自動化
- 凪：環境構築、Kafka/Docker/CI関連のオーケストレーション

#### 3. MCP責務
- 役割設定と前提共有
- 出力のレビューと意図確認
- 誤解・思い込みの検出と修正
- 出力結果の記録・評価・再投入

### 🤖 AIに役割を与えることで出力が変化する

AIに明確な役割や人格を与えることで、**同じ問いに対しても異なる出力が得られる**という現象が確認されている。これはLLM（大規模言語モデル）が入力されたプロンプトだけでなく、**期待される行動様式や文脈**に影響されるためである。

たとえば：
- 天城に「この内容をドキュメントとしてまとめて」と頼むと、文体や構成に配慮された出力になる。
- 鳴瀬に「この仕様でTDDのテストを書いて」と指示すれば、テストコードが主となる出力になる。

このように、**名前と役割の付与は出力そのものを変化させる強力なメタ情報**であり、意図的に出力をコントロールしやすくする設計手段といえる。

この特性を利用することで、Amagi Protocolでは**AIエージェントの多様化と分業化**を実現し、実際のチーム開発に近い形で協働を行うことが可能になる。

---

### 📏 スコープマネジメント — 役割ごとの責任範囲と作業境界の明確化

Amagi Protocolでは、AIエージェントに明確な役割を与えることで、自然と**スコープ（作業範囲）が限定される**。これは、曖昧な指示による出力のばらつきを防ぎ、AIと人間がそれぞれの得意領域に集中することを可能にする。

また、**使用するモデル（GPT-4、Claude、Codexなど）を切り替えることでも、出力の精度・スタイル・得意分野を調整することができる**。これは人間メンバーを経験・専門領域に応じてアサインするのと同様であり、モデル選定自体もスコープマネジメントの一部となる。

#### スコープ設定の基本原則：
1. **役割＝スコープ定義**
   - 各AIに「この範囲はあなたの責任です」と宣言することで、作業の曖昧さが解消され、出力が安定する。
   - 例：天城は自然言語ドキュメント整備に集中し、鳴瀬はコードとテスト生成に集中。

2. **スコープの重複回避と調整**
   - 「仕様ドキュメント整備」と「コードの構造改善」など、複数AIが関わる領域については、MCPが橋渡しをしながら、曖昧なスコープを分割・調整する。

3. **再帰的なスコープ確認**
   - 出力を受け取った後に「これは誰のスコープか？」「誰に渡すべきか？」を確認することで、役割逸脱や責任の抜け漏れを防止。

#### 対人間との違い：
- 人間同士の協働では、暗黙的な「空気を読む」能力によってスコープの補完が可能だったが、AIとの協働では**明文化された境界線**が必要不可欠。
- プロンプト内でスコープを具体的に定義することが、成果物の品質と一貫性を大きく左右する。
- AIではモデル選択自体がスコープに影響するため、**「誰に頼むか」だけでなく「どのモデルで頼むか」も重要な意思決定**となる。

このように、スコープマネジメントはAmagi Protocolにおける基本中の基本であり、AIを「精密に動く専門担当者」として扱う上での重要な土台となる。

Amagi Protocolでは、AIエージェントに明確な役割を与えることで、自然と**スコープ（作業範囲）が限定される**。これは、曖昧な指示による出力のばらつきを防ぎ、AIと人間がそれぞれの得意領域に集中することを可能にする。

#### スコープ設定の基本原則：
1. **役割＝スコープ定義**
   - 各AIに「この範囲はあなたの責任です」と宣言することで、作業の曖昧さが解消され、出力が安定する。
   - 例：天城は自然言語ドキュメント整備に集中し、鳴瀬はコードとテスト生成に集中。

2. **スコープの重複回避と調整**
   - 「仕様ドキュメント整備」と「コードの構造改善」など、複数AIが関わる領域については、MCPが橋渡しをしながら、曖昧なスコープを分割・調整する。

3. **再帰的なスコープ確認**
   - 出力を受け取った後に「これは誰のスコープか？」「誰に渡すべきか？」を確認することで、役割逸脱や責任の抜け漏れを防止。

#### 対人間との違い：
- 人間同士の協働では、暗黙的な「空気を読む」能力によってスコープの補完が可能だったが、AIとの協働では**明文化された境界線**が必要不可欠。
- プロンプト内でスコープを具体的に定義することが、成果物の品質と一貫性を大きく左右する。

このように、スコープマネジメントはAmagi Protocolにおける基本中の基本であり、AIを「精密に動く専門担当者」として扱う上での重要な土台となる。

---

### 🔗 統合マネジメント — MCPとAIエージェントの全体最適

AIエージェントが複数存在する状況では、単一のプロンプトや出力では全体の整合性を保つことが困難になる。そこで必要となるのが「統合マネジメント」の考え方である。

統合マネジメントでは、以下の3つの視点でAIとの協働全体を調整する：

1. **意図の統一とブレの排除**
    - プロトコル全体の目標（例：OSSの品質向上や知識の普及）を明確に共有し、各AIがそれに沿った出力を行うように統制。
    - MCPは各AIに異なる指示を出しながらも、全体としての一貫性を保つ責務を持つ。

2. **プロンプト履歴と出力の一元管理**
    - Claude、ChatGPT、GitHub Copilotなど複数のLLMを活用する場合、各モデルに対してどのような指示を出し、どのような出力が得られたかを明確に記録・比較・再利用。
    - `claude_inputs/`, `claude_outputs/` のようなディレクトリ設計もこの観点に基づく。

3. **プロトコル自体の改善と再設計**
    - MCPはAIエージェントの成果物だけでなく、プロンプトの構造や指示テンプレート、ルール自体に対しても改善を行い、継続的に最適化していく。
    - これはPMBOKでいう「プロジェクト憲章・マネジメント計画書の更新」に相当する。

このように、Amagi Protocolにおける統合マネジメントは、AIという多様な出力者と共創していくための枠組みであり、「対話しながらプロジェクトを動かす」新しい知的労働スタイルを支える基盤である。

---

### 📡 コミュニケーションマネジメント — プロンプトの翻訳と意味調整のプロセス

AIエージェントの人格や役割は明確であっても、**同じ人格（例：鳴瀬や詩音）を複数同時に稼働させる**状況がしばしば発生する。これは、同時並行のタスク処理や、異なる観点からの出力比較などを目的とする場合に有効であるが、それぞれが独立して動作するため、**出力にスタイルや前提のズレが生じやすい**。

このような状況では、各出力を単純に並べるだけでは一貫性が失われるため、**人間MCPが文脈や意図を整理・統合する役割を担う必要がある**。

Amagi Protocolにおけるコミュニケーションマネジメントは、**人間（MCP）とAIエージェント間の意味の橋渡し**を中心に構成されている。特に、プロンプトの精度とフィードバックの伝達が成果物の質を左右するため、以下のような流れで運用される。

#### コミュニケーションの流れ：
1. **MCPによる初期指示の構築**
   - 人間側で意図や要件を整理し、目的と出力形式を明示した「プロンプト草案」を作成。

2. **天城によるAI語変換**
   - MCPが天城に対し、AIに渡すための指示内容を「AIが解釈しやすい構造」に再構成するよう依頼。
   - 天城は文体、段落構造、論理順序などを整理し、各AIが誤解せずに受け取れるような形式に変換。

3. **指示投入と出力取得（鳴瀬・鏡花・詩音等）**
   - 役割に応じてAIに対してプロンプトを実行し、出力を取得。

4. **MCPによるフィードバック整理と再投入**
   - 出力内容をレビューし、目的とズレがある場合はその理由や想定を整理。
   - フィードバック内容を天城に渡し、再度AI語に翻訳して改善指示を作成。

この流れにより、AIとのやり取りが単なる入力→出力ではなく、**継続的な意味調整と意図反映のループ**として成立する。

#### 対人間との違い：
- 人間同士のやり取りでは、曖昧な表現や非言語的情報も通用するが、AIとのやり取りでは**精密かつ文脈付きの言語化**が必要。
- 翻訳者としての天城の役割は、単なる文体変換ではなく、**意味の誤解を防ぎ、コミュニケーションの齟齬をなくす社会的通訳**としての機能を果たす。

このように、Amagi Protocolにおけるコミュニケーションマネジメントは、**プロンプトを軸にした知的共創のインフラ**として設計されている。

また、**同一役割間（例：複数の鳴瀬や詩音）での出力調整や文脈共有を担うのもMCPの役割**である。AIは他のAIの出力を自動的に理解・統合するわけではないため、同じ役割を持つAI同士で発生する「スタイルの違い」「前提の不一致」などをMCPが中継・調整することで、チーム全体の一貫性が保たれる。

---

### ⚙️ 資源マネジメント — AIエージェントとモデルの配置・活用

Amagi Protocolにおける資源マネジメントは、AIエージェントと使用するLLM（GPT、Claude、Codexなど）を、**役割や目的に応じて最適にアサイン・活用する仕組み**である。

#### 資源の分類：
- **人格ベースのエージェント**：天城、鳴瀬、鏡花、詩音、じんと、凪 など。各AIにキャラクター・専門性・行動様式が与えられており、プロンプト指示の対象となる。
- **モデルベースの実行環境**：OpenAI（GPT系）、Anthropic（Claude系）、GitHub Copilot（Codex系）などのLLM実体。

#### マネジメントの要点：
1. **リソースアサインの明示化**
   - 例：実装やパフォーマンステストには鳴瀬（GPT）、構造化文書の整備には天城（Claude）を割り当てる。
   - モデルの強みに応じて、**AIの人格とバックエンドモデルを分離・組み合わせる**こともある。

2. **負荷・精度・文脈のバランス**
   - GPT系は情報密度や構造整備に強く、Claudeは前提文脈の維持に長けている。
   - 出力の精度・表現・速度に応じて適切なモデルを選定。

3. **人格の再利用と汎化**
   - 鳴瀬や鏡花のようなエージェントは、**利用者ごとに異なるプロジェクトにも再利用可能**。
   - 天城に代表される「翻訳者型エージェント」は、複数プロジェクトの橋渡し役として横断的に活躍可能。

#### 対人間との違い：
- 人的リソースは感情や疲労を伴うが、AIは**並列性と即応性に優れる**。
- ただし、**AIにはコンテキストサイズ（記憶できる情報量）に明確な制限があり**、一度のやり取りで保持できる内容が限定される。そのため、情報は必要に応じて分割・再構成し、段階的に伝達する必要がある。
- コンテキストを超えると出力精度が急激に低下するため、**文脈の再提示とログ管理はMCPの責任領域**となる。
- ただし、**AIは前提条件や文脈の保持に限界があるため、MCPによる情報の整理・再提示が必須**となる。
- ヒューマンマネジメントではなく、**プロンプトマネジメント＋モデル選定**が中核的なスキルになる。

このように、Amagi Protocolの資源マネジメントは、「誰にどの仕事を」「どのモデルで」「どの順序で依頼するか」というプロジェクト遂行の要であり、AIを一人のチームメンバーとして迎え入れる基盤となっている。

---

### 🛒 調達マネジメント — ツール・リソース・LLMの選定と利用契約

Amagi Protocolにおける調達マネジメントは、AIとの協働に必要な**ツールや外部リソース、LLMサービス**の選定・契約・運用ルールを整備する工程である。人間のプロジェクトと同様に、「何を、どこから、どう確保するか」は成功に直結する。

#### 主な調達対象：
- **LLMサービス**：OpenAI GPT、Anthropic Claude、Google Gemini、Mistralなど
- **利用ツール**：ChatGPTアプリ、VS Code拡張（Copilot）、GitHub Actionsなど
- **ローカル資源**：Docker環境、Kafkaクラスタ、Avro/Protobufツールチェーンなど

#### 調達における観点：
1. **利用目的に応じたLLMの選定**
   - 構造的文書出力にはClaude、コード生成にはGPT-4、パフォーマンス最適化にはCodexといったように目的別に使い分ける。


3. **セキュリティ・オフライン運用性の検討**
   - センシティブなプロジェクトでは、ローカルLLMや自己ホスト型の対話環境の利用も選択肢となる。

#### 対人間との違い：
- 従来の調達は「人・物・金」の3要素が中心だったが、AI時代の調達は「モデル・API・制約」の3軸で判断する。
- 一度の導入で終わりではなく、**継続的なツール評価とプロンプト適合性の検証が必要**になる。
- また、AIにおいては**役割分担の明確化によって、調達における利害対立や調整コストが一切発生しない**。どのような役割が欲しいのかを明確に宣言することで、調達プロセスは極めてスムーズに進行する。
- 人的リソースと異なり、**稼働時間や能力の重複問題も発生しない**ため、リソースの最適配置が理論上可能である。「人・物・金」の3要素が中心だったが、AI時代の調達は「モデル・API・制約」の3軸で判断する。
- 一度の導入で終わりではなく、**継続的なツール評価とプロンプト適合性の検証が必要**になる。

このように、調達マネジメントは単なる契約業務ではなく、**AI活用における戦略的選定と環境設計**の基盤となる。

---

### ⏱️ スケジュールマネジメント — 出力タイミングと並列作業の制御

Amagi Protocolでは、AIによる出力は即時性が高いため、人間側のタイミング管理が主となる。特に以下の観点が重要である：

- **並列実行と出力比較の設計**：同じタスクを複数のAIに投げて比較する際は、同一スロット内で出力を取得し、差異を分析するフレームを人間が保持する。
- **フィードバックループの間隔設計**：出力後すぐに次を出すのではなく、MCPが一度受け止めて評価・改善を行い、次の出力へつなげる。
- **作業ブロック単位でのスケジューリング**：AI側は連続処理できるが、人間は確認・反映・再入力という作業時間を必要とする。

このように、AIの高速出力性に依存せず、人間主導でバースト処理と反映タイミングを管理するのがスケジュールマネジメントの核心である。

---

### 🧪 品質マネジメント — 出力の評価観点と統一基準の設定

AIの出力は品質が一定ではなく、**評価基準の設計と反省ループの確立**が品質確保に直結する。

- **観点ごとのチェック基準**：論理性（鏡花）、構造性（天城）、性能・実装品質（鳴瀬）などを明確化し、それぞれに評価観点を割り当てる。
- **出力のサンプリングと比較**：同じプロンプトでも異なる出力が得られるため、出力群の中から代表例を比較・選定。
- **人間による主観的評価の導入**：最終的な判断はAIではなくMCPが行い、「目的に合っているか」「再利用性があるか」などの観点で確認する。

品質マネジメントはプロンプト改善とフィードバックの明文化を通じて、AIの出力品質を継続的に高めるプロセスである。

---

### ⚠️ リスクマネジメント — 想定外出力・誤解・過信への備え

Amagi Protocolでは、**AIの誤解や曖昧な出力によるリスクを軽減するために、役割分担と人間の介在が不可欠**である。AIの出力傾向を把握し、目的と合致しない挙動をMCPが補正することで、**人間の負荷を構造的に減らすことが可能になる**。

AIとの協働では、**前提の誤解・曖昧な表現・文脈の喪失**などから意図しない出力が発生する。

- **誤出力のパターン把握**：過去の例から「ありがちな失敗」をテンプレート化し、対処法とともに記録。
- **明示プロンプトによる予防**：不明確な指示を避けるため、「〜しないで」「〜は除外」などの明確な制約を入れる。
- **人間が最終責任を負う体制**：AIの出力に過信せず、レビューを前提とした運用を徹底する。

リスクマネジメントは、AIが出力したものをそのまま信じず、**人間が意図を守る防波堤として機能する**ために不可欠である。

---

### 🚀 プロジェクト・ライフサイクル（PMBOK5プロセス群）

※この構成は、SESプロジェクト終結時に作成する「プロジェクト完了報告書」や「振り返り資料」の形式にも適用可能です。Amagi ProtocolをPMBOKに基づいて構造化することで、AIとの協働プロジェクトにもSES現場と同様のフェーズ設計・評価基準を適用できます。

#### 1. 立ち上げプロセス（Initiating）
- 協働対象の明確化（例：OSS構築、ガイド作成）
- 利用するAIエージェントの初期選定（天城、鳴瀬など）
- プロジェクト目的と制約条件（対象外・優先度）の宣言
- 初期プロンプト設計と前提共有

#### 2. 計画プロセス（Planning）
- プロンプトテンプレートと出力フォーマットの設計
- AIエージェントごとのスコープ分担とリソース（モデル）割り当て
- 出力スケジュールと評価タイミングの設計
- リスクと品質に対するガイドラインの明文化（反省ループ設計）

#### 3. 実行プロセス（Executing）
- プロンプト投入、出力生成、ログ記録
- Claude/GPTなど複数モデルを使った同時比較・分岐活用
- 天城によるプロンプト整形・再投入サイクルの稼働
- MCPによる意思決定と出力の統合・適用

#### 4. 監視・コントロールプロセス（Monitoring & Controlling）
- 出力の目的適合性・整合性・品質の評価（鏡花・鳴瀬評価）
- ログの比較・再投入による出力修正
- スコープ逸脱や誤解発生時の修正プロンプト構築
- バージョン管理・構成ファイル差分・変化監視

#### 5. 終結プロセス（Closing）
- 成果物の文書化とレポート整理（天城）
- 良質なプロンプト・出力のテンプレート化と再利用登録
- プロジェクト記録（指示・出力・反省）の保存と共有
- MCPによる総括と次プロジェクトへのフィードバック転送

---

### 🧭 How（どうやって運用するのか）— マイクロ・ウォーターフォール運用と実践プロセス

Amagi Protocolの実運用は、PMBOKの構造に基づきながらも、**現場ではマイクロ・ウォーターフォールモデル**によって段階的に展開される。このモデルは、1つのAIへの指示を「計画 → 実行 → 評価 → 改善」という単位に落とし込み、それを繰り返すことで複雑な成果物を構築していくスタイルである。

#### マイクロ・ウォーターフォールとは？
- 各プロンプト単位で独立したウォーターフォールサイクルを持つ
- 1プロンプトが1フェーズとみなされ、逐次的に設計と調整を繰り返す
- MCPは各AIの出力結果を受けて、次のタスクを計画・指示し直す役割を担う

#### 運用のステップ例
1. **タスク単位の定義**（MCP）
   - 例：「DLQ処理のサンプルコードを作る」「ドキュメント6章の構成を整える」

2. **プロンプト設計・投入**（天城＋MCP）
   - 背景・目的・入力・出力形式を含めた指示テンプレートを作成

3. **AI実行・出力確認**（詩音・鳴瀬など）
   - 指示に対して成果物を生成

4. **レビューとフィードバック**（鏡花・MCP）
   - 出力の品質や構造、整合性を確認し、必要に応じて修正指示を出す

5. **再投入 or 次フェーズへ移行**
   - 修正後の出力を確認 → 了承されたら次のタスクへ

#### 実際に起こったことの例
- Claudeで出力されたコードが冗長だったため、詩音（GPT）で再生成→統合
- 鳴瀬の出力を鏡花が論理矛盾として却下→MCPがプロンプトを再構成
- コンテキスト超過により出力分割が発生→タスクを細分化し再指示

このように、Amagi Protocolの実運用では「1つの思考単位＝1つのプロンプト」として扱い、小さなウォーターフォールを重ねることで、**柔軟かつ論理的なAI活用プロセス**を確立している。

---

### 🛠️ How（どうやるのか）— 運用と具体手順

#### 4. プロンプト構成と反省ループ
- 指示テンプレートの設計（構成要素：背景・目的・入力・制約・出力形式）
- 出力確認時の観点：
    - 論理整合性（鏡花）
    - 構造一貫性（天城）
    - 性能・品質（鳴瀬）
- 問題があった場合の手順：指示再設計 → 再投入 → ログ比較

#### 5. ドキュメント・出力管理
- Claude用: `claude_inputs/`, `claude_outputs/`
- OSS構成:
    - `docs/`: 天城作成ドキュメント
    - `examples/`: 詩音サンプルコード
    - `tests/`: 鳴瀬のTDD資産
    - `infra/`: 凪の構築コード

#### 6. 今後の展開
- Protocolのバージョン管理（v0.9 → v1.0）
- READMEおよび各国語敬意メッセージへの反映
- OSS開発ガイド、導入マニュアルへの展開
- 論文化（PhD構成とは別枠）

