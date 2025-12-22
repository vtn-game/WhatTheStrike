using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using WhatTheStrike.Components;

namespace WhatTheStrike.Core
{
    /// <summary>
    /// ゲームの状態
    /// </summary>
    public enum GameState
    {
        Title,
        StageIntro,
        Playing,
        Paused,
        StageClear,
        GameOver
    }

    /// <summary>
    /// ゲーム全体の管理
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("設定")]
        [SerializeField] private int maxTurns = 30;
        [SerializeField] private float stageIntroDelay = 2f;

        // シングルトン
        private static GameManager? _instance;
        public static GameManager Instance => _instance!;

        // 状態
        private GameState _state = GameState.Title;
        private int _turnCount = 0;
        private List<Damageable> _clearTargets = new();

        // イベント
        public event Action<GameState>? OnStateChanged;
        public event Action<int>? OnTurnChanged;
        public event Action? OnStageClear;
        public event Action? OnGameOver;

        // プロパティ
        public GameState State => _state;
        public int TurnCount => _turnCount;
        public int MaxTurns => maxTurns;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            RegisterClearTargets();
        }

        /// <summary>
        /// クリア対象のDamageableを登録
        /// </summary>
        private void RegisterClearTargets()
        {
            _clearTargets.Clear();
            var allDamageables = FindObjectsByType<Damageable>(FindObjectsSortMode.None);
            foreach (var damageable in allDamageables)
            {
                if (damageable.IsTargetForClear)
                {
                    _clearTargets.Add(damageable);
                    damageable.OnDeath += OnClearTargetDeath;
                }
            }
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        public void StartGame()
        {
            SetState(GameState.StageIntro);
            Invoke(nameof(StartPlaying), stageIntroDelay);
        }

        /// <summary>
        /// プレイ開始
        /// </summary>
        private void StartPlaying()
        {
            SetState(GameState.Playing);
            _turnCount = 1;
            OnTurnChanged?.Invoke(_turnCount);
        }

        /// <summary>
        /// ターン終了
        /// </summary>
        public void EndTurn()
        {
            if (_state != GameState.Playing) return;

            _turnCount++;
            OnTurnChanged?.Invoke(_turnCount);

            // ターン上限チェック
            if (_turnCount > maxTurns)
            {
                TriggerGameOver();
                return;
            }

            // クリア条件チェック
            CheckClearCondition();
        }

        /// <summary>
        /// クリア対象死亡時
        /// </summary>
        private void OnClearTargetDeath(Damageable damageable)
        {
            _clearTargets.Remove(damageable);
            CheckClearCondition();
        }

        /// <summary>
        /// クリア条件チェック
        /// </summary>
        private void CheckClearCondition()
        {
            if (_clearTargets.Count == 0 || _clearTargets.All(t => !t.IsAlive))
            {
                TriggerStageClear();
            }
        }

        /// <summary>
        /// Shootable全滅チェック（TurnControllerから呼び出される）
        /// </summary>
        public void CheckShootablesAlive(IReadOnlyList<Shootable> shootables)
        {
            if (shootables.All(s => !s.IsAlive))
            {
                TriggerGameOver();
            }
        }

        /// <summary>
        /// ステージクリア
        /// </summary>
        private void TriggerStageClear()
        {
            SetState(GameState.StageClear);
            OnStageClear?.Invoke();
            Debug.Log($"Stage Clear! Turns: {_turnCount}");
        }

        /// <summary>
        /// ゲームオーバー
        /// </summary>
        private void TriggerGameOver()
        {
            SetState(GameState.GameOver);
            OnGameOver?.Invoke();
            Debug.Log("Game Over!");
        }

        /// <summary>
        /// ポーズ
        /// </summary>
        public void Pause()
        {
            if (_state != GameState.Playing) return;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }

        /// <summary>
        /// ポーズ解除
        /// </summary>
        public void Resume()
        {
            if (_state != GameState.Paused) return;
            SetState(GameState.Playing);
            Time.timeScale = 1f;
        }

        /// <summary>
        /// リトライ
        /// </summary>
        public void Retry()
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        /// <summary>
        /// 状態変更
        /// </summary>
        private void SetState(GameState newState)
        {
            if (_state == newState) return;
            _state = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
