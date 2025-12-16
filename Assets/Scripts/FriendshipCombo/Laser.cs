using UnityEngine;
using System.Collections;

namespace WhatTheStrike.FriendshipCombo
{
    /// <summary>
    /// レーザー - 直線状にダメージ
    /// </summary>
    public class Laser : FriendshipComboBase
    {
        [Header("レーザー設定")]
        [SerializeField] private float range = 10f;
        [SerializeField] private float width = 0.5f;
        [SerializeField] private int laserCount = 4; // 発射方向数
        [SerializeField] private bool is8Direction = true;

        public override void Execute(Player.Player owner)
        {
            this.owner = owner;
            StartCoroutine(FireLasers());
        }

        private IEnumerator FireLasers()
        {
            Vector2 origin = owner.transform.position;
            float angleStep = 360f / (is8Direction ? 8 : laserCount);

            for (int i = 0; i < (is8Direction ? 8 : laserCount); i++)
            {
                float angle = i * angleStep;
                Vector2 direction = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)
                );

                FireSingleLaser(origin, direction);
            }

            // エフェクト表示時間
            yield return new WaitForSeconds(duration);

            Destroy(gameObject);
        }

        private void FireSingleLaser(Vector2 origin, Vector2 direction)
        {
            // レイキャストで敵を検出
            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                origin,
                new Vector2(width, width),
                0f,
                direction,
                range
            );

            foreach (var hit in hits)
            {
                var enemy = hit.collider.GetComponent<Enemy.Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(baseDamage);
                }
            }

            // ビジュアル用のラインレンダラー生成
            CreateLaserVisual(origin, direction);
        }

        private void CreateLaserVisual(Vector2 origin, Vector2 direction)
        {
            var laserObj = new GameObject("LaserBeam");
            laserObj.transform.SetParent(transform);

            var lineRenderer = laserObj.AddComponent<LineRenderer>();
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, origin + direction * range);

            // マテリアル設定（デフォルト）
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.red;

            // フェードアウト
            StartCoroutine(FadeLaser(lineRenderer));
        }

        private IEnumerator FadeLaser(LineRenderer lr)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - (elapsed / duration);

                lr.startColor = new Color(1f, 1f, 0f, alpha);
                lr.endColor = new Color(1f, 0f, 0f, alpha);

                yield return null;
            }
        }
    }
}
