using UnityEngine;
using System.Collections;

namespace WhatTheStrike.FriendshipCombo
{
    /// <summary>
    /// 爆発 - 範囲ダメージ
    /// </summary>
    public class Explosion : FriendshipComboBase
    {
        [Header("爆発設定")]
        [SerializeField] private float radius = 3f;
        [SerializeField] private float knockbackForce = 5f;

        public override void Execute(Player.Player owner)
        {
            this.owner = owner;
            transform.position = owner.transform.position;
            StartCoroutine(ExplodeEffect());
        }

        private IEnumerator ExplodeEffect()
        {
            // 爆発ダメージ
            DealExplosionDamage();

            // ビジュアルエフェクト
            CreateExplosionVisual();

            yield return new WaitForSeconds(duration);

            Destroy(gameObject);
        }

        private void DealExplosionDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);

            foreach (var hit in hits)
            {
                // 敵へのダメージ
                var enemy = hit.GetComponent<Enemy.Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(baseDamage);
                }

                // ノックバック
                var rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null && hit.GetComponent<Player.Player>() != owner)
                {
                    Vector2 direction = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }

        private void CreateExplosionVisual()
        {
            var explosionObj = new GameObject("ExplosionVisual");
            explosionObj.transform.SetParent(transform);
            explosionObj.transform.localPosition = Vector3.zero;

            var sr = explosionObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite();
            sr.color = new Color(1f, 0.5f, 0f, 0.8f);

            // 拡大アニメーション
            StartCoroutine(ExpandAnimation(explosionObj.transform, sr));
        }

        private IEnumerator ExpandAnimation(Transform visual, SpriteRenderer sr)
        {
            float elapsed = 0f;
            float expandDuration = duration * 0.3f;

            // 拡大フェーズ
            while (elapsed < expandDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / expandDuration;
                float scale = Mathf.Lerp(0f, radius * 2f, t);
                visual.localScale = Vector3.one * scale;
                yield return null;
            }

            // フェードアウトフェーズ
            elapsed = 0f;
            float fadeDuration = duration * 0.7f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 0.8f * (1f - elapsed / fadeDuration);
                sr.color = new Color(1f, 0.5f, 0f, alpha);
                yield return null;
            }
        }

        private Sprite CreateCircleSprite()
        {
            int size = 64;
            var texture = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            float maxRadius = size / 2f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist < maxRadius)
                    {
                        float alpha = 1f - (dist / maxRadius);
                        texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
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
