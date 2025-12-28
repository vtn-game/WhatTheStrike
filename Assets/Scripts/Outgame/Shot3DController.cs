#nullable enable

using UnityEngine;

namespace WhatTheStrike.Outgame
{
    /// <summary>
    /// 3Dショット入力制御
    /// アウトゲームのステージ選択で使用
    /// </summary>
    public class Shot3DController : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private Shootable3D? shootable;
        [SerializeField] private LineRenderer? trajectoryLine;
        [SerializeField] private Camera? shotCamera;

        [Header("設定")]
        [SerializeField] private float maxPullDistance = 3f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float trajectoryLength = 5f;

        // 状態
        private bool _isDragging;
        private Vector3 _dragStartPos;
        private Vector3 _currentDragPos;
        private Plane _groundPlane;

        private void Start()
        {
            _groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (shotCamera == null)
            {
                shotCamera = Camera.main;
            }

            if (shootable != null)
            {
                shootable.OnStopped += OnShootableStopped;
            }
        }

        private void Update()
        {
            if (shootable == null || shotCamera == null) return;
            if (shootable.State != Shootable3DState.Ready) return;

            HandleInput();
            UpdateTrajectory();
        }

        private void HandleInput()
        {
            // タッチ/マウス開始
            if (Input.GetMouseButtonDown(0))
            {
                Vector3? hitPos = GetGroundHitPosition();
                if (hitPos.HasValue && IsShootableTouched(hitPos.Value))
                {
                    _isDragging = true;
                    _dragStartPos = shootable!.transform.position;
                    _currentDragPos = _dragStartPos;
                    shootable.StartCharge();
                }
            }

            // ドラッグ中
            if (Input.GetMouseButton(0) && _isDragging)
            {
                Vector3? hitPos = GetGroundHitPosition();
                if (hitPos.HasValue)
                {
                    _currentDragPos = hitPos.Value;
                    float normalizedDistance = GetNormalizedPullDistance();
                    shootable!.UpdateCharge(normalizedDistance);
                }
            }

            // リリース
            if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                _isDragging = false;
                ExecuteShot();
                HideTrajectory();
            }
        }

        private Vector3? GetGroundHitPosition()
        {
            if (shotCamera == null) return null;

            Ray ray = shotCamera.ScreenPointToRay(Input.mousePosition);
            if (_groundPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            return null;
        }

        private bool IsShootableTouched(Vector3 worldPos)
        {
            if (shootable == null) return false;

            var collider = shootable.GetComponent<Collider>();
            if (collider != null)
            {
                // XZ平面での距離チェック
                Vector3 shootablePos = shootable.transform.position;
                Vector2 shootable2D = new(shootablePos.x, shootablePos.z);
                Vector2 touch2D = new(worldPos.x, worldPos.z);
                return Vector2.Distance(shootable2D, touch2D) < 1.5f;
            }
            return false;
        }

        private float GetNormalizedPullDistance()
        {
            Vector3 pullVector = _dragStartPos - _currentDragPos;
            pullVector.y = 0;
            return Mathf.Clamp01(pullVector.magnitude / maxPullDistance);
        }

        private Vector3 GetShotDirection()
        {
            Vector3 pullVector = _dragStartPos - _currentDragPos;
            pullVector.y = 0;
            return pullVector.normalized;
        }

        private void ExecuteShot()
        {
            if (shootable == null) return;

            Vector3 pullVector = _dragStartPos - _currentDragPos;
            pullVector.y = 0;

            if (pullVector.magnitude < 0.1f) return;

            shootable.Shoot(pullVector.normalized);
        }

        private void UpdateTrajectory()
        {
            if (trajectoryLine == null || shootable == null) return;

            if (!_isDragging)
            {
                trajectoryLine.enabled = false;
                return;
            }

            trajectoryLine.enabled = true;
            Vector3 direction = GetShotDirection();
            float power = shootable.CurrentChargePower;

            // 軌道表示
            trajectoryLine.positionCount = 2;
            trajectoryLine.SetPosition(0, shootable.transform.position);
            trajectoryLine.SetPosition(1, shootable.transform.position + direction * power * trajectoryLength * 0.1f);
        }

        private void HideTrajectory()
        {
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }
        }

        private void OnShootableStopped(Shootable3D shootable)
        {
            // OutgameManagerが処理するため、ここでは何もしない
        }

        /// <summary>
        /// Shootableを設定
        /// </summary>
        public void SetShootable(Shootable3D newShootable)
        {
            if (shootable != null)
            {
                shootable.OnStopped -= OnShootableStopped;
            }
            shootable = newShootable;
            if (shootable != null)
            {
                shootable.OnStopped += OnShootableStopped;
            }
        }

        /// <summary>
        /// 引っ張り情報を取得（UI用）
        /// </summary>
        public (Vector3 direction, float power, float normalizedDistance) GetPullInfo()
        {
            if (!_isDragging || shootable == null)
            {
                return (Vector3.zero, 0f, 0f);
            }

            return (GetShotDirection(), shootable.CurrentChargePower, GetNormalizedPullDistance());
        }

        private void OnDestroy()
        {
            if (shootable != null)
            {
                shootable.OnStopped -= OnShootableStopped;
            }
        }
    }
}
