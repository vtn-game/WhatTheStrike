# Shootableクラス設計

# 概要
- ドラッグ操作で発射できるオブジェクト
- ターン順で操作が回ってくる

# 実装
- MonoBehaviourを継承する
- Rigidbody2DとCollider2Dを必須コンポーネントとする

# 外部変数
- moveSpeed: 移動速度
- bumpType: バンプタイプ（Reflect / Pierce）
- friendshipComboType: 友情コンボタイプ
- friendshipComboPower: 友情コンボ威力
- friendshipComboRange: 友情コンボ範囲

# 状態管理
```csharp
enum ShootableState {
    Waiting,  // 待機中（他のキャラのターン）
    Ready,    // ショット可能
    Moving,   // 移動中
    Stopped   // 停止（ターン終了待ち）
}
```

# 処理フロー

## 初期化
- Rigidbody2Dの設定（Kinematic → Dynamic）
- Collider2Dの設定

## ショット処理
1. Ready状態でドラッグ入力を受け付け
2. Shoot(direction, power)で発射
3. Moving状態に遷移
4. 速度がstopThreshold以下で停止
5. OnStoppedイベント発火

## 衝突処理
- 壁: 反射
- 敵(Damageable): ダメージ付与、バンプタイプに応じて反射or貫通
- 味方(Shootable): 当てられた側の友情コンボ発動

# イベント
- OnStopped: 停止時に発火（TurnControllerが購読）

# 期待値
- ドラッグ距離に応じたパワーでショットできる
- 壁で反射して移動を続ける
- 速度低下で自動停止

# エッジケース
- ターン中に破壊された場合はスキップ
