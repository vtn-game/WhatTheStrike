using UnityEngine;
using WhatTheStrike.Domain.Services;

namespace WhatTheStrike.Components
{
    /// <summary>
    /// 引っ張りUIコンポーネント
    /// Shootableを起点としてドラッグ方向と逆向きに矢印を表示する
    /// </summary>
    public class PullArrowUI : MonoBehaviour
    {
        [Header("矢印設定")]
        [SerializeField] private SpriteRenderer? arrowSprite;
        [SerializeField] private float baseLength = 1f;
        [SerializeField] private float lengthMultiplier = 2f;

        // 表示対象のShootable
        private Shootable? _targetShootable;
        private bool _isVisible = false;

        private void Awake()
        {
            Hide();
        }

        /// <summary>
        /// 矢印UIを表示する
        /// </summary>
        public void Show(Shootable target)
        {
            _targetShootable = target;
            _isVisible = true;

            if (arrowSprite != null)
            {
                arrowSprite.enabled = true;
            }
        }

        /// <summary>
        /// 矢印UIを非表示にする
        /// </summary>
        public void Hide()
        {
            _isVisible = false;
            _targetShootable = null;

            if (arrowSprite != null)
            {
                arrowSprite.enabled = false;
            }
        }

        /// <summary>
        /// 矢印UIを更新する
        /// </summary>
        /// <param name="direction">発射方向（ドラッグ方向の逆）</param>
        /// <param name="pullDistance">引っ張り距離（0〜MaxPullDistance）</param>
        public void UpdateArrow(Vector2 direction, float pullDistance)
        {
            if (!_isVisible || _targetShootable == null || arrowSprite == null)
            {
                return;
            }

            // 矢印の位置をShootableの位置に設定
            transform.position = _targetShootable.transform.position;

            // 発射方向に向けて回転（Z軸回転）
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // 引っ張り距離に応じて矢印を伸ばす
            float normalizedPull = pullDistance / ShotCalculator.MaxPullDistance;
            float arrowLength = baseLength + normalizedPull * lengthMultiplier;

            // X軸方向にスケール（矢印が右向きを想定）
            transform.localScale = new Vector3(arrowLength, 1f, 1f);
        }
    }
}
