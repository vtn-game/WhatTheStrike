# 友情コンボ仕様

## 概要

味方キャラ同士がぶつかると発動する特殊攻撃。

## 対応コード

- `Assets/Scripts/FriendshipCombo/FriendshipComboBase.cs` - 友情コンボ基底クラス
- `Assets/Scripts/FriendshipCombo/FriendshipComboManager.cs` - 友情コンボ管理
- `Assets/Scripts/FriendshipCombo/Laser.cs` - レーザー
- `Assets/Scripts/FriendshipCombo/EnergyCircle.cs` - エナジーサークル（エナサー）
- `Assets/Scripts/FriendshipCombo/SpeedUp.cs` - スピードアップ
- `Assets/Scripts/FriendshipCombo/Explosion.cs` - 爆発

---

## 発動条件

1. プレイヤーキャラが移動中（Moving状態）
2. 別の味方キャラに接触
3. 接触された味方の友情コンボが発動

---

## 友情コンボ種類

```csharp
enum FriendshipComboType {
    Laser,        // レーザー
    EnergyCircle, // エナジーサークル
    SpeedUp,      // スピードアップ
    Explosion     // 爆発
}
```

---

### 1. レーザー (Laser)

直線状にダメージを与える。

| パラメータ | 説明 |
|-----------|------|
| damage | ダメージ量 |
| range | 射程距離 |
| width | レーザー幅 |
| duration | 持続時間 |

**動作:**
- 発動者から8方向 or ランダム方向にレーザー発射
- 射程内の敵にダメージ
- 壁で遮断される

---

### 2. エナジーサークル (EnergyCircle / エナサー)

周囲にエネルギーの輪を展開。

| パラメータ | 説明 |
|-----------|------|
| damage | ダメージ量 |
| radius | 効果半径 |
| duration | 持続時間 |

**動作:**
- 発動者を中心に円形エリア展開
- エリア内の敵に継続ダメージ

---

### 3. スピードアップ (SpeedUp)

ショットしたキャラの速度を上げる。

| パラメータ | 説明 |
|-----------|------|
| speedMultiplier | 速度倍率 |
| duration | 効果時間 |

**動作:**
- 移動中のキャラにバフ付与
- 速度が一時的に上昇

---

### 4. 爆発 (Explosion)

範囲ダメージを与える。

| パラメータ | 説明 |
|-----------|------|
| damage | ダメージ量 |
| radius | 爆発半径 |

**動作:**
- 発動者の位置で爆発
- 範囲内の敵にダメージ

---

## 連鎖

友情コンボで敵を倒したり、他の味方に当たると連鎖が発生する（拡張用）。
