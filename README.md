# What The Strike

モンストライクな引っ張りアクションゲームの概念を探求するプロジェクト。

## ゲーム概要

**「モンストの概念を破壊する」**

- モンストの「引っ張るもの」や「行動」をいじって別ゲーにする
- ノマステ（ノーマルステージ）っぽい1ステージを用意し、ルールを1個変更して攻略

### ゲームフロー

1. タイトル表示
2. ステージ登場
3. 引っ張り操作（ドラッグで発射方向・パワー調整）
4. 発射（リリースで飛んでいく）
5. クリア判定

### 基本ルール

- **ターン制**: Shootableを順番に発射
- **クリア条件**: クリア対象のDamageableを全滅
- **ゲームオーバー**: 全Shootable消滅 or ターン上限到達

---

## 環境構築

### 必要環境

| 項目 | バージョン |
|------|-----------|
| Unity | 6000.3.2f1 (Unity 6) |
| .NET | C# 10+ |

### セットアップ手順

1. リポジトリをクローン
   ```bash
   git clone <repository-url>
   cd WhatTheStrike
   ```

2. Unity Hubでプロジェクトを開く
   - Unity 6000.3.2f1 がインストールされていない場合はインストール

3. エディタでプロジェクトを開く

### シーン構成

| シーン | パス | 説明 |
|--------|------|------|
| GameLauncher | Assets/Scenes/System/GameLauncher.unity | 起動シーン |
| IngameSystem | Assets/Scenes/System/IngameSystem.unity | インゲームシステム |
| IngameDebug | Assets/Scenes/System/IngameDebug.unity | デバッグ用 |

---

## プロジェクト構成

```
WhatTheStrike/
├── Assets/
│   ├── Scripts/
│   │   ├── Domain/              # ドメイン層（純粋C#）
│   │   │   ├── ValueObjects/    # 値オブジェクト
│   │   │   └── Services/        # 計算ロジック
│   │   ├── Components/          # MonoBehaviourコンポーネント
│   │   │   ├── Shootable.cs     # ショット対象物
│   │   │   ├── Damageable.cs    # 被ダメージ物
│   │   │   └── Gimmick.cs       # ギミック基底クラス
│   │   ├── Gimmicks/            # 具体的なギミック実装
│   │   ├── Core/                # ゲーム管理
│   │   │   ├── GameManager.cs
│   │   │   └── TurnController.cs
│   │   └── Editor/              # エディタ拡張
│   ├── Scenes/
│   └── ThirdParty/
├── spec/                        # 仕様書
│   ├── README.md
│   ├── TODO.md
│   ├── ubi/                     # ユビキタス言語
│   ├── rule/                    # ルール定義
│   ├── gamedesign/              # ゲームデザイン
│   ├── code/                    # コード仕様
│   ├── usecase/                 # ユースケース
│   ├── ux/                      # UX設計
│   └── test/                    # テスト仕様
├── CLAUDE.md                    # AI開発ルール
└── README.md
```

---

## アーキテクチャ

**コンポーネント指向 + 軽量Domain層**

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

### 主要コンポーネント

| コンポーネント | 説明 |
|--------------|------|
| Shootable | ドラッグ操作で発射できるオブジェクト |
| Damageable | ダメージを受けて消滅しうるオブジェクト |
| Gimmick | Shootableと接触時に効果を発動 |

---

## 開発ガイド

### エディタ拡張

メニュー `WhatTheStrike > Create Game Scene` でゲームシーンを自動生成

### 命名規則

| 種別 | 規則 | 例 |
|------|------|-----|
| コンポーネント | 役割を表す名詞 | Shootable, Damageable |
| ギミック | 効果 + Gimmick | WarpGimmick, DamageWallGimmick |
| 値オブジェクト | 概念を表す名詞 | Health, Damage |
| サービス | 計算対象 + Calculator | DamageCalculator |

### コーディング規約

- C# 10+ を使用
- nullable参照型を有効化
- 詳細は `CLAUDE.md` を参照

---

## 仕様書

詳細な仕様は `spec/` ディレクトリを参照:

- `spec/ux/ux_design.md` - ゲームコンセプト・UX設計
- `spec/gamedesign/` - キャラクター・ステージ・ギミック仕様
- `spec/rule/` - ゲームルール・コーディングルール
- `spec/code/` - クラス詳細設計
- `spec/usecase/` - ゲームプレイフロー
- `spec/test/` - テスト仕様

---

## ライセンス

TBD
