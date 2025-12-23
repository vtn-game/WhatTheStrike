# インゲームのルール

## 概要
- モンストライクな引っ張りアクションゲーム
- ターン制でShootableを発射し、Damageableを倒す

## ターン管理
- シーン上のShootableを**登録順（固定順）**に操作
- 1体発射→停止で次のShootableへ
- 全Shootableが1回ずつ発射で1ターン終了

## オブジェクト
- Shootable: プレイヤーが操作するショット対象物
  - gamedesign/chara.mdを参照
- Damageable: ダメージを受ける対象物
  - gamedesign/enemy.mdを参照
- Gimmick: ステージ上の仕掛け
  - gamedesign/gimmicks.mdを参照

## ゲームプレイ
- プレイヤーはShootableをドラッグして引っ張り、離して発射
- Shootableは物理演算で移動し、壁で反射
- Damageableに当たるとダメージを与える
- Shootable同士がぶつかると友情コンボが発動

## 勝敗条件

### クリア条件
- `isTargetForClear = true` の Damageable が**全滅**

### ゲームオーバー条件
以下のいずれかを満たした場合:
- 全Shootable消滅（Shootable + Damageable の場合）
- ターン数上限到達
