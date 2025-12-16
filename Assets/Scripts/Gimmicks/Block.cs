using UnityEngine;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// ブロック - 障害物ギミック
    /// </summary>
    public class Block : GimmickBase
    {
        [Header("ブロック設定")]
        [SerializeField] private bool isDestructible = false;
        [SerializeField] private int maxHp = 10;

        private int currentHp;

        // プロパティ
        public bool IsDestructible => isDestructible;
        public int CurrentHp => currentHp;

        private void Awake()
        {
            currentHp = maxHp;
        }

        public override void OnPlayerEnter(Player.Player player)
        {
            // ブロックはColliderで反射させるので、ここでは破壊処理のみ
            if (isDestructible && player.State == Player.PlayerState.Moving)
            {
                TakeDamage(player.Attack);
            }
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!isDestructible) return;

            currentHp = Mathf.Max(0, currentHp - damage);

            // ダメージエフェクト
            PlayHitEffect();

            if (currentHp <= 0)
            {
                Destroy();
            }
        }

        private void PlayHitEffect()
        {
            // ヒットエフェクト
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashEffect(spriteRenderer));
            }
        }

        private System.Collections.IEnumerator FlashEffect(SpriteRenderer sr)
        {
            Color originalColor = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.05f);
            sr.color = originalColor;
        }

        private void Destroy()
        {
            // 破壊エフェクト後に削除
            // TODO: パーティクルエフェクト
            Destroy(gameObject);
        }

        // ブロックは物理衝突を使う
        private void OnCollisionEnter2D(Collision2D collision)
        {
            var player = collision.gameObject.GetComponent<Player.Player>();
            if (player != null)
            {
                OnPlayerEnter(player);
            }
        }
    }
}
