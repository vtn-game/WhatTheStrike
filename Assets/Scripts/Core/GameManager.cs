using UnityEngine;
using System;

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
        // シングルトン
        private static GameManager instance;
        public static GameManager Instance => instance;

        [Header("参照")]
        [SerializeField] private Stage.Stage currentStage;
        [SerializeField] private Player.PlayerController playerController;

        // 状態
        private GameState gameState = GameState.Title;
        private int turnCount = 0;

        // イベント
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnTurnChanged;

        // プロパティ
        public GameState CurrentState => gameState;
        public int TurnCount => turnCount;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            // ステージイベント登録
            if (currentStage != null)
            {
                currentStage.OnStageClear += OnStageClear;
                currentStage.OnStageFailure += OnStageFailure;
            }
        }

        /// <summary>
        /// ゲーム開始
        /// </summary>
        public void StartGame()
        {
            SetGameState(GameState.StageIntro);

            // ステージイントロ後にプレイ開始
            Invoke(nameof(StartPlaying), 2f);
        }

        /// <summary>
        /// プレイ開始
        /// </summary>
        private void StartPlaying()
        {
            SetGameState(GameState.Playing);
            turnCount = 1;
            OnTurnChanged?.Invoke(turnCount);
        }

        /// <summary>
        /// ターン終了
        /// </summary>
        public void EndTurn()
        {
            if (gameState != GameState.Playing) return;

            turnCount++;
            OnTurnChanged?.Invoke(turnCount);

            // クリア条件チェック
            currentStage?.CheckClearCondition();
        }

        /// <summary>
        /// ポーズ
        /// </summary>
        public void Pause()
        {
            if (gameState != GameState.Playing) return;

            SetGameState(GameState.Paused);
            Time.timeScale = 0f;
        }

        /// <summary>
        /// ポーズ解除
        /// </summary>
        public void Resume()
        {
            if (gameState != GameState.Paused) return;

            SetGameState(GameState.Playing);
            Time.timeScale = 1f;
        }

        /// <summary>
        /// ステージクリア時
        /// </summary>
        private void OnStageClear()
        {
            SetGameState(GameState.StageClear);
            Debug.Log($"Stage Clear! Turns: {turnCount}");
        }

        /// <summary>
        /// ステージ失敗時
        /// </summary>
        private void OnStageFailure()
        {
            SetGameState(GameState.GameOver);
            Debug.Log("Game Over!");
        }

        /// <summary>
        /// リトライ
        /// </summary>
        public void Retry()
        {
            Time.timeScale = 1f;
            // シーンリロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        /// <summary>
        /// タイトルへ戻る
        /// </summary>
        public void ReturnToTitle()
        {
            Time.timeScale = 1f;
            SetGameState(GameState.Title);
            // タイトルシーンへ
            // SceneManager.LoadScene("Title");
        }

        /// <summary>
        /// ゲーム状態を変更
        /// </summary>
        private void SetGameState(GameState newState)
        {
            gameState = newState;
            OnGameStateChanged?.Invoke(newState);
        }
    }
}
