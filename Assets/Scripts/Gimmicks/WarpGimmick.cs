#nullable enable

using UnityEngine;
using WhatTheStrike.Components;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// ワープギミック
    /// Shootableが入ると出口位置にテレポートする
    /// 自身もShootableコンポーネントを付けることで発射可能
    /// NOTE: Shootableコンポーネントを付けるとターン順に含まれる
    /// </summary>
    public class WarpGimmick : Gimmick
    {
        [Header("ワープ設定")]
        [SerializeField] private Transform? exitPoint;
        [SerializeField] private bool preserveVelocity = true;

        [Header("移動中のワープ機能")]
        [SerializeField] private bool disableWarpWhileMoving = true;

        private Shootable? _shootableComponent;

        public Transform? ExitPoint => exitPoint;

        private void Awake()
        {
            _shootableComponent = GetComponent<Shootable>();
        }

        /// <summary>
        /// ワープ機能が現在有効かどうか
        /// 自身がMoving状態の場合は無効
        /// </summary>
        private bool IsWarpFunctional
        {
            get
            {
                if (!isActive) return false;
                if (disableWarpWhileMoving && _shootableComponent != null)
                {
                    return _shootableComponent.State != ShootableState.Moving;
                }
                return true;
            }
        }

        /// <summary>
        /// 出口位置を設定
        /// </summary>
        public void SetExitPoint(Transform exit)
        {
            exitPoint = exit;
        }

        /// <summary>
        /// Shootableがワープに入った時の処理
        /// </summary>
        protected override void OnShootableEnter(Shootable shootable)
        {
            if (!IsWarpFunctional) return;
            if (exitPoint == null) return;

            // 自分自身（Shootableとしての自分）がワープに入った場合は無視
            if (_shootableComponent != null && shootable == _shootableComponent) return;

            ExecuteWarp(shootable);
        }

        /// <summary>
        /// ワープを実行
        /// </summary>
        private void ExecuteWarp(Shootable shootable)
        {
            if (exitPoint == null) return;

            var rb = shootable.GetComponent<Rigidbody2D>();
            if (rb == null) return;

            Vector2 velocity = rb.linearVelocity;

            // 出口位置にテレポート
            shootable.transform.position = exitPoint.position;

            // 速度を維持または0にリセット
            if (preserveVelocity)
            {
                rb.linearVelocity = velocity;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }

            Debug.Log($"Warp: {shootable.name} teleported to {exitPoint.position}");
        }
    }
}
