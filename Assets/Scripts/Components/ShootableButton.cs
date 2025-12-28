#nullable enable

using UnityEngine;
using UnityEngine.Events;

namespace WhatTheStrike.Components
{
    /// <summary>
    /// アウトゲーム用のショット可能なボタン
    /// Shootableがこのボタンの範囲内で停止するとイベントが発火する
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShootableButton : MonoBehaviour
    {
        [Header("ボタン設定")]
        [SerializeField] private UnityEvent onButtonActivated = new();

        [Header("範囲設定")]
        [Tooltip("ボタンの有効範囲（Collider2Dを使用する場合はこの値は無視されます）")]
        [SerializeField] private float activationRadius = 1f;
        [SerializeField] private bool useColliderAsRange = true;

        private Collider2D? _collider;

        public UnityEvent OnButtonActivated => onButtonActivated;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }
        }

        /// <summary>
        /// Shootableが範囲内で停止したかチェック
        /// </summary>
        public bool CheckActivation(Shootable shootable)
        {
            if (shootable.State != ShootableState.Stopped) return false;

            if (IsInRange(shootable.transform.position))
            {
                ActivateButton();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 位置が範囲内かどうか判定
        /// </summary>
        private bool IsInRange(Vector2 position)
        {
            if (useColliderAsRange && _collider != null)
            {
                return _collider.OverlapPoint(position);
            }
            return Vector2.Distance(transform.position, position) <= activationRadius;
        }

        /// <summary>
        /// ボタンを発火
        /// </summary>
        private void ActivateButton()
        {
            Debug.Log($"ShootableButton activated: {gameObject.name}");
            onButtonActivated?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var shootable = other.GetComponent<Shootable>();
            if (shootable == null) return;

            // Shootableの停止イベントを購読
            shootable.OnStopped += OnShootableStopped;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var shootable = other.GetComponent<Shootable>();
            if (shootable == null) return;

            // 購読解除
            shootable.OnStopped -= OnShootableStopped;
        }

        /// <summary>
        /// Shootableが停止した時の処理
        /// </summary>
        private void OnShootableStopped(Shootable shootable)
        {
            // 購読解除
            shootable.OnStopped -= OnShootableStopped;

            // 範囲内で停止したらボタン発火
            if (IsInRange(shootable.transform.position))
            {
                ActivateButton();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // 有効範囲を表示（Colliderを使わない場合のみ）
            if (!useColliderAsRange)
            {
                Gizmos.color = new Color(0, 1, 0, 0.3f);
                Gizmos.DrawSphere(transform.position, activationRadius);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, activationRadius);
            }
        }
    }
}
