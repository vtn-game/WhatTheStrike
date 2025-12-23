# インゲーム中に期待される振る舞い
インゲームはInGameシーンを使用する


## Feature: プレイヤー入力

#### Scenario: ショット入力の処理
  Given 現在のShootableがReady状態である
  When ドラッグ操作が行われる
  Then 引っ張りUIが表示される
  And ドラッグ方向と距離に応じたパワーが計算される

#### Scenario: ショットの発射
  Given 引っ張り操作中である
  When ドラッグをリリースする
  Then Shootableが計算された方向・パワーで発射される
  And ShootableがMoving状態になる

#### Scenario: 入力の無効化
  Given ゲームが[ポーズ状態]である
  When なんらかのドラッグ入力があった
  Then 入力を処理しない


## Feature: Shootableの移動

#### Scenario: 壁との衝突
  Given ShootableがMoving状態である
  When 壁に衝突する
  Then 反射して移動を続ける

#### Scenario: 敵との衝突（反射タイプ）
  Given ShootableがReflectタイプである
  And ShootableがMoving状態である
  When Damageableに衝突する
  Then ダメージを与える
  And 反射して移動を続ける

#### Scenario: 敵との衝突（貫通タイプ）
  Given ShootableがPierceタイプである
  And ShootableがMoving状態である
  When Damageableに衝突する
  Then ダメージを与える
  And そのまま貫通して移動を続ける

#### Scenario: 移動の停止
  Given ShootableがMoving状態である
  When 速度がstopThreshold以下になる
  Then ShootableがStopped状態になる
  And 次のShootableの操作に移行する


## Feature: 友情コンボ

#### Scenario: 友情コンボの発動
  Given ShootableがMoving状態である
  When 別のShootableに衝突する
  Then 当てられた側の友情コンボが発動する

#### Scenario: レーザー友情コンボ
  Given 友情コンボタイプがLaserである
  When 友情コンボが発動する
  Then 8方向にレーザーが発射される
  And 射程内のDamageableにダメージを与える


## Feature: ターン進行

#### Scenario: ターンの進行
  Given 全Shootableが1回ずつ発射した
  When 最後のShootableが停止する
  Then ターンが1進む
  And 最初のShootableから再び操作可能になる


## Feature: ゲームの勝利条件と敗北条件

#### Scenario: ステージクリア
  Given クリア対象のDamageableが存在する
  When 全てのクリア対象DamageableのHPが0になる
  Then ステージクリアとなる

#### Scenario: ゲームオーバー（ターン上限）
  Given ターン上限が設定されている
  When ターン数がターン上限に達する
  Then ゲームオーバーとなる

#### Scenario: ゲームオーバー（Shootable全滅）
  Given ShootableがDamageableでもある
  When 全てのShootableが消滅する
  Then ゲームオーバーとなる
