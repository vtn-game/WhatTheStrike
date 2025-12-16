using UnityEngine;

namespace WhatTheStrike.Player
{
    /// <summary>
    /// 引っ張り操作とショット制御を管理
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("ショット設定")]
        [SerializeField] private float maxPullDistance = 3f;
        [SerializeField] private float minPower = 5f;
        [SerializeField] private float maxPower = 20f;
        [SerializeField] private LineRenderer trajectoryLine;

        [Header("プレイヤー管理")]
        [SerializeField] private Player[] players = new Player[4];

        private int currentPlayerIndex = 0;
        private bool isDragging = false;
        private Vector2 dragStartPos;
        private Vector2 currentDragPos;

        // 現在のプレイヤー
        public Player CurrentPlayer => players[currentPlayerIndex];

        private void Start()
        {
            // 最初のプレイヤーをReady状態に
            if (players.Length > 0 && players[0] != null)
            {
                players[0].SetState(PlayerState.Ready);
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateTrajectory();
            CheckTurnEnd();
        }

        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            if (CurrentPlayer == null || CurrentPlayer.State != PlayerState.Ready) return;

            // タッチ/マウス開始
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // プレイヤーをタッチしたか判定
                if (IsPlayerTouched(worldPos))
                {
                    isDragging = true;
                    dragStartPos = CurrentPlayer.transform.position;
                }
            }

            // ドラッグ中
            if (Input.GetMouseButton(0) && isDragging)
            {
                currentDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            // リリース
            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                isDragging = false;
                ExecuteShot();
            }
        }

        /// <summary>
        /// プレイヤーがタッチされたか判定
        /// </summary>
        private bool IsPlayerTouched(Vector2 worldPos)
        {
            var collider = CurrentPlayer.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                return collider.OverlapPoint(worldPos);
            }
            return Vector2.Distance(worldPos, CurrentPlayer.transform.position) < 1f;
        }

        /// <summary>
        /// ショット実行
        /// </summary>
        private void ExecuteShot()
        {
            Vector2 pullVector = dragStartPos - currentDragPos;
            float pullDistance = Mathf.Min(pullVector.magnitude, maxPullDistance);

            if (pullDistance < 0.1f) return;

            Vector2 direction = pullVector.normalized;
            float power = Mathf.Lerp(minPower, maxPower, pullDistance / maxPullDistance);

            CurrentPlayer.Shoot(direction, power);
            HideTrajectory();
        }

        /// <summary>
        /// 軌道予測線の更新
        /// </summary>
        private void UpdateTrajectory()
        {
            if (!isDragging || trajectoryLine == null) return;

            Vector2 pullVector = dragStartPos - currentDragPos;
            float pullDistance = Mathf.Min(pullVector.magnitude, maxPullDistance);
            Vector2 direction = pullVector.normalized;

            // 簡易的な軌道表示（直線）
            trajectoryLine.enabled = true;
            trajectoryLine.positionCount = 2;
            trajectoryLine.SetPosition(0, CurrentPlayer.transform.position);
            trajectoryLine.SetPosition(1, (Vector2)CurrentPlayer.transform.position + direction * pullDistance * 2);
        }

        /// <summary>
        /// 軌道線を非表示
        /// </summary>
        private void HideTrajectory()
        {
            if (trajectoryLine != null)
            {
                trajectoryLine.enabled = false;
            }
        }

        /// <summary>
        /// ターン終了チェック
        /// </summary>
        private void CheckTurnEnd()
        {
            if (CurrentPlayer == null) return;

            if (CurrentPlayer.CheckStopped())
            {
                NextTurn();
            }
        }

        /// <summary>
        /// 次のターンへ
        /// </summary>
        private void NextTurn()
        {
            // 次の生存プレイヤーを探す
            int startIndex = currentPlayerIndex;
            do
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;

                if (players[currentPlayerIndex] != null && players[currentPlayerIndex].IsAlive)
                {
                    players[currentPlayerIndex].SetState(PlayerState.Ready);
                    return;
                }
            } while (currentPlayerIndex != startIndex);

            // 全員死亡の場合
            GameOver();
        }

        /// <summary>
        /// ゲームオーバー処理
        /// </summary>
        private void GameOver()
        {
            Debug.Log("Game Over!");
            // GameManagerに通知
        }

        /// <summary>
        /// 引っ張り方向と強さを取得（UI表示用）
        /// </summary>
        public (Vector2 direction, float power) GetCurrentPullInfo()
        {
            if (!isDragging) return (Vector2.zero, 0f);

            Vector2 pullVector = dragStartPos - currentDragPos;
            float pullDistance = Mathf.Min(pullVector.magnitude, maxPullDistance);
            float power = Mathf.Lerp(minPower, maxPower, pullDistance / maxPullDistance);

            return (pullVector.normalized, power);
        }
    }
}
