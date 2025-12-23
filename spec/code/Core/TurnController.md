# TurnControllerクラス設計

# 概要
- ターン制御と入力処理を担当
- Shootableの順番管理

# 実装
- MonoBehaviourを継承する

# 外部変数
- shootables: 操作対象のShootableリスト

# 処理フロー

## 初期化
- シーン上のShootableを登録順に収集
- 最初のShootableをReady状態に

## ターン進行
1. 現在のShootableが停止
2. 次のShootableをReady状態に
3. 全員終了でターン終了、GameManagerに通知
4. 次のターン開始

## 入力処理
1. マウス/タッチ開始でドラッグ開始
2. ドラッグ中は方向・パワー計算
3. リリースでShootable.Shoot()呼び出

# 外部インターフェース
- RegisterShootable(Shootable): Shootable登録
- GetCurrentShootable(): 現在操作対象取得

# 期待値
- 登録順にShootableを操作できる
- 全員発射でターン進行
- ドラッグ操作でショット可能

# エッジケース
- Shootableが途中で破壊された場合はスキップ
- 全Shootable消滅でGameOver判定依頼
