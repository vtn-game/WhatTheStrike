# プロジェクト全体の詳細設計

## アーキテクチャ方針
**コンポーネント指向 + 軽量Domain層**を採用

### 設計思想
- コンポーネントを付けるだけでその役割を持つ、Unityらしい直感的な構築
- ドメインロジック（計算・値の検証）は純粋C#で保護
- 中間層を最小限に抑え、シンプルさを維持

## レイヤー構成

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

## コンポーネント設計
- 個々の詳細な設計は、`code/`以下にクラス名と同じmdを記載し仕様化する
- 個々の仕様が無いものは、このページの情報をもとに生成する事

## 主要コンポーネント

### Shootable（ショット対象物）
ドラッグ操作で発射できるオブジェクト
- 詳細: `code/Components/Shootable.md`

### Damageable（被ダメージ物）
ダメージを受けて消滅しうるオブジェクト
- 詳細: `code/Components/Damageable.md`

### Gimmick（ギミック）
Shootableと接触時に効果を発動
- 詳細: `code/Components/Gimmick.md`

## Domain層

### ValueObjects
- Health: HP管理
- Damage: ダメージ値

### Services
- DamageCalculator: ダメージ計算
- ShotCalculator: ショット方向・パワー計算
