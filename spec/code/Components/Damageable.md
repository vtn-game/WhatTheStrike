# Damageableクラス設計

# 概要
- ダメージを受けて消滅しうるオブジェクト
- HP管理とクリア判定フラグを持つ

# 実装
- MonoBehaviourを継承する
- Domain/ValueObjects/Healthを使用してHP管理

# 外部変数
- maxHp: 最大HP
- defense: 防御力
- isTargetForClear: クリア対象フラグ（trueの場合、全滅でステージクリア）

# 処理フロー

## 初期化
- Healthの初期化（maxHp）
- GameManagerへの登録（isTargetForClear時）

## ダメージ処理
1. TakeDamage(rawDamage)呼び出し
2. DamageCalculatorで最終ダメージ計算
3. Health.TakeDamage()でHP減少
4. HP <= 0 で死亡処理

## 死亡処理
1. OnDeathイベント発火
2. GameManagerに通知（クリア対象の場合）
3. オブジェクト破棄

# イベント
- OnDeath: 死亡時に発火

# 外部インターフェース
- TakeDamage(int rawDamage): ダメージを受ける
- Health: 現在のHP状態（読み取り専用）
- IsAlive: 生存判定

# 期待値
- ダメージを受けてHPが減少する
- HP0で消滅する
- クリア対象が全滅でステージクリア

# エッジケース
- 最低1ダメージ保証（防御力がダメージを上回る場合）
