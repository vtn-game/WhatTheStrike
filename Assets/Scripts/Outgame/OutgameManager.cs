#nullable enable

using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhatTheStrike.Outgame
{
    /// <summary>
    /// アウトゲームの状態
    /// </summary>
    public enum OutgameState
    {
        Initializing, // 初期化中
        Ready,        // ショット可能
        Moving,       // 移動中
        StageSelect,  // ステージ選択中（遷移待ち）
        Transitioning // シーン遷移中
    }

    /// <summary>
    /// アウトゲーム管理
    /// ステージ選択画面の制御
    /// </summary>
    public class OutgameManager : MonoBehaviour
    {
        [Header("参照")]
        [SerializeField] private StageRegistry? stageRegistry;
        [SerializeField] private StageLayoutGenerator? layoutGenerator;
        [SerializeField] private Shootable3D? playerShootable;
        [SerializeField] private Shot3DController? shotController;

        [Header("シーン設定")]
        [SerializeField] private string outgameSceneName = "StageSelect";

        // 状態
        private OutgameState _state = OutgameState.Initializing;
        private StageButton3D? _lastHitStage;
        private Vector3 _savedPlayerPosition;

        // シングルトン
        private static OutgameManager? _instance;
        public static OutgameManager? Instance => _instance;

        // プロパティ
        public OutgameState State => _state;
        public StageRegistry? Registry => stageRegistry;

        // 静的データ（シーン間で保持）
        private static string? _pendingSceneName;
        private static Vector3 _returnPosition;
        private static bool _shouldRestorePosition;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _state = OutgameState.Initializing;

            // レイアウト生成
            if (layoutGenerator != null)
            {
                layoutGenerator.GenerateLayout();

                // ボタンイベント登録
                foreach (var button in layoutGenerator.GeneratedButtons)
                {
                    button.OnActivated += OnStageButtonActivated;
                }
            }

            // プレイヤー位置復元
            if (playerShootable != null)
            {
                if (_shouldRestorePosition)
                {
                    playerShootable.transform.position = _returnPosition;
                    _shouldRestorePosition = false;
                }
                else
                {
                    // 初期位置（ワールド下部中央）
                    float startZ = layoutGenerator != null ? -layoutGenerator.WorldSizeZ / 2 + 1 : -4f;
                    playerShootable.transform.position = new Vector3(0, 0.5f, startZ);
                }

                playerShootable.OnStopped += OnPlayerStopped;
            }

            // Shootable3Dイベント
            if (playerShootable != null)
            {
                playerShootable.OnHitCollider += OnPlayerHitCollider;
            }

            _state = OutgameState.Ready;
        }

        private void OnPlayerHitCollider(Shootable3D shootable, Collider collider)
        {
            // ステージボタンに当たった場合
            var stageButton = collider.GetComponent<StageButton3D>();
            if (stageButton != null)
            {
                _lastHitStage = stageButton;
            }
        }

        private void OnPlayerStopped(Shootable3D shootable)
        {
            // 最後に当たったステージに遷移
            if (_lastHitStage != null)
            {
                // StageButton3Dのイベントで処理される
            }
            else
            {
                // ステージに当たらなかった場合
                _state = OutgameState.Ready;
                shootable.ResetState();
            }

            _lastHitStage = null;
        }

        private void OnStageButtonActivated(StageButton3D button, bool isExactStop)
        {
            if (_state == OutgameState.Transitioning) return;

            // クリア済みステージはぴったり停止が必要
            if (button.IsCleared && !isExactStop)
            {
                Debug.Log($"クリア済みステージ {button.SceneName} はぴったり停止が必要です");
                _state = OutgameState.Ready;
                playerShootable?.ResetState();
                return;
            }

            Debug.Log($"ステージ選択: {button.SceneName}");
            TransitionToStage(button.SceneName);
        }

        /// <summary>
        /// ステージへ遷移
        /// </summary>
        private void TransitionToStage(string sceneName)
        {
            _state = OutgameState.Transitioning;

            // 現在位置を保存
            if (playerShootable != null)
            {
                _returnPosition = playerShootable.transform.position;
                _shouldRestorePosition = true;
            }

            _pendingSceneName = sceneName;

            // シーン遷移
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// アウトゲームに戻る（インゲームから呼ばれる）
        /// </summary>
        public static void ReturnToOutgame(bool stageClear = false)
        {
            if (stageClear && _pendingSceneName != null)
            {
                // クリア情報は別途PlayerPrefsなどで保持
                PlayerPrefs.SetInt($"StageClear_{_pendingSceneName}", 1);
                PlayerPrefs.Save();
            }

            // アウトゲームシーンに戻る
            var instance = Instance;
            string outgameName = instance != null ? instance.outgameSceneName : "StageSelect";
            SceneManager.LoadScene(outgameName);
        }

        /// <summary>
        /// クリア状態をロード
        /// </summary>
        public void LoadClearedStates()
        {
            if (stageRegistry == null) return;

            for (int i = 0; i < stageRegistry.StageCount; i++)
            {
                var stage = stageRegistry.GetStage(i);
                if (stage != null)
                {
                    bool isCleared = PlayerPrefs.GetInt($"StageClear_{stage.SceneName}", 0) == 1;
                    stage.IsCleared = isCleared;
                }
            }

            layoutGenerator?.UpdateClearedStates();
        }

        /// <summary>
        /// 全クリア状態をリセット
        /// </summary>
        public void ResetAllClearedStates()
        {
            if (stageRegistry == null) return;

            for (int i = 0; i < stageRegistry.StageCount; i++)
            {
                var stage = stageRegistry.GetStage(i);
                if (stage != null)
                {
                    PlayerPrefs.DeleteKey($"StageClear_{stage.SceneName}");
                    stage.IsCleared = false;
                }
            }
            PlayerPrefs.Save();

            layoutGenerator?.UpdateClearedStates();
        }

        private void OnDestroy()
        {
            if (playerShootable != null)
            {
                playerShootable.OnStopped -= OnPlayerStopped;
                playerShootable.OnHitCollider -= OnPlayerHitCollider;
            }

            if (layoutGenerator != null)
            {
                foreach (var button in layoutGenerator.GeneratedButtons)
                {
                    button.OnActivated -= OnStageButtonActivated;
                }
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
