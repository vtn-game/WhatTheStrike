#nullable enable

using System;
using UnityEngine;

namespace WhatTheStrike.Outgame
{
    /// <summary>
    /// 3Dステージボタン
    /// Shootable3Dが範囲内で停止するとステージ遷移可能
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StageButton3D : MonoBehaviour
    {
        [Header("ステージ情報")]
        [SerializeField] private string sceneName = "";
        [SerializeField] private int stageIndex = -1;

        [Header("ビジュアル")]
        [SerializeField] private SpriteRenderer? characterImageRenderer;
        [SerializeField] private Material? clearedMaterial;
        [SerializeField] private Material? normalMaterial;

        [Header("当たり判定")]
        [SerializeField] private float exactStopRadius = 0.5f;

        // 状態
        private bool _isCleared;
        private Collider _collider = null!;
        private bool _shootableInRange;
        private Shootable3D? _lastShootableInRange;

        // イベント
        public event Action<StageButton3D, bool>? OnActivated;

        // プロパティ
        public string SceneName => sceneName;
        public int StageIndex => stageIndex;
        public bool IsCleared
        {
            get => _isCleared;
            set
            {
                _isCleared = value;
                UpdateVisual();
            }
        }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }

        private void Start()
        {
            UpdateVisual();
        }

        /// <summary>
        /// ステージ情報を設定
        /// </summary>
        public void Setup(string sceneName, int stageIndex, Sprite? characterImage, bool isCleared)
        {
            this.sceneName = sceneName;
            this.stageIndex = stageIndex;
            _isCleared = isCleared;

            if (characterImageRenderer != null && characterImage != null)
            {
                characterImageRenderer.sprite = characterImage;
            }

            UpdateVisual();
        }

        private void UpdateVisual()
        {
            // クリア済みは見た目を変更
            if (characterImageRenderer != null)
            {
                var material = _isCleared ? clearedMaterial : normalMaterial;
                if (material != null)
                {
                    characterImageRenderer.material = material;
                }

                // クリア済みは半透明に
                Color color = characterImageRenderer.color;
                color.a = _isCleared ? 0.5f : 1f;
                characterImageRenderer.color = color;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var shootable = other.GetComponent<Shootable3D>();
            if (shootable != null)
            {
                _shootableInRange = true;
                _lastShootableInRange = shootable;
                shootable.OnStopped += OnShootableStopped;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var shootable = other.GetComponent<Shootable3D>();
            if (shootable != null && shootable == _lastShootableInRange)
            {
                _shootableInRange = false;
                shootable.OnStopped -= OnShootableStopped;
                _lastShootableInRange = null;
            }
        }

        private void OnShootableStopped(Shootable3D shootable)
        {
            shootable.OnStopped -= OnShootableStopped;

            if (!_shootableInRange) return;

            // クリア済みの場合はぴったり停止チェック
            if (_isCleared)
            {
                float distance = Vector3.Distance(
                    new Vector3(transform.position.x, 0, transform.position.z),
                    new Vector3(shootable.transform.position.x, 0, shootable.transform.position.z)
                );

                if (distance <= exactStopRadius)
                {
                    OnActivated?.Invoke(this, true);
                }
            }
            else
            {
                // 未クリアは範囲内停止でOK
                OnActivated?.Invoke(this, false);
            }

            _shootableInRange = false;
            _lastShootableInRange = null;
        }

        private void OnDrawGizmosSelected()
        {
            // 範囲を表示
            Gizmos.color = _isCleared ? Color.gray : Color.green;
            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
            }

            // ぴったり停止範囲を表示（クリア済み用）
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, exactStopRadius);
        }

        private void OnDestroy()
        {
            if (_lastShootableInRange != null)
            {
                _lastShootableInRange.OnStopped -= OnShootableStopped;
            }
        }
    }
}
