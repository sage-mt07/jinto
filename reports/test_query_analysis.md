# JOIN関連クラス群のユニットテスト調査報告書

## 分析対象クラス概要

全11クラスがJOIN操作とLINQ式変換に特化した機能を提供している。主要な責務は以下の通り：

- **Entity Adapter**: 型変換とアダプター機能
- **JOIN処理**: 2-way/3-way JOIN操作のサポート
- **Expression Visitor**: LINQ式木の変換とパラメータ置換
- **Result Set**: JOIN結果の管理と実行

## クラス別テスト要件分析

### 1. EntitySetAdapter<T>
**場所**: `src/Query/Linq/JoinableEntitySet.cs` (行507-574)

**主要機能**:
- `IEntitySet<T>` → `IEntitySet<object>` の型変換
- 型安全性の確保とキャスト処理

**テストクエリ要件**:
```csharp
// 正常系
var typedEntitySet = mockEntitySet<Customer>();
var adapter = new EntitySetAdapter<Customer>(typedEntitySet);
await adapter.AddAsync(new Customer { Id = 1, Name = "Test" });
var results = await adapter.ToListAsync();

// 異常系
await adapter.AddAsync(new Order()); // ArgumentException
await adapter.AddAsync(null); // ArgumentNullException
```

### 2. JoinResult<TOuter, TInner>
**場所**: `src/Query/Linq/JoinableEntitySet.cs` (行77-148)

**主要機能**:
- 2-way JOIN結果の管理
- SELECT句による射影
- 3-way JOINへの拡張

**テストクエリ要件**:
```csharp
var joinResult = new JoinResult<Customer, Order>(
    customerEntitySet, orderEntitySet,
    c => c.Id, o => o.CustomerId);

// SELECT句テスト
var projected = joinResult.Select((c, o) => new { c.Name, o.Amount });

// 3-way JOIN拡張テスト
var threeWay = joinResult.Join(productEntitySet, 
    c => c.Id, p => p.CategoryId);
```

### 3. JoinResultEntitySet<T>
**場所**: `src/Query/Linq/JoinResultEntitySet.cs`

**主要機能**:
- JOIN操作の実行結果管理
- JoinBuilderとの連携
- 非同期enumeration

**テストクエリ要件**:
```csharp
var joinExpr = Expression.Call(/* JOIN expression */);
var resultSet = new JoinResultEntitySet<JoinResult>(
    context, entityModel, joinExpr, joinBuilder);

// 実行テスト
var results = await resultSet.ToListAsync();
Assert.NotNull(results);

// ForEachAsync テスト
await resultSet.ForEachAsync(async item => {
    // 処理検証
});

// 非同期enumeration
await foreach(var item in resultSet) {
    // アイテム検証
}
```

### 4. JoinableEntitySet<T>
**場所**: `src/Query/Linq/JoinableEntitySet.cs` (行13-76)

**主要機能**:
- IEntitySet<T>のJOIN機能拡張
- IJoinableEntitySet<T>インターフェース実装
- JOIN操作の開始点

**テストクエリ要件**:
```csharp
var baseEntitySet = mockEntitySet<Customer>();
var joinable = new JoinableEntitySet<Customer>(baseEntitySet);

// JOIN機能テスト
var joinResult = joinable.Join(orderEntitySet,
    c => c.Id, o => o.CustomerId);

// 委譲メソッドテスト
await joinable.AddAsync(new Customer());
var list = await joinable.ToListAsync();
```

### 5. ParameterReplacementVisitor
**場所**: `src/Query/Linq/ParameterReplacementVisitor.cs`

**主要機能**:
- 複数パラメータの一括置換
- Dictionary-based replacement

**テストクエリ要件**:
```csharp
var param1 = Expression.Parameter(typeof(Customer), "c");
var param2 = Expression.Parameter(typeof(Order), "o");
var replacement = new Dictionary<ParameterExpression, Expression> {
    { param1, Expression.Constant(new Customer()) },
    { param2, Expression.Property(param1, "Order") }
};

var visitor = new ParameterReplacementVisitor(replacement);
var originalExpr = Expression.Lambda(Expression.Property(param1, "Name"), param1);
var result = visitor.Visit(originalExpr);

// 置換結果の検証
Assert.NotEqual(originalExpr, result);
```

### 6. ReplaceParameterVisitor
**場所**: `src/Query/Linq/ReplaceParameterVisitor.cs`

**主要機能**:
- 単一パラメータの置換
- 静的メソッドによるFluent API

**テストクエリ要件**:
```csharp
var oldParam = Expression.Parameter(typeof(Customer), "old");
var newExpr = Expression.Parameter(typeof(Customer), "new");
var sourceExpr = Expression.Lambda(
    Expression.Property(oldParam, "Name"), oldParam);

var result = ReplaceParameterVisitor.Replace(sourceExpr, oldParam, newExpr);

// 置換前後の比較
Assert.Contains("new", result.ToString());
Assert.DoesNotContain("old", result.ToString());
```

### 7. ThreeWayJoinResult<TOuter, TInner, TThird>
**場所**: `src/Query/Linq/JoinableEntitySet.cs` (行150-216)

**主要機能**:
- 3-way JOIN結果の管理
- 複雑なJOIN条件の処理

**テストクエリ要件**:
```csharp
var threeWayJoin = new ThreeWayJoinResult<Customer, Order, Product>(
    customerSet, orderSet, productSet,
    c => c.Id, o => o.CustomerId,
    o => o.ProductId, p => p.Id);

var result = threeWayJoin.Select((c, o, p) => new {
    CustomerName = c.Name,
    OrderAmount = o.Amount,
    ProductName = p.Name
});

var finalResults = await result.ToListAsync();
```

