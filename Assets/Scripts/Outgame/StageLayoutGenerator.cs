#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace WhatTheStrike.Outgame
{
    /// <summary>
    /// ステージ配置生成器
    /// シード固定のランダム配置でステージボタンを配置する
    /// </summary>
    public class StageLayoutGenerator : MonoBehaviour
    {
        [Header("ワールド設定")]
        [SerializeField] private float worldSizeX = 10f;
        [SerializeField] private float worldSizeZ = 10f;
        [SerializeField] private float stageButtonRadius = 0.8f;
        [SerializeField] private float minDistanceBetweenStages = 2f;

        [Header("配置設定")]
        [SerializeField] private int layoutSeed = 12345;
        [SerializeField] private int maxPlacementAttempts = 100;

        [Header("プレハブ")]
        [SerializeField] private GameObject? stageButtonPrefab;

        [Header("参照")]
        [SerializeField] private StageRegistry? stageRegistry;
        [SerializeField] private Transform? stageButtonContainer;

        // 生成されたボタン
        private List<StageButton3D> _generatedButtons = new();

        // プロパティ
        public IReadOnlyList<StageButton3D> GeneratedButtons => _generatedButtons;
        public float WorldSizeX => worldSizeX;
        public float WorldSizeZ => worldSizeZ;

        /// <summary>
        /// ステージを配置
        /// </summary>
        public void GenerateLayout()
        {
            ClearLayout();

            if (stageRegistry == null || stageButtonPrefab == null)
            {
                Debug.LogError("StageLayoutGenerator: StageRegistryまたはPrefabが設定されていません");
                return;
            }

            // シード固定の乱数生成器
            var random = new System.Random(layoutSeed);
            var placedPositions = new List<Vector3>();

            // プレイヤー開始位置を確保（中央下部）
            Vector3 playerStartPos = new(0, 0, -worldSizeZ / 2 + 1);
            placedPositions.Add(playerStartPos);

            Transform container = stageButtonContainer != null ? stageButtonContainer : transform;

            for (int i = 0; i < stageRegistry.StageCount; i++)
            {
                var stageInfo = stageRegistry.GetStage(i);
                if (stageInfo == null) continue;

                Vector3? position = FindValidPosition(random, placedPositions);
                if (!position.HasValue)
                {
                    Debug.LogWarning($"StageLayoutGenerator: ステージ {stageInfo.SceneName} の配置位置が見つかりませんでした");
                    continue;
                }

                // ステージボタン生成
                var buttonObj = Instantiate(stageButtonPrefab, position.Value, Quaternion.identity, container);
                buttonObj.name = $"StageButton_{stageInfo.SceneName}";

                var button = buttonObj.GetComponent<StageButton3D>();
                if (button != null)
                {
                    button.Setup(stageInfo.SceneName, i, stageInfo.CharacterImage, stageInfo.IsCleared);
                    _generatedButtons.Add(button);
                }

                placedPositions.Add(position.Value);
            }

            Debug.Log($"StageLayoutGenerator: {_generatedButtons.Count}個のステージを配置しました");
        }

        /// <summary>
        /// 有効な配置位置を探す
        /// </summary>
        private Vector3? FindValidPosition(System.Random random, List<Vector3> existingPositions)
        {
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                // ランダムな位置を生成
                float x = (float)(random.NextDouble() * 2 - 1) * (worldSizeX / 2 - stageButtonRadius);
                float z = (float)(random.NextDouble() * 2 - 1) * (worldSizeZ / 2 - stageButtonRadius);
                Vector3 candidate = new(x, 0, z);

                // 既存位置との距離チェック
                bool isValid = true;
                foreach (var pos in existingPositions)
                {
                    if (Vector3.Distance(candidate, pos) < minDistanceBetweenStages)
                    {
                        isValid = false;
                        break;
                    }
                }

                if (isValid)
                {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// レイアウトをクリア
        /// </summary>
        public void ClearLayout()
        {
            foreach (var button in _generatedButtons)
            {
                if (button != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(button.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(button.gameObject);
                    }
                }
            }
            _generatedButtons.Clear();
        }

        /// <summary>
        /// ステージボタンのクリア状態を更新
        /// </summary>
        public void UpdateClearedStates()
        {
            if (stageRegistry == null) return;

            foreach (var button in _generatedButtons)
            {
                var stageInfo = stageRegistry.GetStageBySceneName(button.SceneName);
                if (stageInfo != null)
                {
                    button.IsCleared = stageInfo.IsCleared;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // ワールド範囲を表示
            Gizmos.color = Color.cyan;
            Vector3 center = transform.position;
            Vector3 size = new(worldSizeX, 0.1f, worldSizeZ);
            Gizmos.DrawWireCube(center, size);

            // プレイヤー開始位置
            Gizmos.color = Color.green;
            Vector3 playerStart = center + new Vector3(0, 0, -worldSizeZ / 2 + 1);
            Gizmos.DrawWireSphere(playerStart, 0.5f);
        }
    }
}
