# GameManagerクラス設計

# 概要
- ゲーム全体の制御をするクラス
- シングルトンパターンで実装

# 実装
- MonoBehaviourを継承する
- シングルトンとしてInstanceプロパティを提供

# 外部変数
- maxTurns: ターン上限

# 状態管理
```csharp
enum GameState {
    Title,        // タイトル画面
    StageIntro,   // ステージ開始演出
    Playing,      // プレイ中
    Paused,       // ポーズ中
    StageClear,   // ステージクリア
    GameOver      // ゲームオーバー
}
```

# 処理フロー

## 初期化
- シングルトン設定
- クリア対象Damageableの収集

## ターン管理
- 現在ターン数の管理
- ターン上限チェック

## クリア判定
- クリア対象の全滅でStageClear
- 全Shootable消滅 or ターン上限でGameOver

# イベント
- OnTurnChanged(int turn): ターン変更時
- OnStateChanged(GameState state): 状態変更時
- OnStageClear: ステージクリア時
- OnGameOver: ゲームオーバー時

# 外部インターフェース
- RegisterClearTarget(Damageable): クリア対象登録
- UnregisterClearTarget(Damageable): クリア対象解除（死亡時）
- IncrementTurn(): ターン進行
- SetState(GameState): 状態変更

# 期待値
- クリア対象全滅でクリア判定
- ターン上限到達でゲームオーバー判定

# エッジケース
- クリア対象が0の場合は即クリア
