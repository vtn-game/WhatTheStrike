#nullable enable

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using WhatTheStrike.Outgame;

namespace WhatTheStrike.Editor
{
    /// <summary>
    /// アウトゲームシーン自動生成
    /// </summary>
    public static class OutgameSceneBuilder
    {
        [MenuItem("WhatTheStrike/Create Outgame Scene")]
        public static void CreateOutgameScene()
        {
            // 新規シーン作成
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "StageSelect";

            // カメラ設定
            CreateCamera();

            // 床・壁作成
            CreateFloorAndWalls();

            // マネージャー作成
            CreateManagers();

            // プレイヤー作成
            CreatePlayer();

            // ライト作成
            CreateLight();

            Debug.Log("アウトゲームシーンを作成しました");
        }

        private static void CreateCamera()
        {
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            var camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = false;
            camera.fieldOfView = 60;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
            cameraObj.transform.position = new Vector3(0, 15, -5);
            cameraObj.transform.rotation = Quaternion.Euler(70, 0, 0);
            cameraObj.AddComponent<AudioListener>();
        }

        private static void CreateFloorAndWalls()
        {
            float worldSize = 10f;
            float wallHeight = 2f;
            float wallThickness = 0.5f;

            // 床
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.position = new Vector3(0, -0.25f, 0);
            floor.transform.localScale = new Vector3(worldSize, 0.5f, worldSize);
            var floorRenderer = floor.GetComponent<Renderer>();
            floorRenderer.material = CreateMaterial(new Color(0.3f, 0.3f, 0.35f));

            // 壁（上）
            var wallTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallTop.name = "Wall_Top";
            wallTop.transform.position = new Vector3(0, wallHeight / 2, worldSize / 2);
            wallTop.transform.localScale = new Vector3(worldSize + wallThickness * 2, wallHeight, wallThickness);
            wallTop.GetComponent<Renderer>().material = CreateMaterial(new Color(0.5f, 0.5f, 0.55f));

            // 壁（下）
            var wallBottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallBottom.name = "Wall_Bottom";
            wallBottom.transform.position = new Vector3(0, wallHeight / 2, -worldSize / 2);
            wallBottom.transform.localScale = new Vector3(worldSize + wallThickness * 2, wallHeight, wallThickness);
            wallBottom.GetComponent<Renderer>().material = CreateMaterial(new Color(0.5f, 0.5f, 0.55f));

            // 壁（左）
            var wallLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallLeft.name = "Wall_Left";
            wallLeft.transform.position = new Vector3(-worldSize / 2, wallHeight / 2, 0);
            wallLeft.transform.localScale = new Vector3(wallThickness, wallHeight, worldSize);
            wallLeft.GetComponent<Renderer>().material = CreateMaterial(new Color(0.5f, 0.5f, 0.55f));

            // 壁（右）
            var wallRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wallRight.name = "Wall_Right";
            wallRight.transform.position = new Vector3(worldSize / 2, wallHeight / 2, 0);
            wallRight.transform.localScale = new Vector3(wallThickness, wallHeight, worldSize);
            wallRight.GetComponent<Renderer>().material = CreateMaterial(new Color(0.5f, 0.5f, 0.55f));

            // 壁をグループ化
            var wallsParent = new GameObject("Walls");
            wallTop.transform.SetParent(wallsParent.transform);
            wallBottom.transform.SetParent(wallsParent.transform);
            wallLeft.transform.SetParent(wallsParent.transform);
            wallRight.transform.SetParent(wallsParent.transform);
        }

        private static void CreateManagers()
        {
            // Managers親オブジェクト
            var managersObj = new GameObject("Managers");

            // OutgameManager
            var outgameManagerObj = new GameObject("OutgameManager");
            outgameManagerObj.transform.SetParent(managersObj.transform);
            var outgameManager = outgameManagerObj.AddComponent<OutgameManager>();

            // StageLayoutGenerator
            var layoutGeneratorObj = new GameObject("StageLayoutGenerator");
            layoutGeneratorObj.transform.SetParent(managersObj.transform);
            var layoutGenerator = layoutGeneratorObj.AddComponent<StageLayoutGenerator>();

            // ステージボタンコンテナ
            var buttonContainer = new GameObject("StageButtons");
            buttonContainer.transform.SetParent(layoutGeneratorObj.transform);

            // SerializedObjectで参照を設定
            var outgameSO = new SerializedObject(outgameManager);
            outgameSO.FindProperty("layoutGenerator").objectReferenceValue = layoutGenerator;
            outgameSO.ApplyModifiedProperties();

            var layoutSO = new SerializedObject(layoutGenerator);
            layoutSO.FindProperty("stageButtonContainer").objectReferenceValue = buttonContainer.transform;
            layoutSO.ApplyModifiedProperties();
        }

