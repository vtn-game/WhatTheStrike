# What The Strike - プロジェクト構造

## ディレクトリ構成

```
WhatTheStrike/
├── spec/                          # 仕様書
│   ├── game-design.md             # ゲーム全体の設計
│   ├── project-structure.md       # このファイル
│   ├── player.md                  # プレイヤーキャラ仕様
│   ├── enemy.md                   # 敵キャラ仕様
│   ├── stage.md                   # ステージ仕様
│   ├── gimmicks.md                # ギミック仕様
│   └── friendship-combo.md        # 友情コンボ仕様
│
└── Assets/                        # Unity Assets
    └── Scripts/
        ├── Core/                  # コアシステム
        │   ├── GameManager.cs
        │   └── PhysicsManager.cs
        ├── Player/                # プレイヤー関連
        │   ├── Player.cs
        │   └── PlayerController.cs
        ├── Enemy/                 # 敵関連
        │   └── Enemy.cs
        ├── Stage/                 # ステージ関連
        │   └── Stage.cs
        ├── Gimmicks/              # ギミック関連
        │   ├── GimmickBase.cs
        │   ├── Block.cs
        │   ├── GravityBarrier.cs
        │   ├── Mine.cs
        │   ├── Warp.cs
        │   └── DamageWall.cs
        └── FriendshipCombo/       # 友情コンボ関連
            ├── FriendshipComboBase.cs
            ├── Laser.cs
            ├── EnergyCircle.cs
            ├── SpeedUp.cs
            └── Explosion.cs
```

## コンポーネント対応表

| コンポーネント | Specファイル | コードファイル |
|---------------|-------------|---------------|
| プレイヤー | spec/player.md | Assets/Scripts/Player/*.cs |
| 敵 | spec/enemy.md | Assets/Scripts/Enemy/*.cs |
| ステージ | spec/stage.md | Assets/Scripts/Stage/*.cs |
| ギミック | spec/gimmicks.md | Assets/Scripts/Gimmicks/*.cs |
| 友情コンボ | spec/friendship-combo.md | Assets/Scripts/FriendshipCombo/*.cs |
