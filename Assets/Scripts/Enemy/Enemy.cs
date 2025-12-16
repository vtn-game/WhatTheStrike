using UnityEngine;
using System;

namespace WhatTheStrike.Enemy
{
    /// <summary>
    /// 敵のサイズ
    /// </summary>
    public enum EnemySize
    {
        Small,
        Medium,
        Large,
        Boss
    }

    /// <summary>
    /// 属性（拡張用）
    /// </summary>
    public enum Element
    {
        None,
        Fire,
        Water,
        Wood,
        Light,
        Dark
    }

    /// <summary>
    /// 敵キャラクター
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("ステータス")]
        [SerializeField] private int maxHp = 50;
        [SerializeField] private int attack = 5;
        [SerializeField] private int defense = 0;
        [SerializeField] private EnemySize size = EnemySize.Medium;
        [SerializeField] private Element element = Element.None;

        // 現在のステータス
        private int currentHp;

        // イベント
        public event Action<Enemy> OnDeath;
        public event Action<Enemy, int> OnDamaged;

        // プロパティ
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int Attack => attack;
        public int Defense => defense;
        public EnemySize Size => size;
        public Element Element => element;
        public bool IsAlive => currentHp > 0;

        private void Awake()
        {
            currentHp = maxHp;
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int rawDamage)
        {
            // 防御力を考慮したダメージ計算（最低1ダメージ保証）
            int finalDamage = Mathf.Max(1, rawDamage - defense);
            currentHp = Mathf.Max(0, currentHp - finalDamage);

            OnDamaged?.Invoke(this, finalDamage);

            // ダメージエフェクト
            PlayDamageEffect();

            if (currentHp <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// ダメージエフェクト再生
        /// </summary>
        private void PlayDamageEffect()
        {
            // ヒットストップやフラッシュなど
            StartCoroutine(DamageFlash());
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
            }
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            OnDeath?.Invoke(this);

            // 死亡アニメーション後に破棄
            StartCoroutine(DeathSequence());
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            // 死亡アニメーション
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                float duration = 0.5f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = 1f - (elapsed / duration);
                    spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
                    yield return null;
                }
            }

            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // プレイヤーとの衝突
            var player = collision.gameObject.GetComponent<Player.Player>();
            if (player != null && player.State == Player.PlayerState.Moving)
            {
                // プレイヤーからダメージを受ける
                TakeDamage(player.Attack);

                // プレイヤーにもダメージを与える（オプション）
                // player.TakeDamage(attack);
            }
        }

        /// <summary>
        /// HP回復（拡張用）
        /// </summary>
        public void Heal(int amount)
        {
            currentHp = Mathf.Min(maxHp, currentHp + amount);
        }
    }
}
