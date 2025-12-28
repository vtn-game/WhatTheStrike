#nullable enable

using System;
using UnityEngine;

namespace WhatTheStrike.Outgame
{
    /// <summary>
    /// 3D版ショット対象物の状態
    /// </summary>
    public enum Shootable3DState
    {
        Ready,    // 発射可能
        Moving,   // 移動中
        Stopped   // 停止
    }

    /// <summary>
    /// 3D版ショット対象物コンポーネント
    /// アウトゲームのステージ選択で使用
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Shootable3D : MonoBehaviour
    {
        [Header("物理設定")]
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float stopThreshold = 0.05f;
        [SerializeField] private float friction = 0.98f;

        [Header("パワー設定")]
        [SerializeField] private float minPower = 1f;
        [SerializeField] private float maxPower = 10f;
        [SerializeField] private float chargeTimeForMax = 2f;

        // 状態
        private Shootable3DState _state = Shootable3DState.Ready;
        private Rigidbody _rb = null!;
        private float _chargeStartTime;
        private float _currentChargePower;

        // イベント
        public event Action<Shootable3D>? OnStopped;
        public event Action<Shootable3D, Collider>? OnHitCollider;

        // プロパティ
        public Shootable3DState State => _state;
        public float CurrentChargePower => _currentChargePower;
        public float ChargeProgress => Mathf.Clamp01((Time.time - _chargeStartTime) / chargeTimeForMax);
        public float MinPower => minPower;
        public float MaxPower => maxPower;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }

        /// <summary>
        /// チャージ開始
        /// </summary>
        public void StartCharge()
        {
            if (_state != Shootable3DState.Ready) return;
            _chargeStartTime = Time.time;
            _currentChargePower = minPower;
        }

        /// <summary>
        /// チャージ更新（引っ張り中毎フレーム呼ぶ）
        /// </summary>
        /// <param name="pullDistance">引っ張り距離（正規化済み 0-1）</param>
        public void UpdateCharge(float pullDistance)
        {
            if (_state != Shootable3DState.Ready) return;

            float chargeTime = Time.time - _chargeStartTime;
            float timeProgress = Mathf.Clamp01(chargeTime / chargeTimeForMax);

            // 時間と距離の両方でパワーを決定
            float combinedProgress = Mathf.Max(timeProgress, pullDistance);
            _currentChargePower = Mathf.Lerp(minPower, maxPower, combinedProgress);
        }

        /// <summary>
        /// 発射
        /// </summary>
        /// <param name="direction">XZ平面上の方向</param>
        public void Shoot(Vector3 direction)
        {
            if (_state != Shootable3DState.Ready) return;

            _state = Shootable3DState.Moving;
            Vector3 velocity = direction.normalized * _currentChargePower * baseSpeed;
            velocity.y = 0; // Y方向は固定
            _rb.linearVelocity = velocity;
        }

        /// <summary>
        /// 即座に停止
        /// </summary>
        public void ForceStop()
        {
            _rb.linearVelocity = Vector3.zero;
            _state = Shootable3DState.Stopped;
            OnStopped?.Invoke(this);
        }

        /// <summary>
        /// 状態をリセット
        /// </summary>
        public void ResetState()
        {
            _rb.linearVelocity = Vector3.zero;
            _state = Shootable3DState.Ready;
            _currentChargePower = minPower;
        }

        private void FixedUpdate()
        {
            if (_state != Shootable3DState.Moving) return;

            // 摩擦適用
            _rb.linearVelocity *= friction;

            // 停止判定
            if (_rb.linearVelocity.magnitude < stopThreshold)
            {
                _rb.linearVelocity = Vector3.zero;
                _state = Shootable3DState.Stopped;
                OnStopped?.Invoke(this);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            OnHitCollider?.Invoke(this, collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            OnHitCollider?.Invoke(this, other);
        }
    }
}
