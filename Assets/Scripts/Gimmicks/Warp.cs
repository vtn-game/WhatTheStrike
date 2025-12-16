using UnityEngine;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// ワープ - 別の位置にテレポート
    /// </summary>
    public class Warp : GimmickBase
    {
        [Header("ワープ設定")]
        [SerializeField] private Transform exitPoint;
        [SerializeField] private bool preserveVelocity = true;
        [SerializeField] private float cooldownTime = 0.5f;

        private bool isOnCooldown = false;

        public override void OnPlayerEnter(Player.Player player)
        {
            if (isOnCooldown) return;
            if (exitPoint == null) return;

            TeleportPlayer(player);
        }

        /// <summary>
        /// プレイヤーをテレポート
        /// </summary>
        private void TeleportPlayer(Player.Player player)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            Vector2 currentVelocity = rb != null ? rb.linearVelocity : Vector2.zero;

            // エントリーエフェクト
            PlayEntryEffect();

            // 位置を移動
            player.transform.position = exitPoint.position;

            // 速度の処理
            if (rb != null)
            {
                if (preserveVelocity)
                {
                    // 速度維持（出口の向きに合わせて回転させる場合はここで処理）
                    rb.linearVelocity = currentVelocity;
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }

            // イグジットエフェクト
            PlayExitEffect();

            // クールダウン開始
            StartCoroutine(Cooldown());
        }

        private System.Collections.IEnumerator Cooldown()
        {
            isOnCooldown = true;
            yield return new WaitForSeconds(cooldownTime);
            isOnCooldown = false;
        }

        private void PlayEntryEffect()
        {
            // 入口エフェクト
        }

        private void PlayExitEffect()
        {
            // 出口エフェクト
        }

        private void OnDrawGizmos()
        {
            // 入口と出口を線で結ぶ
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            if (exitPoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(exitPoint.position, 0.5f);

                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position, exitPoint.position);
            }
        }
    }
}
