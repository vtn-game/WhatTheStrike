using UnityEngine;

namespace WhatTheStrike.Gimmicks
{
    /// <summary>
    /// ギミック基底クラス
    /// </summary>
    public abstract class GimmickBase : MonoBehaviour
    {
        [Header("ギミック共通設定")]
        [SerializeField] protected bool isActive = true;

        /// <summary>
        /// プレイヤーがギミックに入った時
        /// </summary>
        public abstract void OnPlayerEnter(Player.Player player);

        /// <summary>
        /// プレイヤーがギミック内にいる間
        /// </summary>
        public virtual void OnPlayerStay(Player.Player player) { }

        /// <summary>
        /// プレイヤーがギミックから出た時
        /// </summary>
        public virtual void OnPlayerExit(Player.Player player) { }

        /// <summary>
        /// ギミックの有効/無効を切り替え
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive) return;

            var player = other.GetComponent<Player.Player>();
            if (player != null)
            {
                OnPlayerEnter(player);
            }
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (!isActive) return;

            var player = other.GetComponent<Player.Player>();
            if (player != null)
            {
                OnPlayerStay(player);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!isActive) return;

            var player = other.GetComponent<Player.Player>();
            if (player != null)
            {
                OnPlayerExit(player);
            }
        }
    }
}
