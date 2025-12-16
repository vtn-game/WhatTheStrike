using UnityEngine;

namespace WhatTheStrike.FriendshipCombo
{
    /// <summary>
    /// 友情コンボの種類
    /// </summary>
    public enum FriendshipComboType
    {
        None,
        Laser,
        EnergyCircle,
        SpeedUp,
        Explosion
    }

    /// <summary>
    /// 友情コンボ基底クラス
    /// </summary>
    public abstract class FriendshipComboBase : MonoBehaviour
    {
        [Header("友情コンボ共通設定")]
        [SerializeField] protected int baseDamage = 10;
        [SerializeField] protected float duration = 1f;

        protected Player.Player owner;

        /// <summary>
        /// 友情コンボを実行
        /// </summary>
        public abstract void Execute(Player.Player owner);

        /// <summary>
        /// エフェクト生成
        /// </summary>
        protected virtual void SpawnEffect() { }

        /// <summary>
        /// 範囲内の敵を取得
        /// </summary>
        protected Enemy.Enemy[] GetEnemiesInRange(Vector2 center, float radius)
        {
            var colliders = Physics2D.OverlapCircleAll(center, radius);
            var enemies = new System.Collections.Generic.List<Enemy.Enemy>();

            foreach (var col in colliders)
            {
                var enemy = col.GetComponent<Enemy.Enemy>();
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }

            return enemies.ToArray();
        }
    }
}
