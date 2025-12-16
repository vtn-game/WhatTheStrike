using UnityEngine;
using System.Collections;

namespace WhatTheStrike.FriendshipCombo
{
    /// <summary>
    /// スピードアップ - 移動速度上昇
    /// </summary>
    public class SpeedUp : FriendshipComboBase
    {
        [Header("スピードアップ設定")]
        [SerializeField] private float speedMultiplier = 1.5f;

        private Player.Player targetPlayer;
        private float originalSpeed;

        public override void Execute(Player.Player owner)
        {
            this.owner = owner;

            // 移動中のプレイヤー（ぶつかってきた方）を探す
            var allPlayers = FindObjectsByType<Player.Player>(FindObjectsSortMode.None);

            foreach (var player in allPlayers)
            {
                if (player != owner && player.State == Player.PlayerState.Moving)
                {
                    targetPlayer = player;
                    break;
                }
            }

            if (targetPlayer != null)
            {
                StartCoroutine(ApplySpeedUp());
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator ApplySpeedUp()
        {
            // 速度アップを適用
            var rb = targetPlayer.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 currentVelocity = rb.linearVelocity;
                rb.linearVelocity = currentVelocity * speedMultiplier;
            }

            // エフェクト表示
            CreateSpeedUpVisual();

            yield return new WaitForSeconds(duration);

            Destroy(gameObject);
        }

        private void CreateSpeedUpVisual()
        {
            // スピードラインエフェクト
            var effectObj = new GameObject("SpeedUpEffect");
            effectObj.transform.SetParent(targetPlayer.transform);
            effectObj.transform.localPosition = Vector3.zero;

            var sr = effectObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(0f, 1f, 0f, 0.7f);

            // フェードアウト
            StartCoroutine(FadeEffect(sr));
        }

        private IEnumerator FadeEffect(SpriteRenderer sr)
        {
            float elapsed = 0f;

            while (elapsed < duration && sr != null)
            {
                elapsed += Time.deltaTime;
                float alpha = 0.7f * (1f - elapsed / duration);
                sr.color = new Color(0f, 1f, 0f, alpha);
                yield return null;
            }
        }
    }
}
