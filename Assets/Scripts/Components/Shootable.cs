using UnityEngine;
using UnityEngine.Events;
using System;
using WhatTheStrike.Domain.Services;

namespace WhatTheStrike.Components
{
    /// <summary>
    /// バンプタイプ（衝突時の挙動）
    /// </summary>
    public enum BumpType
    {
        Reflect,  // 反射
        Pierce    // 貫通
    }

    /// <summary>
    /// 友情コンボタイプ
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
    /// ショット対象物の状態
    /// </summary>
    public enum ShootableState
    {
        Waiting,  // 待機中（他のShootableのターン）
        Ready,    // 発射可能
        Moving,   // 移動中
        Stopped   // 停止（ターン終了待ち）
    }

    /// <summary>
    /// ショット対象物コンポーネント
    /// ドラッグ操作で発射できるオブジェクトに付ける
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Shootable : MonoBehaviour
    {
        [Header("ステータス")]
        [SerializeField] private float speed = 1.0f;
        [SerializeField] private BumpType bumpType = BumpType.Reflect;
        [SerializeField] private int attack = 10;

        [Header("友情コンボ")]
        [SerializeField] private FriendshipComboType friendshipComboType = FriendshipComboType.None;
        [SerializeField] private int friendshipComboDamage = 10;
        [SerializeField] private float friendshipComboRange = 3f;

        [Header("物理設定")]
        [SerializeField] private float stopThreshold = 0.1f;
        [SerializeField] private float bounciness = 0.8f;

        // 状態
        private ShootableState _state = ShootableState.Waiting;
        private Rigidbody2D _rb = null!;

        // イベント
        public event Action<Shootable>? OnStopped;
        public event Action<Shootable, Shootable>? OnHitShootable;

        // プロパティ
        public ShootableState State => _state;
        public int Attack => attack;
        public float Speed => speed;
        public BumpType BumpType => bumpType;
        public FriendshipComboType FriendshipComboType => friendshipComboType;
        public int FriendshipComboDamage => friendshipComboDamage;
        public float FriendshipComboRange => friendshipComboRange;
        public bool IsAlive => gameObject.activeInHierarchy;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0;
        }

        /// <summary>
        /// 状態を変更
        /// </summary>
        public void SetState(ShootableState newState)
        {
            _state = newState;
        }

        /// <summary>
        /// 発射
        /// </summary>
        public bool Shoot(Vector2 direction, float power)
        {
            if (_state != ShootableState.Ready) return false;

            _state = ShootableState.Moving;
            _rb.linearVelocity = direction.normalized * power * speed;
            return true;
        }

        private void FixedUpdate()
        {
            if (_state != ShootableState.Moving) return;

            // 停止判定
            if (_rb.linearVelocity.magnitude < stopThreshold)
            {
                _rb.linearVelocity = Vector2.zero;
                _state = ShootableState.Stopped;
                OnStopped?.Invoke(this);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_state != ShootableState.Moving) return;

            // 壁との衝突（反射）
            if (bumpType == BumpType.Reflect)
            {
                var normal = collision.contacts[0].normal;
                _rb.linearVelocity = Vector2.Reflect(_rb.linearVelocity, normal) * bounciness;
            }

            // 他のShootableとの衝突
            var otherShootable = collision.gameObject.GetComponent<Shootable>();
            if (otherShootable != null)
            {
                OnHitShootable?.Invoke(this, otherShootable);

                // 当てられた側の友情コンボを発動
                if (otherShootable.FriendshipComboType != FriendshipComboType.None)
                {
                    TriggerFriendshipCombo(otherShootable);
                }
            }

            // Damageableとの衝突
            var damageable = collision.gameObject.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attack);
            }
        }

        /// <summary>
        /// 友情コンボを発動
        /// </summary>
        private void TriggerFriendshipCombo(Shootable owner)
        {
            Vector2 center = owner.transform.position;

            switch (owner.FriendshipComboType)
            {
                case FriendshipComboType.Laser:
                    ExecuteLaser(center, owner.FriendshipComboDamage);
                    break;
                case FriendshipComboType.EnergyCircle:
                    ExecuteEnergyCircle(center, owner.FriendshipComboDamage, owner.FriendshipComboRange);
                    break;
                case FriendshipComboType.SpeedUp:
                    ExecuteSpeedUp();
                    break;
                case FriendshipComboType.Explosion:
                    ExecuteExplosion(center, owner.FriendshipComboDamage, owner.FriendshipComboRange);
                    break;
            }
        }

        private void ExecuteLaser(Vector2 center, int damage)
        {
            Vector2[] directions =
            {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
            };

            foreach (var dir in directions)
            {
                var hits = Physics2D.RaycastAll(center, dir, 10f);
                foreach (var hit in hits)
                {
                    var damageable = hit.collider.GetComponent<Damageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage);
                    }
                }
                Debug.DrawRay(center, dir * 10f, Color.yellow, 0.5f);
            }
        }

        private void ExecuteEnergyCircle(Vector2 center, int damage, float radius)
        {
            var colliders = Physics2D.OverlapCircleAll(center, radius);
            foreach (var col in colliders)
            {
                var damageable = col.GetComponent<Damageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
            }
        }

        private void ExecuteSpeedUp()
        {
            _rb.linearVelocity *= 1.5f;
        }

        private void ExecuteExplosion(Vector2 center, int damage, float radius)
        {
            var colliders = Physics2D.OverlapCircleAll(center, radius);
            foreach (var col in colliders)
            {
                var damageable = col.GetComponent<Damageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage * 2);
                }
            }
        }

        /// <summary>
        /// 消滅処理
        /// </summary>
        public void Destroy()
        {
            gameObject.SetActive(false);
        }
    }
}
