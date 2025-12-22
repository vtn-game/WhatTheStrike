using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using WhatTheStrike.Components;
using WhatTheStrike.Core;

namespace WhatTheStrike.Editor
{
    /// <summary>
    /// ゲームシーンを自動構築するエディタスクリプト（コンポーネント指向版）
    /// </summary>
    public class SceneBuilder : EditorWindow
    {
        [MenuItem("WhatTheStrike/Create Game Scene")]
        public static void CreateGameScene()
        {
            // 新規シーン作成
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            scene.name = "GameScene";

            // カメラ設定
            SetupCamera();

            // Core生成
            CreateCore();

            // ステージ生成
            CreateStage();

            // Shootables（プレイヤー）生成
            CreateShootables();

            // 敵生成
            CreateEnemies();

            // UI生成
            CreateUI();

            // シーン保存ダイアログ
            if (EditorUtility.DisplayDialog("Scene Created",
                "ゲームシーンが作成されました。保存しますか？", "保存", "後で"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                }
                EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
                AssetDatabase.Refresh();
            }

            Debug.Log("WhatTheStrike: コンポーネント指向版ゲームシーンの構築が完了しました！");
        }

        private static void SetupCamera()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(0, 0, -10);
                camera.orthographic = true;
                camera.orthographicSize = 6;
                camera.backgroundColor = new Color(0.2f, 0.2f, 0.3f);
            }
        }

        private static void CreateCore()
        {
            // GameManager
            var gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<GameManager>();

            // TurnController
            var turnControllerObj = new GameObject("TurnController");
            var turnController = turnControllerObj.AddComponent<TurnController>();

            // 軌道表示用LineRenderer
            var trajectoryObj = new GameObject("TrajectoryLine");
            trajectoryObj.transform.SetParent(turnControllerObj.transform);
            var lineRenderer = trajectoryObj.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
            lineRenderer.enabled = false;

            SerializedObject so = new SerializedObject(turnController);
            so.FindProperty("trajectoryLine").objectReferenceValue = lineRenderer;
            so.ApplyModifiedProperties();
        }

        private static void CreateStage()
        {
            var stageObj = new GameObject("Stage");

            float width = 18f;
            float height = 10f;
            float wallThickness = 0.5f;

            var bouncyMaterial = new PhysicsMaterial2D("Bouncy")
            {
                bounciness = 0.8f,
                friction = 0f
            };

            CreateWall("TopWall", new Vector3(0, height / 2 + wallThickness / 2, 0),
                new Vector2(width + wallThickness * 2, wallThickness), stageObj.transform, bouncyMaterial);
            CreateWall("BottomWall", new Vector3(0, -height / 2 - wallThickness / 2, 0),
                new Vector2(width + wallThickness * 2, wallThickness), stageObj.transform, bouncyMaterial);
            CreateWall("LeftWall", new Vector3(-width / 2 - wallThickness / 2, 0, 0),
                new Vector2(wallThickness, height), stageObj.transform, bouncyMaterial);
            CreateWall("RightWall", new Vector3(width / 2 + wallThickness / 2, 0, 0),
                new Vector2(wallThickness, height), stageObj.transform, bouncyMaterial);

            // フロア（背景）
            var floorObj = new GameObject("Floor");
            floorObj.transform.SetParent(stageObj.transform);
            var floorSr = floorObj.AddComponent<SpriteRenderer>();
            floorSr.sprite = CreateRectSprite(180, 100);
            floorSr.color = new Color(0.15f, 0.15f, 0.2f);
            floorSr.sortingOrder = -10;
            floorObj.transform.localScale = new Vector3(width, height, 1);
        }

        private static void CreateWall(string name, Vector3 position, Vector2 size, Transform parent, PhysicsMaterial2D material)
        {
            var wallObj = new GameObject(name);
            wallObj.transform.SetParent(parent);
            wallObj.transform.localPosition = position;

            var collider = wallObj.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.sharedMaterial = material;

            var sr = wallObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite((int)(size.x * 10), (int)(size.y * 10));
            sr.color = new Color(0.4f, 0.4f, 0.5f);
            wallObj.transform.localScale = new Vector3(size.x, size.y, 1);
        }

        private static void CreateShootables()
        {
            var shootablesParent = new GameObject("Shootables");

            Vector3[] positions =
            {
                new Vector3(0, -3, 0),
                new Vector3(-2.5f, -3.5f, 0),
                new Vector3(2.5f, -3.5f, 0),
                new Vector3(0, -4, 0)
            };

            Color[] colors =
            {
                new Color(1f, 0.3f, 0.3f),   // 赤
                new Color(0.3f, 0.5f, 1f),   // 青
                new Color(0.3f, 1f, 0.3f),   // 緑
                new Color(1f, 1f, 0.3f)      // 黄
            };

            string[] names = { "Red", "Blue", "Green", "Yellow" };

            FriendshipComboType[] comboTypes =
            {
                FriendshipComboType.Laser,
                FriendshipComboType.EnergyCircle,
                FriendshipComboType.SpeedUp,
                FriendshipComboType.Explosion
            };

            for (int i = 0; i < 4; i++)
            {
                var obj = CreateCircleObject($"Player_{names[i]}", positions[i], colors[i], 0.8f);
                obj.transform.SetParent(shootablesParent.transform);

                // Shootable
                var shootable = obj.AddComponent<Shootable>();
                SerializedObject shootableSo = new SerializedObject(shootable);
                shootableSo.FindProperty("speed").floatValue = 1.0f;
                shootableSo.FindProperty("attack").intValue = 10;
                shootableSo.FindProperty("friendshipComboType").enumValueIndex = (int)comboTypes[i];
                shootableSo.FindProperty("friendshipComboDamage").intValue = 10;
                shootableSo.FindProperty("friendshipComboRange").floatValue = 3f;
                shootableSo.ApplyModifiedProperties();

                // Damageable（プレイヤーもダメージを受ける）
                var damageable = obj.AddComponent<Damageable>();
                SerializedObject damageableSo = new SerializedObject(damageable);
                damageableSo.FindProperty("maxHp").intValue = 100;
                damageableSo.FindProperty("defense").intValue = 0;
                damageableSo.FindProperty("isTargetForClear").boolValue = false;
                damageableSo.ApplyModifiedProperties();

                // Rigidbody2D
                var rb = obj.GetComponent<Rigidbody2D>();
                rb.linearDamping = 2f;
                rb.angularDamping = 1f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // 物理マテリアル
                var material = new PhysicsMaterial2D($"Player_{i}_Material")
                {
                    bounciness = 0.7f,
                    friction = 0.1f
                };
                obj.GetComponent<CircleCollider2D>().sharedMaterial = material;
            }
        }

        private static void CreateEnemies()
        {
            var enemiesParent = new GameObject("Enemies");

            Vector3[] positions =
            {
                new Vector3(0, 2, 0),
                new Vector3(-3, 3, 0),
                new Vector3(3, 3, 0),
                new Vector3(-5, 1, 0),
                new Vector3(5, 1, 0)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var obj = CreateCircleObject($"Enemy_{i + 1}", positions[i], new Color(0.8f, 0.2f, 0.8f), 1f);
                obj.transform.SetParent(enemiesParent.transform);

                // Damageable（クリア対象）
                var damageable = obj.AddComponent<Damageable>();
                SerializedObject so = new SerializedObject(damageable);
                so.FindProperty("maxHp").intValue = 50;
                so.FindProperty("defense").intValue = 0;
                so.FindProperty("isTargetForClear").boolValue = true;
                so.ApplyModifiedProperties();

                // Rigidbody2D（静的）
                var rb = obj.GetComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        private static GameObject CreateCircleObject(string name, Vector3 position, Color color, float scale)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * scale;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(64);
            sr.color = color;

            var collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            var rb = obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;

            return obj;
        }

        private static void CreateUI()
        {
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // ターン表示
            CreateTurnDisplay(canvasObj.transform);

            // ゲーム状態表示
            CreateGameStateDisplay(canvasObj.transform);
        }

        private static void CreateTurnDisplay(Transform parent)
        {
            var turnObj = new GameObject("TurnDisplay");
            turnObj.transform.SetParent(parent, false);

            var rect = turnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-20, -20);
            rect.sizeDelta = new Vector2(150, 40);

            var bg = turnObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.5f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(turnObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<Text>();
            text.text = "Turn: 1";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            // UIUpdater
            var uiUpdater = turnObj.AddComponent<UIUpdater>();
            SerializedObject so = new SerializedObject(uiUpdater);
            so.FindProperty("turnText").objectReferenceValue = text;
            so.ApplyModifiedProperties();
        }

        private static void CreateGameStateDisplay(Transform parent)
        {
            var stateObj = new GameObject("GameStateDisplay");
            stateObj.transform.SetParent(parent, false);

            var rect = stateObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(400, 100);

            var bg = stateObj.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.7f);

            var textObj = new GameObject("Text");
            textObj.transform.SetParent(stateObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObj.AddComponent<Text>();
            text.text = "STAGE START!";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 36;
            text.color = Color.yellow;
            text.alignment = TextAnchor.MiddleCenter;

            stateObj.SetActive(false);
        }

        private static Sprite CreateCircleSprite(int size)
        {
            var texture = new Texture2D(size, size);
            var center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f;

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist < radius)
                    {
                        float brightness = 1f - (dist / radius) * 0.3f;
                        texture.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            return Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }

        private static Sprite CreateRectSprite(int width, int height)
        {
            var texture = new Texture2D(width, height);
            var colors = new Color[width * height];

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.white;
            }

            texture.SetPixels(colors);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.one * 0.5f, Mathf.Max(width, height));
        }
    }
}
