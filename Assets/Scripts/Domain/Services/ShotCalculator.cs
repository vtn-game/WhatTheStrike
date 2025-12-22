using System;
using UnityEngine;

namespace WhatTheStrike.Domain.Services
{
    /// <summary>
    /// ショット計算のドメインサービス
    /// </summary>
    public static class ShotCalculator
    {
        public const float MaxPullDistance = 3f;
        public const float MinPower = 5f;
        public const float MaxPower = 20f;
        public const float MinPullThreshold = 0.1f;

        /// <summary>
        /// 引っ張り情報からショットパラメータを計算
        /// </summary>
        public static (Vector2 direction, float power) Calculate(Vector2 start, Vector2 current)
        {
            Vector2 pullVector = start - current;
            float pullDistance = pullVector.magnitude;

            if (pullDistance < MinPullThreshold)
            {
                return (Vector2.up, 0f);
            }

            pullDistance = Mathf.Min(pullDistance, MaxPullDistance);
            Vector2 direction = pullVector.normalized;
            float power = Mathf.Lerp(MinPower, MaxPower, pullDistance / MaxPullDistance);

            return (direction, power);
        }

        /// <summary>
        /// ショットが有効か判定
        /// </summary>
        public static bool IsValidShot(Vector2 start, Vector2 current)
        {
            return (start - current).magnitude >= MinPullThreshold;
        }

        /// <summary>
        /// 引っ張り距離を取得（UI表示用）
        /// </summary>
        public static float GetPullDistance(Vector2 start, Vector2 current)
        {
            return Mathf.Min((start - current).magnitude, MaxPullDistance);
        }
    }
}