        private static void CreatePlayer()
        {
            // プレイヤー球体
            var playerObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            playerObj.name = "Player";
            playerObj.transform.position = new Vector3(0, 0.5f, -4);
            playerObj.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            playerObj.GetComponent<Renderer>().material = CreateMaterial(new Color(0.2f, 0.6f, 1f));

            // Rigidbody設定
            var rb = playerObj.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

            // Shootable3D追加
            var shootable = playerObj.AddComponent<Shootable3D>();

            // Shot3DController追加
            var shotController = playerObj.AddComponent<Shot3DController>();

            // 軌道表示用LineRenderer
            var lineRenderer = playerObj.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
            lineRenderer.enabled = false;

            // SerializedObjectで参照を設定
            var controllerSO = new SerializedObject(shotController);
            controllerSO.FindProperty("shootable").objectReferenceValue = shootable;
            controllerSO.FindProperty("trajectoryLine").objectReferenceValue = lineRenderer;
            controllerSO.ApplyModifiedProperties();

            // OutgameManagerに参照を設定
            var outgameManager = Object.FindFirstObjectByType<OutgameManager>();
            if (outgameManager != null)
            {
                var managerSO = new SerializedObject(outgameManager);
                managerSO.FindProperty("playerShootable").objectReferenceValue = shootable;
                managerSO.FindProperty("shotController").objectReferenceValue = shotController;
                managerSO.ApplyModifiedProperties();
            }
        }

        private static void CreateLight()
        {
            var lightObj = new GameObject("Directional Light");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            lightObj.transform.position = new Vector3(0, 10, 0);
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        private static Material CreateMaterial(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            var material = new Material(shader!);
            material.color = color;
            return material;
        }

        [MenuItem("WhatTheStrike/Create Stage Button Prefab")]
        public static void CreateStageButtonPrefab()
        {
            // 保存先フォルダ
            string folderPath = "Assets/Prefabs/Outgame";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "Outgame");
            }

            // ステージボタン作成
            var buttonObj = new GameObject("StageButton");

            // 円形のビジュアル（円柱で代用）
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(buttonObj.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
            visual.GetComponent<Renderer>().material = CreateMaterial(new Color(0.8f, 0.4f, 0.2f));

            // コライダー削除（親にまとめる）
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            // キャラクター画像表示用（SpriteRenderer）
            var imageObj = new GameObject("CharacterImage");
            imageObj.transform.SetParent(buttonObj.transform);
            imageObj.transform.localPosition = new Vector3(0, 0.15f, 0);
            imageObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            imageObj.transform.localScale = new Vector3(1f, 1f, 1f);
            var spriteRenderer = imageObj.AddComponent<SpriteRenderer>();

            // 当たり判定（球）
            var collider = buttonObj.AddComponent<SphereCollider>();
            collider.radius = 0.8f;
            collider.isTrigger = true;

            // StageButton3Dコンポーネント
            var stageButton = buttonObj.AddComponent<StageButton3D>();

            // SerializedObjectで参照を設定
            var buttonSO = new SerializedObject(stageButton);
            buttonSO.FindProperty("characterImageRenderer").objectReferenceValue = spriteRenderer;
            buttonSO.ApplyModifiedProperties();

            // プレハブとして保存
            string prefabPath = $"{folderPath}/StageButton.prefab";
            PrefabUtility.SaveAsPrefabAsset(buttonObj, prefabPath);
            Object.DestroyImmediate(buttonObj);

            Debug.Log($"ステージボタンプレハブを作成しました: {prefabPath}");

            // StageLayoutGeneratorにプレハブを設定
            var layoutGenerator = Object.FindFirstObjectByType<StageLayoutGenerator>();
            if (layoutGenerator != null)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var layoutSO = new SerializedObject(layoutGenerator);
                layoutSO.FindProperty("stageButtonPrefab").objectReferenceValue = prefab;
                layoutSO.ApplyModifiedProperties();
            }
        }
    }
}
