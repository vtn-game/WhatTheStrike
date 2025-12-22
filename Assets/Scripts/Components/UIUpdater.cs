using UnityEngine;
using UnityEngine.UI;
using WhatTheStrike.Core;

namespace WhatTheStrike.Components
{
    /// <summary>
    /// UI更新コンポーネント
    /// ターン数やゲーム状態の表示を管理
    /// </summary>
    public class UIUpdater : MonoBehaviour
    {
        [Header("UI参照")]
        [SerializeField] private Text? turnText;
        [SerializeField] private GameObject? gameStateDisplay;
        [SerializeField] private Text? gameStateText;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTurnChanged += UpdateTurnDisplay;
                GameManager.Instance.OnStateChanged += UpdateGameStateDisplay;
                GameManager.Instance.OnStageClear += ShowStageClear;
                GameManager.Instance.OnGameOver += ShowGameOver;
            }
        }

        /// <summary>
        /// ターン表示を更新
        /// </summary>
        private void UpdateTurnDisplay(int turn)
        {
            if (turnText != null)
            {
                turnText.text = $"Turn: {turn}";
            }
        }

        /// <summary>
        /// ゲーム状態表示を更新
        /// </summary>
        private void UpdateGameStateDisplay(GameState state)
        {
            if (gameStateDisplay == null) return;

            switch (state)
            {
                case GameState.StageIntro:
                    ShowMessage("STAGE START!", Color.yellow);
                    break;
                case GameState.Playing:
                    HideMessage();
                    break;
                case GameState.Paused:
                    ShowMessage("PAUSED", Color.white);
                    break;
            }
        }

        /// <summary>
        /// ステージクリア表示
        /// </summary>
        private void ShowStageClear()
        {
            ShowMessage("STAGE CLEAR!", Color.green);
        }

        /// <summary>
        /// ゲームオーバー表示
        /// </summary>
        private void ShowGameOver()
        {
            ShowMessage("GAME OVER", Color.red);
        }

        /// <summary>
        /// メッセージを表示
        /// </summary>
        private void ShowMessage(string message, Color color)
        {
            if (gameStateDisplay != null)
            {
                gameStateDisplay.SetActive(true);
            }
            if (gameStateText != null)
            {
                gameStateText.text = message;
                gameStateText.color = color;
            }
        }

        /// <summary>
        /// メッセージを非表示
        /// </summary>
        private void HideMessage()
        {
            if (gameStateDisplay != null)
            {
                gameStateDisplay.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnTurnChanged -= UpdateTurnDisplay;
                GameManager.Instance.OnStateChanged -= UpdateGameStateDisplay;
                GameManager.Instance.OnStageClear -= ShowStageClear;
                GameManager.Instance.OnGameOver -= ShowGameOver;
            }
        }
    }
}
