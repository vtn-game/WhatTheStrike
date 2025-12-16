using UnityEngine;
using System;
using System.Collections.Generic;

namespace WhatTheStrike.Stage
{
    /// <summary>
    /// クリア条件
    /// </summary>
    public enum ClearCondition
    {
        DefeatAllEnemies,  // 敵全滅
        DefeatBoss,        // ボス撃破
        MatchShot,         // マッチショット
        RingBell,          // 鐘を鳴らす
        Custom             // カスタム条件
    }

    /// <summary>
    /// ステージ管理
    /// </summary>
    public class Stage : MonoBehaviour
    {
        [Header("ステージ情報")]
        [SerializeField] private string stageId;
        [SerializeField] private string stageTitle;
        [SerializeField] private ClearCondition clearCondition = ClearCondition.DefeatAllEnemies;

        [Header("ステージサイズ")]
        [SerializeField] private float width = 18f;
        [SerializeField] private float height = 10f;

        [Header("配置")]
        [SerializeField] private Transform[] playerSpawnPoints = new Transform[4];
        [SerializeField] private Transform[] enemySpawnPoints;
        [SerializeField] private Transform[] gimmickSpawnPoints;

        [Header("壁")]
        [SerializeField] private BoxCollider2D[] walls;

        // 登録されたオブジェクト
        private List<Enemy.Enemy> enemies = new List<Enemy.Enemy>();
        private Enemy.Enemy bossEnemy;

        // イベント
        public event Action OnStageClear;
        public event Action OnStageFailure;

        // プロパティ
        public string StageId => stageId;
        public string StageTitle => stageTitle;
        public float Width => width;
        public float Height => height;

        private void Start()
        {
            SetupWalls();
            RegisterEnemies();
        }

        /// <summary>
        /// 壁のセットアップ
        /// </summary>
        private void SetupWalls()
        {
            if (walls == null || walls.Length == 0)
            {
                CreateDefaultWalls();
            }
        }

        /// <summary>
        /// デフォルト壁の生成
        /// </summary>
        private void CreateDefaultWalls()
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            float wallThickness = 1f;

            // 上壁
            CreateWall(new Vector2(0, halfHeight + wallThickness / 2), new Vector2(width + wallThickness * 2, wallThickness));
            // 下壁
            CreateWall(new Vector2(0, -halfHeight - wallThickness / 2), new Vector2(width + wallThickness * 2, wallThickness));
            // 左壁
            CreateWall(new Vector2(-halfWidth - wallThickness / 2, 0), new Vector2(wallThickness, height));
            // 右壁
            CreateWall(new Vector2(halfWidth + wallThickness / 2, 0), new Vector2(wallThickness, height));
        }

        private void CreateWall(Vector2 position, Vector2 size)
        {
            var wallObj = new GameObject("Wall");
            wallObj.transform.SetParent(transform);
            wallObj.transform.localPosition = position;
            wallObj.layer = LayerMask.NameToLayer("Wall");

            var collider = wallObj.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        /// <summary>
        /// 敵の登録
        /// </summary>
        private void RegisterEnemies()
        {
            var allEnemies = FindObjectsByType<Enemy.Enemy>(FindObjectsSortMode.None);
            foreach (var enemy in allEnemies)
            {
                RegisterEnemy(enemy);
            }
        }

        /// <summary>
        /// 敵を登録
        /// </summary>
        public void RegisterEnemy(Enemy.Enemy enemy)
        {
            if (!enemies.Contains(enemy))
            {
                enemies.Add(enemy);
                enemy.OnDeath += OnEnemyDeath;

                if (enemy.Size == Enemy.EnemySize.Boss)
                {
                    bossEnemy = enemy;
                }
            }
        }

        /// <summary>
        /// 敵死亡時の処理
        /// </summary>
        private void OnEnemyDeath(Enemy.Enemy enemy)
        {
            enemies.Remove(enemy);
            CheckClearCondition();
        }

        /// <summary>
        /// クリア条件チェック
        /// </summary>
        public void CheckClearCondition()
        {
            bool cleared = false;

            switch (clearCondition)
            {
                case ClearCondition.DefeatAllEnemies:
                    cleared = enemies.Count == 0;
                    break;

                case ClearCondition.DefeatBoss:
                    cleared = bossEnemy == null || !bossEnemy.IsAlive;
                    break;

                case ClearCondition.MatchShot:
                case ClearCondition.RingBell:
                case ClearCondition.Custom:
                    // 外部からTriggerClear()で呼び出す
                    break;
            }

            if (cleared)
            {
                TriggerClear();
            }
        }

        /// <summary>
        /// クリアをトリガー（外部から呼び出し可能）
        /// </summary>
        public void TriggerClear()
        {
            Debug.Log($"Stage Clear: {stageTitle}");
            OnStageClear?.Invoke();
        }

        /// <summary>
        /// 失敗をトリガー
        /// </summary>
        public void TriggerFailure()
        {
            Debug.Log($"Stage Failed: {stageTitle}");
            OnStageFailure?.Invoke();
        }

        /// <summary>
        /// プレイヤー配置位置を取得
        /// </summary>
        public Vector2 GetPlayerSpawnPosition(int index)
        {
            if (playerSpawnPoints != null && index < playerSpawnPoints.Length && playerSpawnPoints[index] != null)
            {
                return playerSpawnPoints[index].position;
            }

            // デフォルト位置
            float xOffset = (index - 1.5f) * 2f;
            return new Vector2(xOffset, -height / 3f);
        }

        /// <summary>
        /// ステージ内かどうか判定
        /// </summary>
        public bool IsInsideStage(Vector2 position)
        {
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;

            return position.x > -halfWidth && position.x < halfWidth &&
                   position.y > -halfHeight && position.y < halfHeight;
        }
    }
}
