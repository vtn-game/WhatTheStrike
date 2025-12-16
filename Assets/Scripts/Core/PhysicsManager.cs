using UnityEngine;

namespace WhatTheStrike.Core
{
    /// <summary>
    /// 物理演算の管理
    /// </summary>
    public class PhysicsManager : MonoBehaviour
    {
        // シングルトン
        private static PhysicsManager instance;
        public static PhysicsManager Instance => instance;

        [Header("物理設定")]
        [SerializeField] private float friction = 0.98f;
        [SerializeField] private float stopThreshold = 0.1f;
        [SerializeField] private float bounciness = 0.8f;

        [Header("レイヤー設定")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private LayerMask gimmickLayer;

        // プロパティ
        public float Friction => friction;
        public float StopThreshold => stopThreshold;
        public float Bounciness => bounciness;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            ConfigurePhysics();
        }

        /// <summary>
        /// 物理設定の初期化
        /// </summary>
        private void ConfigurePhysics()
        {
            // 2D物理設定
            Physics2D.gravity = Vector2.zero; // 重力なし（トップダウン視点）

            // 物理マテリアル設定は各オブジェクトで行う
        }

        /// <summary>
        /// 摩擦を適用（カスタム減速）
        /// </summary>
        public void ApplyFriction(Rigidbody2D rb)
        {
            if (rb == null) return;

            rb.linearVelocity *= friction;

            // 停止判定
            if (rb.linearVelocity.magnitude < stopThreshold)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        /// <summary>
        /// 反射ベクトルを計算
        /// </summary>
        public Vector2 CalculateReflection(Vector2 velocity, Vector2 normal)
        {
            return Vector2.Reflect(velocity, normal) * bounciness;
        }

        /// <summary>
        /// 貫通処理
        /// </summary>
        public void HandlePiercing(Player.Player player, Enemy.Enemy enemy)
        {
            // 貫通時は反射せず、ダメージのみ
            enemy.TakeDamage(player.Attack);

            // 軽微な減速
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity *= 0.95f;
            }
        }

        /// <summary>
        /// 衝突物の種類を判定
        /// </summary>
        public CollisionType GetCollisionType(GameObject obj)
        {
            if (obj.GetComponent<Player.Player>() != null)
                return CollisionType.Player;

            if (obj.GetComponent<Enemy.Enemy>() != null)
                return CollisionType.Enemy;

            if (obj.layer == LayerMask.NameToLayer("Wall"))
                return CollisionType.Wall;

            if (obj.GetComponent<Gimmicks.GimmickBase>() != null)
                return CollisionType.Gimmick;

            return CollisionType.Unknown;
        }

        /// <summary>
        /// 物理マテリアルを作成
        /// </summary>
        public PhysicsMaterial2D CreateBouncyMaterial()
        {
            var material = new PhysicsMaterial2D("Bouncy");
            material.bounciness = bounciness;
            material.friction = 0f;
            return material;
        }
    }

    /// <summary>
    /// 衝突タイプ
    /// </summary>
    public enum CollisionType
    {
        Unknown,
        Player,
        Enemy,
        Wall,
        Gimmick
    }
}
