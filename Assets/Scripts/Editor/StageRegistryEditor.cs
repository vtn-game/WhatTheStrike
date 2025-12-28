#nullable enable

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using WhatTheStrike.Outgame;

namespace WhatTheStrike.Editor
{
    /// <summary>
    /// StageRegistry用のカスタムエディター
    /// ステージシーンを自動収集する
    /// </summary>
    [CustomEditor(typeof(StageRegistry))]
    public class StageRegistryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            var registry = (StageRegistry)target;

            if (GUILayout.Button("ステージシーンを自動収集", GUILayout.Height(30)))
            {
                CollectStageScenes(registry);
            }

            EditorGUILayout.Space(5);

            if (GUILayout.Button("ステージリストをクリア", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("確認", "ステージリストをクリアしますか？", "はい", "いいえ"))
                {
                    ClearStages(registry);
                }
            }
        }

        private void CollectStageScenes(StageRegistry registry)
        {
            registry.ClearStages();

            string folder = registry.StageSceneFolder;
            string prefix = registry.StageScenePrefix;

            // フォルダが存在しない場合は作成
            if (!AssetDatabase.IsValidFolder(folder))
            {
                string[] pathParts = folder.Split('/');
                string currentPath = pathParts[0];
                for (int i = 1; i < pathParts.Length; i++)
                {
                    string nextPath = currentPath + "/" + pathParts[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, pathParts[i]);
                    }
                    currentPath = nextPath;
                }
                Debug.Log($"ステージフォルダを作成しました: {folder}");
            }

            // シーンファイルを検索
            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { folder });
            var stageScenes = sceneGuids
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Where(path => Path.GetFileNameWithoutExtension(path).StartsWith(prefix))
                .OrderBy(path => path)
                .ToList();

            foreach (var scenePath in stageScenes)
            {
                string sceneName = Path.GetFileNameWithoutExtension(scenePath);
                string displayName = sceneName.Replace(prefix, "").Replace("_", " ");

                // キャラクター画像を探す（同名のスプライトがあれば使用）
                var spritePath = Path.ChangeExtension(scenePath, null) + ".png";
                Sprite? sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                var stageInfo = new StageInfo(sceneName, displayName, sprite);
                registry.AddStage(stageInfo);

                Debug.Log($"ステージを登録: {sceneName}");
            }

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();

            Debug.Log($"ステージ収集完了: {stageScenes.Count}個のステージを登録しました");
        }

        private void ClearStages(StageRegistry registry)
        {
            registry.ClearStages();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log("ステージリストをクリアしました");
        }
    }
}
