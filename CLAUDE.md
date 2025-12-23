# WhatTheStrike - 開発ルール

## アーキテクチャ

このプロジェクトは**コンポーネント指向 + 軽量Domain層**を採用しています。

### 設計思想

- コンポーネントを付けるだけでその役割を持つ、Unityらしい直感的な構築
- ドメインロジック（計算・値の検証）は純粋C#で保護
- 中間層を最小限に抑え、シンプルさを維持

### レイヤー構成

```
┌─────────────────────────────────────────────────┐
│              Components / Core                   │
│     (MonoBehaviour、ゲーム管理、入力処理)          │
├─────────────────────────────────────────────────┤
│                   Domain                         │
│          (ValueObjects, Services)                │
│              純粋C#、Unity非依存                  │
└─────────────────────────────────────────────────┘
```

### ディレクトリ構造

```
Assets/Scripts/
├── Domain/                    # ドメイン層（純粋C#、Unity非依存）
│   ├── ValueObjects/          # 値オブジェクト（Health, Damage, Direction等）
│   └── Services/              # ドメインサービス（計算ロジック）
│
├── Components/                # コンポーネント（MonoBehaviour）
│   ├── Shootable.cs           # ショット対象物
│   ├── Damageable.cs          # 被ダメージ物
│   └── Gimmick.cs             # ギミック基底クラス
│
├── Gimmicks/                  # 具体的なギミック実装
│   ├── WarpGimmick.cs
│   ├── DamageWallGimmick.cs
│   └── ...
│
├── Core/                      # ゲーム管理
│   ├── GameManager.cs         # ゲーム状態管理
│   └── TurnController.cs      # ターン制御
│
└── Editor/                    # エディタ拡張
    └── SceneBuilder.cs
```

## コンポーネント仕様

### Shootable（ショット対象物）

ドラッグ操作で発射できるオブジェクト。

```csharp
[Shootable]
├─ ターン順: int（登録順で自動割り当て）
├─ 移動速度: float
├─ バンプタイプ: Reflect / Pierce
├─ 友情コンボタイプ: None / Laser / EnergyCircle / SpeedUp / Explosion
├─ 友情コンボ威力: int
└─ 友情コンボ範囲: float
```

- 固定順でターンが回る
- 移動中に他のShootableに当たると、**当てられた側**の友情コンボが発動

### Damageable（被ダメージ物）

ダメージを受けて消滅しうるオブジェクト。

```csharp
[Damageable]
├─ 最大HP: int
├─ 現在HP: int
├─ 防御力: int
├─ クリア対象: bool（trueの場合、全滅でステージクリア）
└─ OnDeath: UnityEvent
```

- Shootableや友情コンボからダメージを受ける
- HP0で消滅（OnDeathイベント発火）

### Gimmick（ギミック）

Shootableと接触時に効果を発動。

```csharp
[Gimmick] (abstract)
├─ 有効/無効: bool
└─ OnPlayerEnter(Shootable): abstract
```

## ゲームルール

### ターン管理

- シーン上のShootableを登録順に操作
- 1体発射→停止で次のShootableへ
- 全Shootableが1回ずつ発射で1ターン終了

### 勝敗判定

**クリア条件:**
- `isTargetForClear = true` の Damageable が全滅

**ゲームオーバー条件:**
- 全Shootable消滅（Shootable + Damageable の場合）
- ターン数上限到達

## 命名規則

- **コンポーネント**: 役割を表す名詞（Shootable, Damageable）
- **ギミック**: 効果 + Gimmick（WarpGimmick, DamageWallGimmick）
- **値オブジェクト**: 概念を表す名詞（Health, Damage, Direction）
- **サービス**: 計算対象 + Calculator（DamageCalculator, ShotCalculator）

## コーディング規約

### 言語

- C# 10+ を使用
- nullable参照型を有効化

### 一般ルール

- コンポーネントは単一責任を守る
- ドメインロジック（計算・検証）はDomain層に配置
- Unity API依存のコードはComponents/Core層に配置

## ゲーム設計

詳細は `spec/` ディレクトリを参照:
- `ux/ux_design.md` - ゲームコンセプト・UX設計
- `gamedesign/chara.md` - プレイヤー仕様
- `gamedesign/enemy.md` - 敵仕様
- `gamedesign/stage.md` - ステージ仕様
- `gamedesign/gimmicks.md` - ギミック仕様
- `gamedesign/friendship-combo.md` - 友情コンボ仕様
- `rule/gamerule.md` - ゲームルール
- `code/` - クラス詳細設計
