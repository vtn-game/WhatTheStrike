using UnityEngine;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// 地雷 - 接触すると爆発
    /// </summary>
    public class Mine : GimmickBase
    {
        [Header("地雷設定")]
        [SerializeField] private int damage = 20;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private bool isOneShot = true;
        [SerializeField] private float rearmTime = 3f;

        private bool isArmed = true;

        public override void OnPlayerEnter(Player.Player player)
        {
            if (!isArmed) return;

            Explode();
        }

        /// <summary>
        /// 爆発処理
        /// </summary>
        private void Explode()
        {
            // 爆発エフェクト
            PlayExplosionEffect();

            // 範囲内のオブジェクトにダメージ
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            foreach (var hit in hits)
            {
                // プレイヤーへのダメージ
                var player = hit.GetComponent<Player.Player>();
                if (player != null)
                {
                    player.TakeDamage(damage);

                    // ノックバック
                    ApplyKnockback(player);
                }

                // 敵へのダメージ
                var enemy = hit.GetComponent<Enemy.Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }

            if (isOneShot)
            {
                Destroy(gameObject);
            }
            else
            {
                isArmed = false;
                StartCoroutine(Rearm());
            }
        }

        /// <summary>
        /// ノックバックを適用
        /// </summary>
        private void ApplyKnockback(Player.Player player)
        {
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (player.transform.position - transform.position).normalized;
                rb.AddForce(direction * damage * 0.5f, ForceMode2D.Impulse);
            }
        }

        /// <summary>
        /// 再起動
        /// </summary>
        private System.Collections.IEnumerator Rearm()
        {
            yield return new WaitForSeconds(rearmTime);
            isArmed = true;
        }

        /// <summary>
        /// 爆発エフェクト
        /// </summary>
        private void PlayExplosionEffect()
        {
            // TODO: パーティクルエフェクト
            Debug.Log("Mine exploded!");
        }

        private void OnDrawGizmos()
        {
            // 爆発範囲を表示
            Gizmos.color = isArmed ? Color.red : Color.gray;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
