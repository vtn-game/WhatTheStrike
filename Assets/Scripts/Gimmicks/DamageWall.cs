using UnityEngine;
using System.Collections.Generic;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// ダメージウォール - 触れるとダメージ
    /// </summary>
    public class DamageWall : GimmickBase
    {
        [Header("ダメージウォール設定")]
        [SerializeField] private int damage = 10;
        [SerializeField] private float damageInterval = 0.5f;

        // 各プレイヤーの最終ダメージ時刻
        private Dictionary<Player.Player, float> lastDamageTime = new Dictionary<Player.Player, float>();

        public override void OnPlayerEnter(Player.Player player)
        {
            ApplyDamage(player);
        }

        public override void OnPlayerStay(Player.Player player)
        {
            // インターバルチェック
            if (lastDamageTime.TryGetValue(player, out float lastTime))
            {
                if (Time.time - lastTime >= damageInterval)
                {
                    ApplyDamage(player);
                }
            }
            else
            {
                ApplyDamage(player);
            }
        }

        public override void OnPlayerExit(Player.Player player)
        {
            // クリーンアップ
            if (lastDamageTime.ContainsKey(player))
            {
                lastDamageTime.Remove(player);
            }
        }

        /// <summary>
        /// ダメージを適用
        /// </summary>
        private void ApplyDamage(Player.Player player)
        {
            player.TakeDamage(damage);
            lastDamageTime[player] = Time.time;

            // ダメージエフェクト
            PlayDamageEffect(player);
        }

        private void PlayDamageEffect(Player.Player player)
        {
            // ダメージエフェクト
            Debug.Log($"DamageWall hit {player.name} for {damage} damage!");
        }

        private void OnDrawGizmos()
        {
            // ダメージウォールを赤く表示
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube(transform.position, transform.localScale);
        }
    }
}