### 8. TypedJoinResultEntitySet<TOuter, TInner, TResult>
**場所**: `src/Query/Linq/JoinableEntitySet.cs` (行269-322)

**主要機能**:
- 型安全なJOIN結果管理
- 2-way JOINの具象実装

**テストクエリ要件**:
```csharp
var typedJoinResult = new TypedJoinResultEntitySet<Customer, Order, JoinResult>(
    context, entityModel,
    customerSet, orderSet,
    c => c.Id, o => o.CustomerId,
    (c, o) => new JoinResult { CustomerName = c.Name, Amount = o.Amount });

// 実行と結果検証
var results = await typedJoinResult.ToListAsync();
Assert.All(results, r => Assert.NotNull(r.CustomerName));

// 制限事項テスト
await Assert.ThrowsAsync<NotSupportedException>(
    () => typedJoinResult.AddAsync(new JoinResult()));
```

### 9. TypedThreeWayJoinResultEntitySet<TOuter, TInner, TThird, TResult>
**場所**: `src/Query/Linq/JoinableEntitySet.cs` (行325-379)

**主要機能**:
- 型安全な3-way JOIN結果管理
- 複雑な型関係の処理

**テストクエリ要件**:
```csharp
var threeWayTypedResult = new TypedThreeWayJoinResultEntitySet<
    Customer, Order, Product, ComplexJoinResult>(
    context, entityModel,
    customerSet, orderSet, productSet,
    outerKeySelector, innerKeySelector,
    firstThirdKeySelector, secondThirdKeySelector,
    (c, o, p) => new ComplexJoinResult {
        Customer = c, Order = o, Product = p
    });

var results = await threeWayTypedResult.ToListAsync();
```

### 10. UnifiedJoinResult<TOuter, TInner>
**場所**: `src/Query/Linq/UnifiedJoinResult.cs`

**主要機能**:
- JOIN操作の統一インターフェース
- JoinBuilderとの統合
- 式木構築

**テストクエリ要件**:
```csharp
var unifiedJoin = new UnifiedJoinResult<Customer, Order>(
    customerSet, orderSet,
    outerKeySelector, innerKeySelector,
    joinBuilder, context);

// SELECT操作
var projected = unifiedJoin.Select((c, o) => new { c.Name, o.Amount });

// 3-way JOIN拡張
var extended = unifiedJoin.Join(productSet,
    c => c.CategoryId, p => p.CategoryId);

// KSQL生成確認
var ksqlQuery = joinBuilder.Build(/* expression */);
Assert.Contains("JOIN", ksqlQuery);
```

### 11. UnifiedThreeWayJoinResult<TOuter, TInner, TThird>
**場所**: `src/Query/Linq/UnifiedThreeWayJoinResult.cs`

**主要機能**:
- 3-way JOINの統一実装
- JoinKeySourceによる柔軟なキー指定

**テストクエリ要件**:
```csharp
var unifiedThreeWay = new UnifiedThreeWayJoinResult<Customer, Order, Product>(
    customerSet, orderSet, productSet,
    outerKeySelector, innerKeySelector,
    secondJoinKeySelector, thirdKeySelector,
    JoinKeySource.Outer, joinBuilder, context);

var result = unifiedThreeWay.Select((c, o, p) => new {
    CustomerName = c.Name,
    OrderDate = o.Date,
    ProductName = p.Name
});
```

## 重要な発見事項

### 1. アーキテクチャ上の課題
- **複数の実装パターン**: JOIN処理に3つの異なる実装アプローチが混在
- **責務の重複**: `JoinResult`と`UnifiedJoinResult`が類似機能を提供
- **型安全性の不整合**: 一部のクラスで型変換処理が不完全

### 2. テスト実装時の注意点
- **Mock Dependencies**: `IKsqlContext`, `EntityModel`, `JoinBuilder`の適切なモック化が必要
- **Expression Tree**: 複雑な式木の構築とvalidationが重要
- **Async Pattern**: 非同期処理の適切なテストカバレッジが必要

### 3. 相互依存関係
```
JoinableEntitySet<T>
    ↓
JoinResult<TOuter, TInner>
    ↓
TypedJoinResultEntitySet<TOuter, TInner, TResult>
    ↓
JoinResultEntitySet<T> (via JoinBuilder)
```

## 推奨テスト戦略

### Phase 1: 基礎クラス単体テスト
1. `ParameterReplacementVisitor` / `ReplaceParameterVisitor`
2. `EntitySetAdapter<T>`

### Phase 2: JOIN基本機能テスト
3. `JoinableEntitySet<T>`
4. `JoinResult<TOuter, TInner>`

### Phase 3: 統合テスト
5. `UnifiedJoinResult<TOuter, TInner>`
6. 3-way JOIN関連クラス群

### Phase 4: 実行結果テスト
7. `JoinResultEntitySet<T>`
8. `TypedJoinResultEntitySet<TOuter, TInner, TResult>`

## 品質基準適合性評価

**🔴 不適合項目**:
- 複数の責務重複により単体テストの複雑性が増大
- 型安全性の実装にギャップあり

**🟡 要改善項目**:
- Expression tree処理の例外ハンドリング
- 非同期処理のキャンセレーション対応

**🟢 適合項目**:
- インターフェース分離原則の遵守
- 適切な抽象化レベルの維持

テスト実装時は、特にExpression Treeの構築と型変換処理に重点を置いた検証が必要です。