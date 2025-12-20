using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WhatTheStrike.Editor
{
    /// <summary>
    /// ゲームシーンを自動構築するエディタスクリプト
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

            // マネージャー生成
            CreateManagers();

            // ステージ生成
            CreateStage();

            // プレイヤー4体生成
            var players = CreatePlayers();

            // 敵を生成
            CreateEnemies();

            // UI生成
            CreateUI(players);

            // シーン保存ダイアログ
            if (EditorUtility.DisplayDialog("Scene Created",
                "ゲームシーンが作成されました。保存しますか？", "保存", "後で"))
            {
                // Scenesフォルダがなければ作成
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                }
                EditorSceneManager.SaveScene(scene, "Assets/Scenes/GameScene.unity");
                AssetDatabase.Refresh();
            }

            Debug.Log("WhatTheStrike: ゲームシーンの構築が完了しました！");
        }

        /// <summary>
        /// カメラ設定
        /// </summary>
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

        /// <summary>
        /// マネージャーを生成
        /// </summary>
        private static void CreateManagers()
        {
            // GameManager
            var gameManagerObj = new GameObject("GameManager");
            gameManagerObj.AddComponent<Core.GameManager>();

            // PhysicsManager
            var physicsManagerObj = new GameObject("PhysicsManager");
            physicsManagerObj.AddComponent<Core.PhysicsManager>();

            // FriendshipComboManager
            var comboManagerObj = new GameObject("FriendshipComboManager");
            comboManagerObj.AddComponent<FriendshipCombo.FriendshipComboManager>();
        }

        /// <summary>
        /// ステージを生成
        /// </summary>
        private static void CreateStage()
        {
            var stageObj = new GameObject("Stage");
            var stage = stageObj.AddComponent<Stage.Stage>();

            // 壁を生成
            CreateWalls(stageObj.transform);
        }

        /// <summary>
        /// 壁を生成
        /// </summary>
        private static void CreateWalls(Transform parent)
        {
            float width = 18f;
            float height = 10f;
            float wallThickness = 0.5f;

            // 物理マテリアル（反射用）
            var bouncyMaterial = new PhysicsMaterial2D("Bouncy");
            bouncyMaterial.bounciness = 0.8f;
            bouncyMaterial.friction = 0f;

            // 上壁
            CreateWall("TopWall", new Vector3(0, height / 2 + wallThickness / 2, 0),
                new Vector2(width + wallThickness * 2, wallThickness), parent, bouncyMaterial);

            // 下壁
            CreateWall("BottomWall", new Vector3(0, -height / 2 - wallThickness / 2, 0),
                new Vector2(width + wallThickness * 2, wallThickness), parent, bouncyMaterial);

            // 左壁
            CreateWall("LeftWall", new Vector3(-width / 2 - wallThickness / 2, 0, 0),
                new Vector2(wallThickness, height), parent, bouncyMaterial);

            // 右壁
            CreateWall("RightWall", new Vector3(width / 2 + wallThickness / 2, 0, 0),
                new Vector2(wallThickness, height), parent, bouncyMaterial);
        }

        private static void CreateWall(string name, Vector3 position, Vector2 size, Transform parent, PhysicsMaterial2D material)
        {
            var wallObj = new GameObject(name);
            wallObj.transform.SetParent(parent);
            wallObj.transform.localPosition = position;
            wallObj.layer = LayerMask.NameToLayer("Default");

            var collider = wallObj.AddComponent<BoxCollider2D>();
            collider.size = size;
            collider.sharedMaterial = material;

            var sr = wallObj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite((int)size.x * 10, (int)size.y * 10);
            sr.color = new Color(0.4f, 0.4f, 0.5f);
            wallObj.transform.localScale = new Vector3(size.x, size.y, 1);
        }

        /// <summary>
        /// プレイヤー4体を生成
        /// </summary>
        private static Player.Player[] CreatePlayers()
        {
            var playersParent = new GameObject("Players");
            var players = new Player.Player[4];

            Vector3[] positions = new Vector3[]
            {
                new Vector3(0, -3, 0),
                new Vector3(-2.5f, -3.5f, 0),
                new Vector3(2.5f, -3.5f, 0),
                new Vector3(0, -4, 0)
            };

            Color[] colors = new Color[]
            {
                new Color(1f, 0.3f, 0.3f),   // 赤
                new Color(0.3f, 0.5f, 1f),   // 青
                new Color(0.3f, 1f, 0.3f),   // 緑
                new Color(1f, 1f, 0.3f)      // 黄
            };

            FriendshipCombo.FriendshipComboType[] comboTypes = new FriendshipCombo.FriendshipComboType[]
            {
                FriendshipCombo.FriendshipComboType.Laser,
                FriendshipCombo.FriendshipComboType.EnergyCircle,
                FriendshipCombo.FriendshipComboType.SpeedUp,
                FriendshipCombo.FriendshipComboType.Explosion
            };

            for (int i = 0; i < 4; i++)
            {
                var playerObj = CreateCapsuleObject($"Player_{i + 1}", positions[i], colors[i], 0.8f);
                playerObj.transform.SetParent(playersParent.transform);

                var player = playerObj.AddComponent<Player.Player>();
                players[i] = player;

                // Rigidbody2D
                var rb = playerObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.linearDamping = 2f;
                rb.angularDamping = 1f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // 物理マテリアル
                var material = new PhysicsMaterial2D($"Player_{i + 1}_Material");
                material.bounciness = 0.7f;
                material.friction = 0.1f;
                playerObj.GetComponent<CircleCollider2D>().sharedMaterial = material;
            }

            // PlayerController
            var controllerObj = new GameObject("PlayerController");
            controllerObj.transform.SetParent(playersParent.transform);
            var controller = controllerObj.AddComponent<Player.PlayerController>();

            // 軌道表示用LineRenderer
            var trajectoryObj = new GameObject("TrajectoryLine");
            trajectoryObj.transform.SetParent(controllerObj.transform);
            var lineRenderer = trajectoryObj.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.white;
            lineRenderer.endColor = new Color(1, 1, 1, 0.3f);
            lineRenderer.enabled = false;

            // Controllerにプレイヤーをセット（SerializedObjectで設定）
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty playersProperty = so.FindProperty("players");
            playersProperty.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                playersProperty.GetArrayElementAtIndex(i).objectReferenceValue = players[i];
            }
            so.FindProperty("trajectoryLine").objectReferenceValue = lineRenderer;
            so.ApplyModifiedProperties();

            return players;
        }

        /// <summary>
        /// 敵を生成
        /// </summary>
        private static void CreateEnemies()
        {
            var enemiesParent = new GameObject("Enemies");

            // サンプル敵を配置
            Vector3[] positions = new Vector3[]
            {
                new Vector3(0, 2, 0),
                new Vector3(-3, 3, 0),
                new Vector3(3, 3, 0),
                new Vector3(-5, 1, 0),
                new Vector3(5, 1, 0)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var enemyObj = CreateCapsuleObject($"Enemy_{i + 1}", positions[i], new Color(0.8f, 0.2f, 0.8f), 1f);
                enemyObj.transform.SetParent(enemiesParent.transform);

                var enemy = enemyObj.AddComponent<Enemy.Enemy>();

                // Rigidbody2D（静的）
                var rb = enemyObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        /// <summary>
        /// カプセル型オブジェクトを生成
        /// </summary>
        private static GameObject CreateCapsuleObject(string name, Vector3 position, Color color, float scale)
        {
            var obj = new GameObject(name);
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one * scale;

            // SpriteRenderer（カプセル風の円）
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(64);
            sr.color = color;

            // CircleCollider2D
            var collider = obj.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            return obj;
        }

        /// <summary>
        /// UIを生成
        /// </summary>
        private static void CreateUI(Player.Player[] players)
        {
            // Canvas
            var canvasObj = new GameObject("Canvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();

            // HP表示パネル
            var hpPanelObj = new GameObject("HPPanel");
            hpPanelObj.transform.SetParent(canvasObj.transform, false);
            var hpPanelRect = hpPanelObj.AddComponent<RectTransform>();
            hpPanelRect.anchorMin = new Vector2(0, 1);
            hpPanelRect.anchorMax = new Vector2(0, 1);
            hpPanelRect.pivot = new Vector2(0, 1);
            hpPanelRect.anchoredPosition = new Vector2(20, -20);
            hpPanelRect.sizeDelta = new Vector2(300, 150);

            var panelImage = hpPanelObj.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.5f);

            // 各プレイヤーのHP表示
            Color[] playerColors = new Color[]
            {
                new Color(1f, 0.3f, 0.3f),
                new Color(0.3f, 0.5f, 1f),
                new Color(0.3f, 1f, 0.3f),
                new Color(1f, 1f, 0.3f)
            };

            for (int i = 0; i < 4; i++)
            {
                CreatePlayerHPBar(hpPanelObj.transform, i, playerColors[i]);
            }

            // ターン表示
            CreateTurnDisplay(canvasObj.transform);

            // ゲーム状態表示
            CreateGameStateDisplay(canvasObj.transform);
        }

        /// <summary>
        /// プレイヤーHPバーを生成
        /// </summary>
        private static void CreatePlayerHPBar(Transform parent, int index, Color color)
        {
            var hpBarObj = new GameObject($"Player{index + 1}_HP");
            hpBarObj.transform.SetParent(parent, false);

            var rect = hpBarObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.anchoredPosition = new Vector2(0, -10 - index * 35);
            rect.sizeDelta = new Vector2(-20, 30);

            // ラベル
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(hpBarObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(0.2f, 1);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var labelText = labelObj.AddComponent<Text>();
            labelText.text = $"P{index + 1}";
            labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            labelText.fontSize = 16;
            labelText.color = color;
            labelText.alignment = TextAnchor.MiddleCenter;

            // HPバー背景
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(hpBarObj.transform, false);
            var bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.22f, 0.2f);
            bgRect.anchorMax = new Vector2(1, 0.8f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f);

            // HPバー
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(bgObj.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);

            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = color;
        }

        /// <summary>
        /// ターン表示を生成
        /// </summary>
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
        }

        /// <summary>
        /// ゲーム状態表示を生成
        /// </summary>
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

            // 初期は非表示
            stateObj.SetActive(false);
        }

        /// <summary>
        /// 円形スプライトを生成
        /// </summary>
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
                        // グラデーションで立体感を出す
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

        /// <summary>
        /// 矩形スプライトを生成
        /// </summary>
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
