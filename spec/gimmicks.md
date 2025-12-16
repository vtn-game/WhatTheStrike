# ギミック仕様

## 概要

ステージに配置されるギミック。プレイヤーの移動に影響を与える。

## 対応コード

- `Assets/Scripts/Gimmicks/GimmickBase.cs` - ギミック基底クラス
- `Assets/Scripts/Gimmicks/Block.cs` - ブロック
- `Assets/Scripts/Gimmicks/GravityBarrier.cs` - 重力バリア
- `Assets/Scripts/Gimmicks/Mine.cs` - 地雷
- `Assets/Scripts/Gimmicks/Warp.cs` - ワープ
- `Assets/Scripts/Gimmicks/DamageWall.cs` - ダメージウォール

---

## ギミック一覧

### 1. ブロック (Block)

障害物。破壊可能/不可能の2タイプ。

| パラメータ | 説明 |
|-----------|------|
| isDestructible | 破壊可能かどうか |
| hp | 破壊可能な場合のHP |

**動作:**
- プレイヤーが接触すると反射
- 破壊可能な場合、ダメージでHPが減少
- HP <= 0 で破壊

---

### 2. 重力バリア (GravityBarrier)

通過すると速度が変化するエリア。

| パラメータ | 説明 |
|-----------|------|
| direction | 重力方向（上下左右） |
| strength | 重力の強さ |

**動作:**
- プレイヤーが侵入すると指定方向に力を加える
- 軌道が曲がる

---

### 3. 地雷 (Mine)

接触すると爆発してダメージを与える。

| パラメータ | 説明 |
|-----------|------|
| damage | 爆発ダメージ |
| explosionRadius | 爆発範囲 |
| isOneShot | 1回限りか |

**動作:**
- プレイヤーが接触すると爆発
- 範囲内の敵/味方にダメージ
- 1回限りの場合は消滅

---

### 4. ワープ (Warp)

別の位置にテレポートさせる。

| パラメータ | 説明 |
|-----------|------|
| exitPoint | 出口位置 |
| preserveVelocity | 速度を維持するか |

**動作:**
- プレイヤーが入口に侵入
- 出口位置にテレポート
- 速度維持 or リセット

---

### 5. ダメージウォール (DamageWall)

触れるとダメージを受ける壁。

| パラメータ | 説明 |
|-----------|------|
| damage | 接触ダメージ |
| damageInterval | ダメージ間隔 |

**動作:**
- プレイヤーが接触するとダメージ
- 連続接触時はインターバル後に再ダメージ

---

## 共通インターフェース

```csharp
interface IGimmick {
    void OnPlayerEnter(Player player);
    void OnPlayerStay(Player player);
    void OnPlayerExit(Player player);
}
```
