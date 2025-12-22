using UnityEngine;
using UnityEngine.Events;
using System;
using WhatTheStrike.Domain.ValueObjects;
using WhatTheStrike.Domain.Services;

namespace WhatTheStrike.Components
{
    /// <summary>
    /// 被ダメージ物コンポーネント
    /// ダメージを受けて消滅しうるオブジェクトに付ける
    /// </summary>
    public class Damageable : MonoBehaviour
    {
        [Header("ステータス")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int defense = 0;

        [Header("クリア条件")]
        [SerializeField] private bool isTargetForClear = false;

        [Header("イベント")]
        [SerializeField] private UnityEvent? onDeath;
        [SerializeField] private UnityEvent<int>? onDamaged;

        // 状態
        private Health _health;
        private SpriteRenderer? _spriteRenderer;

        // イベント
        public event Action<Damageable>? OnDeath;
        public event Action<Damageable, int>? OnDamaged;

        // プロパティ
        public int CurrentHp => _health.Current;
        public int MaxHp => _health.Max;
        public float HpRatio => _health.Ratio;
        public bool IsAlive => _health.IsAlive;
        public bool IsTargetForClear => isTargetForClear;
        public int Defense => defense;

        private void Awake()
        {
            _health = new Health(maxHp);
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int rawDamage)
        {
            if (!IsAlive) return;

            var damage = DamageCalculator.Calculate(rawDamage);
            _health = DamageCalculator.ApplyDamage(_health, damage, defense);

            int finalDamage = damage.CalculateFinal(defense);
            OnDamaged?.Invoke(this, finalDamage);
            onDamaged?.Invoke(finalDamage);

            PlayDamageEffect();

            if (_health.IsDead)
            {
                Die();
            }
        }

        /// <summary>
        /// 回復
        /// </summary>
        public void Heal(int amount)
        {
            _health = _health.Heal(amount);
        }

        /// <summary>
        /// ダメージエフェクト
        /// </summary>
        private void PlayDamageEffect()
        {
            if (_spriteRenderer != null)
            {
                StartCoroutine(DamageFlash());
            }
        }

        private System.Collections.IEnumerator DamageFlash()
        {
            if (_spriteRenderer == null) yield break;

            var originalColor = _spriteRenderer.color;
            _spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            _spriteRenderer.color = originalColor;
        }

        /// <summary>
        /// 死亡処理
        /// </summary>
        private void Die()
        {
            OnDeath?.Invoke(this);
            onDeath?.Invoke();

            StartCoroutine(DeathSequence());
        }

        private System.Collections.IEnumerator DeathSequence()
        {
            if (_spriteRenderer != null)
            {
                float duration = 0.5f;
                float elapsed = 0f;
                var originalColor = _spriteRenderer.color;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float alpha = 1f - (elapsed / duration);
                    _spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    yield return null;
                }
            }

            // Shootableも持っている場合は非アクティブに、そうでなければ破棄
            var shootable = GetComponent<Shootable>();
            if (shootable != null)
            {
                shootable.Destroy();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
