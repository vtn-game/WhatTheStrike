# Gimmickクラス設計

# 概要
- Shootableと接触時に効果を発動するオブジェクト
- 抽象基底クラスとして実装

# 実装
- MonoBehaviourを継承するabstract class
- Collider2Dを必須（トリガーとして使用）

# 外部変数
- isEnabled: 有効/無効フラグ

# 処理フロー

## 接触検知
1. OnTriggerEnter2Dでコライダー侵入検知
2. Shootableコンポーネント取得
3. OnShootableEnter(shootable)呼び出し

## 効果発動
- 派生クラスでOnShootableEnterを実装

# 外部インターフェース
- OnShootableEnter(Shootable): Shootable接触時の処理（abstract）

# 派生クラス

## WarpGimmick
- exitPoint: 出口位置
- preserveVelocity: 速度維持フラグ
- 効果: ShootableをexitPointにテレポート

## DamageWallGimmick
- damage: 接触ダメージ
- 効果: Shootableにダメージ（Damageable持ちの場合）

## BlockGimmick
- isDestructible: 破壊可能フラグ
- hp: HP（破壊可能時）
- 効果: Shootableを反射、破壊可能なら耐久減少

## GravityBarrierGimmick
- direction: 重力方向
- strength: 重力の強さ
- 効果: Shootableに力を加えて軌道変更

## MineGimmick
- damage: 爆発ダメージ
- explosionRadius: 爆発範囲
- 効果: 接触で爆発、範囲内にダメージ

# 期待値
- Shootable接触で各種効果が発動する
- 派生クラスで多様なギミックを実装可能

# エッジケース
- isEnabled = false 時は効果なし
