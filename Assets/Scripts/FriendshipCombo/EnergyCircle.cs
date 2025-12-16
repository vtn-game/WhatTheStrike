using UnityEngine;
using System.Collections;

namespace WhatTheStrike.FriendshipCombo
{
    /// <summary>
    /// エナジーサークル（エナサー） - 周囲に継続ダメージ
    /// </summary>
    public class EnergyCircle : FriendshipComboBase
    {
        [Header("エナジーサークル設定")]
        [SerializeField] private float radius = 3f;
        [SerializeField] private float damageInterval = 0.3f;
        [SerializeField] private int tickDamage = 5;

        public override void Execute(Player.Player owner)
        {
            this.owner = owner;
            transform.position = owner.transform.position;
            StartCoroutine(EnergyCircleEffect());
        }

        private IEnumerator EnergyCircleEffect()
        {
            float elapsed = 0f;
            float lastDamageTime = 0f;

            // ビジュアル生成
            var visual = CreateCircleVisual();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // オーナーに追従
                transform.position = owner.transform.position;

                // ダメージティック
                if (elapsed - lastDamageTime >= damageInterval)
                {
                    DealDamageToEnemiesInRange();
                    lastDamageTime = elapsed;
                }

                // ビジュアル更新（脈動エフェクト）
                float scale = 1f + 0.1f * Mathf.Sin(elapsed * 10f);
                visual.transform.localScale = Vector3.one * radius * 2f * scale;

                yield return null;
            }

            Destroy(gameObject);
        }

        private void DealDamageToEnemiesInRange()
        {
            var enemies = GetEnemiesInRange(transform.position, radius);

            foreach (var enemy in enemies)
            {
                enemy.TakeDamage(tickDamage);
            }
        }

        private GameObject CreateCircleVisual()
        {
            var circleObj = new GameObject("EnergyCircleVisual");
            circleObj.transform.SetParent(transform);
            circleObj.transform.localPosition = Vector3.zero;

            // スプライトレンダラーで円を表示
            var sr = circleObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(0f, 1f, 1f, 0.5f);
            circleObj.transform.localScale = Vector3.one * radius * 2f;

            return circleObj;
        }

        private Sprite CreateCircleSprite()
        {
            // シンプルな円形テクスチャを生成
            int size = 64;
            var texture = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            float maxRadius = size / 2f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist < maxRadius && dist > maxRadius * 0.8f)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f);
        }
    }
}
