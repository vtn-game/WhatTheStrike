# コーディングルール

## 基本要素
- ファイルおよびクラスはなるべく分割する事
- クラス間の関係性は疎結合である事
- コンポーネントは単一責任を守る
- ドメインロジック（計算・検証）はDomain層に配置
- Unity API依存のコードはComponents/Core層に配置

## 言語仕様
- C# 10+ を使用
- nullable参照型を有効化

## 命名規則
| 種別 | 規則 | 例 |
|------|------|-----|
| コンポーネント | 役割を表す名詞 | Shootable, Damageable |
| ギミック | 効果 + Gimmick | WarpGimmick, DamageWallGimmick |
| 値オブジェクト | 概念を表す名詞 | Health, Damage, Direction |
| サービス | 計算対象 + Calculator | DamageCalculator, ShotCalculator |

## ライブラリの利用
- コルーチンはUniTaskを利用する事（導入時）
- イベント実装はR3を利用する事（導入時）
- interfaceを使用したクラスをインスペクタに表示するためにSubClassSelectorを使用する（導入時）

## ディレクトリ構造
```
Assets/Scripts/
├── Domain/                    # ドメイン層（純粋C#、Unity非依存）
│   ├── ValueObjects/          # 値オブジェクト
│   └── Services/              # ドメインサービス
│
├── Components/                # コンポーネント（MonoBehaviour）
│   ├── Shootable.cs
│   ├── Damageable.cs
│   └── Gimmick.cs
│
├── Gimmicks/                  # 具体的なギミック実装
│
├── Core/                      # ゲーム管理
│   ├── GameManager.cs
│   └── TurnController.cs
│
└── Editor/                    # エディタ拡張
```
