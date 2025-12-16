using UnityEngine;

namespace WhatTheStrike.Player
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
    /// プレイヤーの状態
    /// </summary>
    public enum PlayerState
    {
        Waiting,  // 待機中
        Ready,    // ショット可能
        Moving,   // 移動中
        Stopped   // 停止
    }

    /// <summary>
    /// プレイヤーキャラクターのデータと状態を管理
    /// </summary>
    public class Player : MonoBehaviour
    {
        [Header("ステータス")]
        [SerializeField] private int maxHp = 100;
        [SerializeField] private int attack = 10;
        [SerializeField] private float speed = 1.0f;
        [SerializeField] private BumpType bumpType = BumpType.Reflect;

        [Header("友情コンボ")]
        [SerializeField] private FriendshipCombo.FriendshipComboType friendshipComboType;

        // 現在のステータス
        private int currentHp;
        private PlayerState state = PlayerState.Waiting;

        // コンポーネント参照
        private Rigidbody2D rb;
        private CircleCollider2D circleCollider;

        // プロパティ
        public int CurrentHp => currentHp;
        public int MaxHp => maxHp;
        public int Attack => attack;
        public float Speed => speed;
        public BumpType BumpType => bumpType;
        public PlayerState State => state;
        public bool IsAlive => currentHp > 0;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            circleCollider = GetComponent<CircleCollider2D>();
            currentHp = maxHp;
        }

        /// <summary>
        /// 状態を変更する
        /// </summary>
        public void SetState(PlayerState newState)
        {
            state = newState;
        }

        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage)
        {
            currentHp = Mathf.Max(0, currentHp - damage);

            if (currentHp <= 0)
            {
                OnDeath();
            }
        }

        /// <summary>
        /// 発射する
        /// </summary>
        public void Shoot(Vector2 direction, float power)
        {
            if (state != PlayerState.Ready) return;

            state = PlayerState.Moving;
            rb.linearVelocity = direction.normalized * power * speed;
        }

        /// <summary>
        /// 停止判定
        /// </summary>
        public bool CheckStopped(float threshold = 0.1f)
        {
            if (state != PlayerState.Moving) return false;

            if (rb.linearVelocity.magnitude < threshold)
            {
                rb.linearVelocity = Vector2.zero;
                state = PlayerState.Stopped;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 友情コンボを発動する
        /// </summary>
        public void TriggerFriendshipCombo(Player triggeredBy)
        {
            // FriendshipComboManagerから処理を呼び出す
            var comboManager = FindFirstObjectByType<FriendshipCombo.FriendshipComboManager>();
            comboManager?.ExecuteCombo(this, friendshipComboType);
        }

        private void OnDeath()
        {
            // 死亡処理
            gameObject.SetActive(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 壁との衝突は物理エンジンが処理

            // 味方との衝突
            var otherPlayer = collision.gameObject.GetComponent<Player>();
            if (otherPlayer != null && state == PlayerState.Moving)
            {
                otherPlayer.TriggerFriendshipCombo(this);
            }
        }
    }
}
