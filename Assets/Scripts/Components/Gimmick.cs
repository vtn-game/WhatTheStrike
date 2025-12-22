using UnityEngine;

namespace WhatTheStrike.Components
{
    /// <summary>
    /// ギミック基底クラス
    /// Shootableと接触時に効果を発動するオブジェクトに付ける
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class Gimmick : MonoBehaviour
    {
        [Header("ギミック共通設定")]
        [SerializeField] protected bool isActive = true;

        /// <summary>
        /// Shootableがギミックに入った時
        /// </summary>
        protected abstract void OnShootableEnter(Shootable shootable);

        /// <summary>
        /// Shootableがギミック内にいる間（オプション）
        /// </summary>
        protected virtual void OnShootableStay(Shootable shootable) { }

        /// <summary>
        /// Shootableがギミックから出た時（オプション）
        /// </summary>
        protected virtual void OnShootableExit(Shootable shootable) { }

        /// <summary>
        /// ギミックの有効/無効を切り替え
        /// </summary>
        public void SetActive(bool active)
        {
            isActive = active;
        }

        public bool IsActive => isActive;

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!isActive) return;

            var shootable = other.GetComponent<Shootable>();
            if (shootable != null && shootable.State == ShootableState.Moving)
            {
                OnShootableEnter(shootable);
            }
        }

        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            if (!isActive) return;

            var shootable = other.GetComponent<Shootable>();
            if (shootable != null && shootable.State == ShootableState.Moving)
            {
                OnShootableStay(shootable);
            }
        }

        protected virtual void OnTriggerExit2D(Collider2D other)
        {
            if (!isActive) return;

            var shootable = other.GetComponent<Shootable>();
            if (shootable != null)
            {
                OnShootableExit(shootable);
            }
        }
    }
}
