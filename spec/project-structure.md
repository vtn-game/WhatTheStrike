# What The Strike - プロジェクト構造

## アーキテクチャ

このプロジェクトは**コンポーネント指向 + 軽量Domain層**を採用しています。

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

## ディレクトリ構成

```
WhatTheStrike/
├── CLAUDE.md                      # 開発ルール
├── spec/                          # 仕様書
│   ├── game-design.md             # ゲーム全体の設計
│   ├── project-structure.md       # このファイル
│   ├── player.md                  # プレイヤーキャラ仕様
│   ├── enemy.md                   # 敵キャラ仕様
│   ├── stage.md                   # ステージ仕様
│   ├── gimmicks.md                # ギミック仕様
│   └── friendship-combo.md        # 友情コンボ仕様
│
└── Assets/Scripts/
    ├── Domain/                    # ドメイン層（純粋C#、Unity非依存）
    │   ├── ValueObjects/          # 値オブジェクト
    │   │   ├── Health.cs          # HP管理
    │   │   ├── Damage.cs          # ダメージ計算
    │   │   └── Direction.cs       # 方向
    │   └── Services/              # ドメインサービス
    │       ├── DamageCalculator.cs
    │       └── ShotCalculator.cs
    │
    ├── Components/                # コンポーネント（MonoBehaviour）
    │   ├── Shootable.cs           # ショット対象物
    │   ├── Damageable.cs          # 被ダメージ物
    │   └── Gimmick.cs             # ギミック基底クラス
    │
    ├── Gimmicks/                  # 具体的なギミック実装
    │   ├── WarpGimmick.cs
    │   ├── DamageWallGimmick.cs
    │   ├── GravityBarrierGimmick.cs
    │   ├── MineGimmick.cs
    │   └── BlockGimmick.cs
    │
    ├── Core/                      # ゲーム管理
    │   ├── GameManager.cs         # ゲーム状態管理
    │   └── TurnController.cs      # ターン制御・入力処理
    │
    └── Editor/                    # エディタ拡張
        └── SceneBuilder.cs
```

## コンポーネント対応表

| コンポーネント | 用途 | 例 |
|---------------|------|-----|
| Shootable | ショット対象物 | プレイヤーキャラ |
| Damageable | 被ダメージ物 | 敵、破壊可能ブロック |
| Shootable + Damageable | 両方 | プレイヤーキャラ（ダメージも受ける場合） |
| Gimmick派生 | ギミック | ワープ、ダメージ壁、重力バリア等 |

## シーン構成

エディタメニュー `WhatTheStrike > Create Game Scene` で自動構築:

```
GameScene
├── Main Camera
├── GameManager
├── TurnController
├── Stage
│   ├── TopWall
│   ├── BottomWall
│   ├── LeftWall
│   └── RightWall
├── Shootables/
│   ├── Player_Red      [Shootable] [Damageable]
│   ├── Player_Blue     [Shootable] [Damageable]
│   ├── Player_Green    [Shootable] [Damageable]
│   └── Player_Yellow   [Shootable] [Damageable]
├── Enemies/
│   ├── Enemy_1         [Damageable] isTargetForClear=true
│   ├── Enemy_2         [Damageable] isTargetForClear=true
│   └── ...
├── Gimmicks/
│   └── (任意のギミック)
└── Canvas
    ├── HPPanel
    ├── TurnDisplay
    └── GameStateDisplay
```

## ゲームフロー

```
┌─────────────┐
│    Title    │
└──────┬──────┘
       ↓ StartGame
┌─────────────┐
│ StageIntro  │
└──────┬──────┘
       ↓ 2秒後
┌─────────────┐ ←───────────────────────┐
│   Playing   │                         │
│             │  Shootable発射→停止     │
│  ┌────────────────────────────┐      │
│  │ 現在のShootableを操作      │      │
│  │ ドラッグ→発射→移動→停止   │      │
│  └────────────────────────────┘      │
│             │                         │
│      停止後、次のShootableへ ─────────┘
│             │
│  全Shootable発射で1ターン終了
│             │
└──────┬──────┘
       │
       ├─→ クリア対象Damageable全滅 → StageClear
       │
       ├─→ 全Shootable消滅 → GameOver
       │
       └─→ ターン上限到達 → GameOver
```
