using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using WhatTheStrike.Components;
using WhatTheStrike.Domain.Services;

namespace WhatTheStrike.Core
{
    /// <summary>
    /// ターン制御と入力処理
    /// </summary>
    public class TurnController : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private float maxPullDistance = 3f;
        [SerializeField] private LineRenderer? trajectoryLine;

        // 状態
        private List<Shootable> _shootables = new();
        private int _currentIndex = 0;
        private int _shootCountInTurn = 0;
        private bool _isDragging = false;
        private Vector2 _dragStartPos;

        // プロパティ
        public Shootable? CurrentShootable =>
            _currentIndex < _shootables.Count ? _shootables[_currentIndex] : null;
        public IReadOnlyList<Shootable> Shootables => _shootables;

        private void Start()
        {
            RegisterShootables();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
        }

        /// <summary>
        /// シーン上のShootableを登録
        /// </summary>
        private void RegisterShootables()
        {
            _shootables = FindObjectsByType<Shootable>(FindObjectsSortMode.InstanceID).ToList();

            foreach (var shootable in _shootables)
            {
                shootable.OnStopped += OnShootableStopped;
            }

            // 最初のShootableを準備状態に
            if (_shootables.Count > 0)
            {
                _shootables[0].SetState(ShootableState.Ready);
            }
        }

        private void Update()
        {
            if (GameManager.Instance?.State != GameState.Playing) return;

            HandleInput();
            UpdateTrajectory();
        }

        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            var current = CurrentShootable;
            if (current == null || current.State != ShootableState.Ready) return;

            // タッチ/マウス開始
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (IsShootableTouched(current, worldPos))
                {
                    _isDragging = true;
                    _dragStartPos = current.transform.position;
                }
            }

            // ドラッグ中
            if (Input.GetMouseButton(0) && _isDragging)
            {
                // 軌道表示はUpdateTrajectoryで処理
            }

            // リリース
            if (Input.GetMouseButtonUp(0) && _isDragging)
            {
                _isDragging = false;
                ExecuteShot(current);
                HideTrajectory();
            }
        }

        /// <summary>
        /// Shootableがタッチされたか判定
        /// </summary>
        private bool IsShootableTouched(Shootable shootable, Vector2 worldPos)
        {
            var collider = shootable.GetComponent<Collider2D>();
            if (collider != null)
            {
                return collider.OverlapPoint(worldPos);
            }
            return Vector2.Distance(worldPos, shootable.transform.position) < 1f;
        }

        /// <summary>
        /// ショット実行
        /// </summary>
        private void ExecuteShot(Shootable shootable)
        {
            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (!ShotCalculator.IsValidShot(_dragStartPos, currentPos)) return;

            var (direction, power) = ShotCalculator.Calculate(_dragStartPos, currentPos);
            shootable.Shoot(direction, power);
        }

        /// <summary>
        /// 軌道表示更新
        /// </summary>
        private void UpdateTrajectory()
        {
            if (!_isDragging || trajectoryLine == null) return;

            var current = CurrentShootable;
            if (current == null) return;

            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var (direction, _) = ShotCalculator.Calculate(_dragStartPos, currentPos);
            float pullDistance = ShotCalculator.GetPullDistance(_dragStartPos, currentPos);

            trajectoryLine.enabled = true;
            trajectoryLine.positionCount = 2;
            trajectoryLine.SetPosition(0, current.transform.position);
            trajectoryLine.SetPosition(1, (Vector2)current.transform.position + direction * pullDistance * 2);
        }

        /// <summary>
        /// 軌道を非表示
        /// </summary>
        private void HideTrajectory()
        {
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }
        }

        /// <summary>
        /// Shootable停止時
        /// </summary>
        private void OnShootableStopped(Shootable shootable)
        {
            _shootCountInTurn++;

            // Shootable全滅チェック
            GameManager.Instance?.CheckShootablesAlive(_shootables);

            // 次のShootableへ
            MoveToNextShootable();
        }

        /// <summary>
        /// 次のShootableへ移行
        /// </summary>
        private void MoveToNextShootable()
        {
            // 生存しているShootableを探す
            int startIndex = _currentIndex;
            do
            {
                _currentIndex = (_currentIndex + 1) % _shootables.Count;

                if (_shootables[_currentIndex].IsAlive)
                {
                    _shootables[_currentIndex].SetState(ShootableState.Ready);

                    // 全員が1回ずつ発射したらターン終了
                    if (_shootCountInTurn >= GetAliveShootableCount())
                    {
                        _shootCountInTurn = 0;
                        GameManager.Instance?.EndTurn();
                    }
                    return;
                }
            } while (_currentIndex != startIndex);

            // 全員死亡（GameManagerでチェック済み）
        }

        /// <summary>
        /// 生存Shootable数を取得
        /// </summary>
        private int GetAliveShootableCount()
        {
            return _shootables.Count(s => s.IsAlive);
        }

        /// <summary>
        /// ゲーム状態変更時
        /// </summary>
        private void OnGameStateChanged(GameState state)
        {
            if (state == GameState.Playing && _shootables.Count > 0)
            {
                // ゲーム開始時に最初のShootableを準備状態に
                _currentIndex = 0;
                _shootCountInTurn = 0;
                if (_shootables[0].IsAlive)
                {
                    _shootables[0].SetState(ShootableState.Ready);
                }
            }
        }

        /// <summary>
        /// 現在の引っ張り情報を取得（UI用）
        /// </summary>
        public (Vector2 direction, float power) GetCurrentPullInfo()
        {
            if (!_isDragging) return (Vector2.zero, 0f);

            Vector2 currentPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return ShotCalculator.Calculate(_dragStartPos, currentPos);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }
    }
}
