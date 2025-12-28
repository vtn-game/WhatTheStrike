#nullable enable

using UnityEngine;
using WhatTheStrike.Components;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// 壁の向き（転送方向）
    /// </summary>
    public enum WallDirection
    {
        Top,    // 上の壁（下に転送）
        Bottom, // 下の壁（上に転送）
        Left,   // 左の壁（右に転送）
        Right   // 右の壁（左に転送）
    }

    /// <summary>
    /// テレポート壁ギミック
    /// ぶつかったShootableを反対側の壁に転送する
    /// 自身もShootableコンポーネントを付けることで発射可能
    /// NOTE: Shootableコンポーネントを付けるとターン順に含まれる
    /// </summary>
    public class TeleportWallGimmick : Gimmick
    {
        [Header("壁設定")]
        [SerializeField] private WallDirection wallDirection = WallDirection.Top;
        [SerializeField] private Transform? teleportDestination;
        [SerializeField] private bool preserveVelocity = true;

        [Header("自動転送先計算")]
        [Tooltip("trueの場合、teleportDestinationを無視して反対側の位置を自動計算")]
        [SerializeField] private bool autoCalculateDestination = true;
        [SerializeField] private float teleportDistance = 10f;

        [Header("Shootable機能")]
        [SerializeField] private bool disableWhileMoving = true;

        private Shootable? _shootableComponent;

        private void Awake()
        {
            _shootableComponent = GetComponent<Shootable>();
        }

        /// <summary>
        /// ギミックが現在有効かどうか
        /// </summary>
        private bool IsGimmickFunctional
        {
            get
            {
                if (!isActive) return false;
                if (disableWhileMoving && _shootableComponent != null)
                {
                    return _shootableComponent.State != ShootableState.Moving;
                }
                return true;
            }
        }

        /// <summary>
        /// Shootableが壁に当たった時の処理
        /// </summary>
        protected override void OnShootableEnter(Shootable shootable)
        {
            if (!IsGimmickFunctional) return;

            // 自分自身（Shootableとしての自分）が当たった場合は無視
            if (_shootableComponent != null && shootable == _shootableComponent) return;

            ExecuteTeleport(shootable);
        }

        /// <summary>
        /// テレポートを実行
        /// </summary>
        private void ExecuteTeleport(Shootable shootable)
        {
            var rb = shootable.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            Vector2 velocity = rb.linearVelocity;
            Vector2 destination = CalculateDestination(shootable.transform.position);

            // 転送先に移動
            shootable.transform.position = destination;

            // 速度を維持または反転
            if (preserveVelocity)
            {
                rb.linearVelocity = velocity;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            Debug.Log($"TeleportWall: {shootable.name} teleported to {destination}");
        }

        /// <summary>
        /// 転送先を計算
        /// </summary>
        private Vector2 CalculateDestination(Vector2 currentPosition)
        {
            if (!autoCalculateDestination && teleportDestination != null)
            {
                return teleportDestination.position;
            }

            // 反対側の位置を自動計算
            Vector2 offset = wallDirection switch
            {
                WallDirection.Top => new Vector2(0, -teleportDistance),
                WallDirection.Bottom => new Vector2(0, teleportDistance),
                WallDirection.Left => new Vector2(teleportDistance, 0),
                WallDirection.Right => new Vector2(-teleportDistance, 0),
                _ => Vector2.zero
            };

            // 壁の位置を基準に転送先を計算
            return (Vector2)transform.position + offset;
        }

        /// <summary>
        /// 転送先を設定
        /// </summary>
        public void SetDestination(Transform destination)
        {
            teleportDestination = destination;
            autoCalculateDestination = false;
        }

        /// <summary>
        /// 壁の向きを設定
        /// </summary>
        public void SetWallDirection(WallDirection direction)
        {
            wallDirection = direction;
        }

        private void OnDrawGizmosSelected()
        {
            // 転送先を表示
            Vector2 destination;
            if (!autoCalculateDestination && teleportDestination != null)
            {
                destination = teleportDestination.position;
            }
            else
            {
                destination = CalculateDestination(transform.position);
            }

            // 壁から転送先への線
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, destination);

            // 転送先のマーカー
            Gizmos.color = new Color(0, 1, 1, 0.5f);
            Gizmos.DrawSphere(destination, 0.3f);

            // 壁の向きを示す矢印
            Gizmos.color = Color.yellow;
            Vector3 arrowDirection = wallDirection switch
            {
                WallDirection.Top => Vector3.down,
                WallDirection.Bottom => Vector3.up,
                WallDirection.Left => Vector3.right,
                WallDirection.Right => Vector3.left,
                _ => Vector3.zero
            };
            Gizmos.DrawRay(transform.position, arrowDirection * 2f);
        }
    }
}
