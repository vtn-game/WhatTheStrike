#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WhatTheStrike.Outgame
{
    /// <summary>
    /// ステージ登録用ScriptableObject
    /// エディターでステージシーンを自動収集する
    /// </summary>
    [CreateAssetMenu(fileName = "StageRegistry", menuName = "WhatTheStrike/Stage Registry")]
    public class StageRegistry : ScriptableObject
    {
        [Header("ステージ設定")]
        [SerializeField] private string stageScenePrefix = "Stage_";
        [SerializeField] private string stageSceneFolder = "Assets/Scenes/Stages";

        [Header("登録済みステージ")]
        [SerializeField] private List<StageInfo> stages = new();

        /// <summary>
        /// 登録されているステージ一覧
        /// </summary>
        public IReadOnlyList<StageInfo> Stages => stages;

        /// <summary>
        /// ステージ数
        /// </summary>
        public int StageCount => stages.Count;

        /// <summary>
        /// ステージシーンのプレフィックス
        /// </summary>
        public string StageScenePrefix => stageScenePrefix;

        /// <summary>
        /// ステージシーンのフォルダパス
        /// </summary>
        public string StageSceneFolder => stageSceneFolder;

        /// <summary>
        /// ステージを取得
        /// </summary>
        public StageInfo? GetStage(int index)
        {
            if (index < 0 || index >= stages.Count) return null;
            return stages[index];
        }

        /// <summary>
        /// シーン名からステージを取得
        /// </summary>
        public StageInfo? GetStageBySceneName(string sceneName)
        {
            return stages.FirstOrDefault(s => s.SceneName == sceneName);
        }

        /// <summary>
        /// ステージをクリア済みにする
        /// </summary>
        public void SetStageCleared(string sceneName, bool cleared = true)
        {
            var stage = GetStageBySceneName(sceneName);
            if (stage != null)
            {
                stage.IsCleared = cleared;
            }
        }

        /// <summary>
        /// 全ステージクリア済みか
        /// </summary>
        public bool IsAllCleared => stages.All(s => s.IsCleared);

#if UNITY_EDITOR
        /// <summary>
        /// ステージリストをクリア（Editor用）
        /// </summary>
        public void ClearStages()
        {
            stages.Clear();
        }

        /// <summary>
        /// ステージを追加（Editor用）
        /// </summary>
        public void AddStage(StageInfo stageInfo)
        {
            stages.Add(stageInfo);
        }
#endif
    }
}
