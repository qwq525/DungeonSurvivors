using System.Collections.Generic;
using System.IO;
using DungeonSurvivors.Commerce;
using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Enemies;
using DungeonSurvivors.Gameplay.Player;
using DungeonSurvivors.Gameplay.Progression;
using DungeonSurvivors.Gameplay.Weapons;
using DungeonSurvivors.Systems;
using DungeonSurvivors.UI;
using DungeonSurvivors.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// 一键生成 W1 可玩场景 + 占位 Prefab（AI 生成，命令行可执行）：
    /// 1) 生成占位精灵：玩家 / 史莱姆 / 经验宝石 / 追踪弹 / 地砖；
    /// 2) 生成 Prefab：Enemy_Slime / Projectile_Missile / ExperienceGem；
    /// 3) 生成 Main 场景：相机、地面、围墙、GameManager、PoolRoot、Player（移动+闪避+武器+生命+经验）、EnemySpawner、GemSpawner；
    /// 4) 写入 Build Settings。
    /// 运行方式：
    /// Unity.exe -batchmode -quit -projectPath "E:\Unity_Project\DungeonSurvivors" -executeMethod DungeonSurvivors.EditorTools.Day0SceneSetup.Run -logFile "...\Logs\w1_setup.log"
    /// 菜单：DungeonSurvivors -> Setup Scene (W1)
    /// </summary>
    public static class Day0SceneSetup
    {
        private const string PlaceholderDir = "Assets/_Project/Art/Placeholders";
        private const string PrefabEnemyDir = "Assets/_Project/Prefabs/Enemies";
        private const string PrefabCombatDir = "Assets/_Project/Prefabs/Combat";
        private const string PrefabProgressionDir = "Assets/_Project/Prefabs/Progression";
        private const string ScenePath = "Assets/_Project/Scenes/Main.unity";
        private const string ArtScenePath = "Assets/_Project/Scenes/MainArt.unity";
        private const string ArtPrefabDir = "Assets/_Project/Art/Real/Prefabs";
        private const string SlimeArtDir = "Assets/ThirdParty/Craftpix/Slime";
        private const string BaseMaleArtDir = "Assets/ThirdParty/Craftpix/BaseMale";
        private const string FontAssetPath = "Assets/_Project/Art/Fonts/NotoSansCJKsc-Regular.otf";
        private const float ArenaHalfWidth = 24f;
        private const float ArenaHalfHeight = 14f;

        [MenuItem("DungeonSurvivors/Setup Scene (W1)")]
        public static void Run()
        {
            EnsureFolder(PlaceholderDir);
            EnsureFolder(PrefabEnemyDir);
            EnsureFolder(PrefabCombatDir);
            EnsureFolder(PrefabProgressionDir);
            EnsureFolder("Assets/_Project/Scenes");
            EnsureFolder("Assets/_Project/Prefabs");

            bool useArt = HasCommandLineFlag("-art")
                          && AssetDatabase.IsValidFolder(SlimeArtDir)
                          && AssetDatabase.IsValidFolder(BaseMaleArtDir);
            string scenePath = useArt ? ArtScenePath : ScenePath;
            CharacterArt playerArt = useArt ? LoadCharacterArt(SlimeArtDir, "Slime1") : null;
            CharacterArt enemyArt = useArt ? LoadCharacterArt(BaseMaleArtDir, "Unarmed") : null;
            if (useArt && (playerArt == null || enemyArt == null))
            {
                Debug.LogWarning("[W1Setup] Art sheets could not be loaded; falling back to placeholders.");
                useArt = false;
                scenePath = ScenePath;
            }
            if (useArt)
            {
                EnsureFolder("Assets/_Project/Art/Real");
                EnsureFolder(ArtPrefabDir);
            }

            Sprite playerSprite = CreateSprite("player_placeholder.png", 32, PlayerPixel, false);
            Sprite groundSprite = CreateSprite("ground_placeholder.png", 32, GroundPixel, true);
            Sprite floorSprite = LoadExternalSprite("Assets/ThirdParty/Kenney/Dungeon/floor_tile.png");
            Sprite slimeSprite = CreateSprite("slime_placeholder.png", 32, SlimePixel, false);
            Sprite projectileSprite = CreateSprite("projectile_placeholder.png", 16, ProjectilePixel, false);
            Sprite gemSprite = CreateSprite("gem_placeholder.png", 16, GemPixel, false);
            Sprite glowSprite = CreateSprite("glow_placeholder.png", 128, GlowPixel, false);
            Sprite vignetteSprite = CreateSprite("vignette_placeholder.png", 256, VignettePixel, false);
            Sprite circleSprite = CreateSprite("circle_placeholder.png", 128, CirclePixel, false);
            Sprite enemyArrowSprite = CreateSprite("enemy_arrow_placeholder.png", 16, EnemyArrowPixel, false);

            CharacterArt swordsmanArt = useArt ? LoadCharacterArtFromFiles(
                "Assets/ThirdParty/Craftpix/Classes/Swordsman/Swordsman_lvl1_Idle_without_shadow.png",
                "Assets/ThirdParty/Craftpix/Classes/Swordsman/Swordsman_lvl1_Run_without_shadow.png") : null;
            CharacterArt archerArt = useArt ? LoadCharacterArtFromFiles(
                "Assets/ThirdParty/Craftpix/Classes/Archer/Archer_lvl1_Idle_without_shadow.png",
                "Assets/ThirdParty/Craftpix/Classes/Archer/Archer_lvl1_Run_without_shadow.png") : null;
            CharacterArt mageArt = useArt ? LoadCharacterArtFromFiles(
                "Assets/ThirdParty/Craftpix/Classes/Mage/Mage_lvl4_Idle_full_without_shadow.png",
                "Assets/ThirdParty/Craftpix/Classes/Mage/Mage_lvl4_Run_full_without_shadow.png") : null;

            Projectile projectilePrefab = CreateProjectilePrefab(projectileSprite);
            ExperienceGem gemPrefab = CreateGemPrefab(gemSprite);
            EnemyProjectile enemyProjectilePrefab = CreateEnemyProjectilePrefab(enemyArrowSprite);
            EnemyBase[] enemyPrefabs = CreateEnemyPrefabs(slimeSprite, enemyArt, swordsmanArt, archerArt, mageArt, enemyProjectilePrefab);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ---------- Camera ----------
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(11, 10, 18, 255); // #0B0A12
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();

            // ---------- EventSystem（UGUI 鼠标/键盘事件必需，之前漏了导致按钮点不动） ----------
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

            // ---------- Ground & Walls ----------
            GameObject groundObject = new GameObject("Ground");
            SpriteRenderer groundRenderer = groundObject.AddComponent<SpriteRenderer>();
            groundRenderer.sprite = floorSprite != null ? floorSprite : groundSprite;
            groundRenderer.color = floorSprite != null ? new Color(0.46f, 0.42f, 0.48f, 1f) : Color.white;
            groundRenderer.drawMode = SpriteDrawMode.Tiled;
            groundRenderer.size = new Vector2(ArenaHalfWidth * 2f, ArenaHalfHeight * 2f);
            groundRenderer.sortingOrder = -10;

            // ---------- Atmosphere: torches ----------
            CreateTorch("Torch_TL", new Vector2(-ArenaHalfWidth + 1.5f, ArenaHalfHeight - 1.4f), glowSprite);
            CreateTorch("Torch_TR", new Vector2(ArenaHalfWidth - 1.5f, ArenaHalfHeight - 1.4f), glowSprite);
            CreateTorch("Torch_BL", new Vector2(-ArenaHalfWidth + 1.5f, -ArenaHalfHeight + 1.4f), glowSprite);
            CreateTorch("Torch_BR", new Vector2(ArenaHalfWidth - 1.5f, -ArenaHalfHeight + 1.4f), glowSprite);

            CreateWall("Wall_Top", new Vector2(0f, ArenaHalfHeight + 0.5f), new Vector2(ArenaHalfWidth * 2f + 2f, 1f));
            CreateWall("Wall_Bottom", new Vector2(0f, -ArenaHalfHeight - 0.5f), new Vector2(ArenaHalfWidth * 2f + 2f, 1f));
            CreateWall("Wall_Left", new Vector2(-ArenaHalfWidth - 0.5f, 0f), new Vector2(1f, ArenaHalfHeight * 2f + 2f));
            CreateWall("Wall_Right", new Vector2(ArenaHalfWidth + 0.5f, 0f), new Vector2(1f, ArenaHalfHeight * 2f + 2f));

            TerrainBuilder.Build();

            // ---------- Managers ----------
            GameObject gameManagerObject = new GameObject("GameManager");
            gameManagerObject.AddComponent<GameManager>();

            GameObject poolRootObject = new GameObject("PoolRoot");
            Transform poolRoot = poolRootObject.transform;

            // ---------- Player ----------
            GameObject playerObject = new GameObject("Player");
            SpriteRenderer playerRenderer = playerObject.AddComponent<SpriteRenderer>();
            playerRenderer.sprite = playerSprite;
            playerRenderer.sortingOrder = 10;

            if (playerArt != null)
            {
                playerRenderer.sprite = playerArt.Idle.Rows[0][0];
                playerObject.transform.localScale = Vector3.one * 1.6f;
                DirectionalSpriteAnimator playerAnimator = playerObject.AddComponent<DirectionalSpriteAnimator>();
                playerAnimator.Configure(playerRenderer,
                    playerArt.Idle.Rows[0], playerArt.Run.Rows[0],
                    playerArt.Idle.Rows[1], playerArt.Run.Rows[1],
                    playerArt.Idle.Rows[2], playerArt.Run.Rows[2]);
            }

            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            CircleCollider2D collider = playerObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.35f;

            playerObject.AddComponent<PlayerController>();
            playerObject.AddComponent<DashAbility>();
            PlayerHealth playerHealth = playerObject.AddComponent<PlayerHealth>();
            playerObject.AddComponent<PlayerExperience>();
            playerObject.AddComponent<TalentEffects>();

            MissileWeapon weapon = playerObject.AddComponent<MissileWeapon>();
            weapon.Configure(projectilePrefab, poolRoot);
            OrbitWeapon orbitWeapon = playerObject.AddComponent<OrbitWeapon>();

            // ---------- Camera follow ----------
            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
            follow.SetTarget(playerObject.transform);
            float halfViewHeight = camera.orthographicSize;
            float halfViewWidth = halfViewHeight * 16f / 9f;
            follow.SetBounds(
                new Vector2(-(ArenaHalfWidth - halfViewWidth + 0.8f), -(ArenaHalfHeight - halfViewHeight + 0.8f)),
                new Vector2(ArenaHalfWidth - halfViewWidth + 0.8f, ArenaHalfHeight - halfViewHeight + 0.8f));

            // ---------- Spawners ----------
            GameObject enemySpawnerObject = new GameObject("EnemySpawner");
            EnemySpawner enemySpawner = enemySpawnerObject.AddComponent<EnemySpawner>();
            enemySpawner.Configure(enemyPrefabs, poolRoot);
            enemySpawner.TuneForArena(12, 5, 150, 18f, 0.2f, 10.5f);

            GameObject gemSpawnerObject = new GameObject("GemSpawner");
            GemSpawner gemSpawner = gemSpawnerObject.AddComponent<GemSpawner>();
            gemSpawner.Configure(gemPrefab, poolRoot);

            // ---------- HUD ----------
            Canvas hudCanvas = CreateHud();
            CreateBossHealthBar(hudCanvas.transform);
            CreateSkillCooldown(hudCanvas.transform, circleSprite);
            CreateMinimap(hudCanvas.transform);
            CreateVignette(hudCanvas.transform, vignetteSprite);
            DamageNumberSpawner damageNumbers = hudCanvas.gameObject.AddComponent<DamageNumberSpawner>();
            damageNumbers.Configure(hudCanvas);
            CreateDevDataPanel(hudCanvas.transform);
            ScreenFlash screenFlash = CreateScreenFlash(hudCanvas.transform);

            // ---------- Combat feedback ----------
            CombatFeedback feedback = cameraObject.AddComponent<CombatFeedback>();
            feedback.Configure(follow, screenFlash);

            // ---------- Upgrade panel & run flow ----------
            UpgradePanel upgradePanel = CreateUpgradePanel(hudCanvas.transform);
            UpgradeSystem upgradeSystem = playerObject.AddComponent<UpgradeSystem>();
            upgradeSystem.Configure(upgradePanel, weapon, playerHealth, orbitWeapon);

            ResultPanel resultPanel = CreateResultPanel(hudCanvas.transform);
            PausePanel pausePanel = CreatePausePanel(hudCanvas.transform);
            AdMockPanel adPanel = CreateAdMockPanel(hudCanvas.transform);

            GameObject adServiceObject = new GameObject("AdService");
            MockAdService adService = adServiceObject.AddComponent<MockAdService>();
            adService.Configure(adPanel);

            // ---------- Shell: main menu / talents / settings ----------
            MainMenuRefs mainMenu = CreateMainMenu(hudCanvas.transform);
            TalentPanel talentPanel = CreateTalentPanel(hudCanvas.transform);
            SettingsPanel settingsPanel = CreateSettingsPanel(hudCanvas.transform);
            AchievementPanel achievementPanel = CreateAchievementPanel(hudCanvas.transform);
            CodexPanel codexPanel = CreateCodexPanel(hudCanvas.transform);
            TutorialPanel tutorialPanel = CreateTutorialPanel(hudCanvas.transform);
            mainMenu.Controller.Configure(mainMenu.Root, mainMenu.Start, mainMenu.Daily, mainMenu.Codex, mainMenu.Talents, mainMenu.Settings,
                mainMenu.Achievements, mainMenu.Quit, talentPanel, settingsPanel, achievementPanel, codexPanel, tutorialPanel);

            GameObject gameFlowObject = new GameObject("GameFlow");
            gameFlowObject.AddComponent<RunStats>();
            GameFlowController gameFlow = gameFlowObject.AddComponent<GameFlowController>();
            gameFlow.Configure(resultPanel, pausePanel, adService);

            StressTestRunner stressTest = gameFlowObject.AddComponent<StressTestRunner>();
            stressTest.Configure(enemySpawner, playerHealth, weapon);
            gameFlowObject.AddComponent<ScreenshotRunner>();

            GameObject audioObject = new GameObject("AudioService");
            AudioService audioService = audioObject.AddComponent<AudioService>();
            audioService.Configure(
                LoadAudioClip("Assets/ThirdParty/Kenney/Audio/shoot.ogg"),
                LoadAudioClip("Assets/ThirdParty/Kenney/Audio/hit.ogg"),
                LoadAudioClip("Assets/ThirdParty/Kenney/Audio/death.ogg"),
                LoadAudioClip("Assets/ThirdParty/Kenney/Audio/hurt.ogg"),
                LoadAudioClip("Assets/ThirdParty/Kenney/Audio/levelup.ogg"),
                LoadAudioClip("Assets/ThirdParty/Kenney/Audio/pickup.ogg"));

            // ---------- Save ----------
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            AssetDatabase.SaveAssets();

            Debug.Log("[W1Setup] Scene generated: " + scenePath + " (art=" + useArt + ")");
        }

        private class SpriteSheetRows
        {
            public Sprite[][] Rows;
        }

        private class CharacterArt
        {
            public SpriteSheetRows Idle;
            public SpriteSheetRows Run;
        }

        private static CharacterArt LoadCharacterArtFromFiles(string idlePath, string runPath)
        {
            SpriteSheetRows idle = LoadSheetRowsFromFile(idlePath);
            SpriteSheetRows run = LoadSheetRowsFromFile(runPath);
            if (idle == null || run == null) return null;
            if (idle.Rows.Length < 3 || run.Rows.Length < 3) return null;
            return new CharacterArt { Idle = idle, Run = run };
        }

        private static CharacterArt LoadCharacterArt(string folder, string prefix)
        {
            SpriteSheetRows idle = LoadSheetRows(folder, prefix, "Idle");
            SpriteSheetRows run = LoadSheetRows(folder, prefix, "Run");
            if (idle == null || run == null) return null;
            if (idle.Rows.Length < 3 || run.Rows.Length < 3) return null;
            return new CharacterArt { Idle = idle, Run = run };
        }

        private static SpriteSheetRows LoadSheetRows(string folder, string prefix, string animation)
        {
            return LoadSheetRowsFromFile(folder + "/" + prefix + "_" + animation + "_without_shadow.png");
        }

        private static SpriteSheetRows LoadSheetRowsFromFile(string path)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            List<List<Sprite>> rows = new List<List<Sprite>>();
            for (int i = 0; i < 4; i++) rows.Add(new List<Sprite>());

            foreach (Object asset in assets)
            {
                Sprite sprite = asset as Sprite;
                if (sprite == null) continue;
                string[] parts = sprite.name.Split('_');
                if (parts.Length < 2) continue;
                string rowPart = parts[parts.Length - 2];
                if (!rowPart.StartsWith("r")) continue;
                if (!int.TryParse(rowPart.Substring(1), out int row)) continue;
                if (row < 0 || row >= rows.Count) continue;
                rows[row].Add(sprite);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Count == 0) return null;
                rows[i].Sort((a, b) => ExtractColumn(a.name).CompareTo(ExtractColumn(b.name)));
            }

            SpriteSheetRows result = new SpriteSheetRows();
            result.Rows = new Sprite[rows.Count][];
            for (int i = 0; i < rows.Count; i++) result.Rows[i] = rows[i].ToArray();
            return result;
        }

        private static int ExtractColumn(string spriteName)
        {
            string[] parts = spriteName.Split('_');
            if (parts.Length == 0) return 0;
            string colPart = parts[parts.Length - 1];
            if (colPart.StartsWith("c") && int.TryParse(colPart.Substring(1), out int column)) return column;
            return 0;
        }

        private static bool HasCommandLineFlag(string flag)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i], flag, System.StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }

        private static EnemyBase[] CreateEnemyPrefabs(Sprite fallbackSprite, CharacterArt baseArt,
            CharacterArt swordsmanArt, CharacterArt archerArt, CharacterArt mageArt, EnemyProjectile projectilePrefab)
        {
            List<EnemyBase> result = new List<EnemyBase>();
            result.Add(CreateEnemyPrefab("Enemy_Swordsman", swordsmanArt, fallbackSprite, true,
                EnemyCombatProfile.AttackType.Melee, 0.7f, 0.5f, 0.8f, 3, null, 1.7f));
            result.Add(CreateEnemyPrefab("Enemy_Archer", archerArt, fallbackSprite, true,
                EnemyCombatProfile.AttackType.Ranged, 6f, 3f, 1.3f, 4, projectilePrefab, 1.7f));
            result.Add(CreateEnemyPrefab("Enemy_Mage", mageArt, fallbackSprite, true,
                EnemyCombatProfile.AttackType.Ranged, 5f, 2.5f, 1.6f, 5, projectilePrefab, 1.7f));
            if (result.Count == 0 || result[0] == null)
            {
                result.Clear();
                result.Add(CreateEnemyPrefab("Enemy_Slime", baseArt, fallbackSprite, false,
                    EnemyCombatProfile.AttackType.Melee, 0.7f, 0.5f, 0.8f, 3, null, 1f));
            }
            return result.ToArray();
        }

        private static EnemyBase CreateEnemyPrefab(string prefabName, CharacterArt art, Sprite fallbackSprite, bool sideFacesRight,
            EnemyCombatProfile.AttackType attackType, float attackRange, float retreatDistance, float interval, int damage,
            EnemyProjectile projectilePrefab, float scale)
        {
            GameObject go = new GameObject(prefabName);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = fallbackSprite;
            renderer.sortingOrder = 5;

            if (art != null)
            {
                renderer.sprite = art.Idle.Rows[0][0];
                go.transform.localScale = Vector3.one * scale;
                DirectionalSpriteAnimator animator = go.AddComponent<DirectionalSpriteAnimator>();
                animator.Configure(renderer,
                    art.Idle.Rows[0], art.Run.Rows[0],
                    art.Idle.Rows[1], art.Run.Rows[1],
                    art.Idle.Rows[2], art.Run.Rows[2], 10f, sideFacesRight);
            }

            EnemyCombatProfile profile = go.AddComponent<EnemyCombatProfile>();
            profile.Configure(attackType, attackRange, retreatDistance, interval, damage, projectilePrefab);
            go.AddComponent<EnemyBase>();

            string prefabPath = art != null
                ? ArtPrefabDir + "/" + prefabName + ".prefab"
                : PrefabEnemyDir + "/" + prefabName + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<EnemyBase>();
        }

        private static EnemyProjectile CreateEnemyProjectilePrefab(Sprite sprite)
        {
            GameObject go = new GameObject("EnemyProjectile");
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 20;
            go.transform.localScale = Vector3.one * 0.5f;
            go.AddComponent<EnemyProjectile>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabCombatDir + "/EnemyProjectile.prefab");
            Object.DestroyImmediate(go);
            return prefab.GetComponent<EnemyProjectile>();
        }

        private static Projectile CreateProjectilePrefab(Sprite sprite)
        {
            GameObject go = new GameObject("Projectile_Missile");
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 20;
            go.transform.localScale = Vector3.one * 0.4f;
            go.AddComponent<Projectile>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabCombatDir + "/Projectile_Missile.prefab");
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Projectile>();
        }

        private static ExperienceGem CreateGemPrefab(Sprite sprite)
        {
            GameObject go = new GameObject("ExperienceGem");
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 6;
            go.transform.localScale = Vector3.one * 0.5f;
            go.AddComponent<ExperienceGem>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, PrefabProgressionDir + "/ExperienceGem.prefab");
            Object.DestroyImmediate(go);
            return prefab.GetComponent<ExperienceGem>();
        }

        private static Canvas CreateHud()
        {
            GameObject canvasObject = new GameObject("HUD", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            HUDController hud = canvasObject.AddComponent<HUDController>();

            GameObject hudGroup = CreateUiObject("HUD_Group", canvasObject.transform);
            SetRect(hudGroup.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Transform hudRoot = hudGroup.transform;

            // 血条
            Image healthBackground = CreateImage("HealthBar_Background", hudRoot, new Color32(36, 30, 51, 235));
            SetRect(healthBackground.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -24f), new Vector2(360f, 32f));

            Image healthFill = CreateImage("HealthBar_Fill", healthBackground.transform, new Color32(255, 90, 90, 255));
            SetRect(healthFill.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            healthFill.type = Image.Type.Simple;

            Text healthText = CreateText("HealthText", healthBackground.transform, "100 / 100", 18, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(healthText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            // 等级
            Text levelText = CreateText("LevelText", hudRoot, "等级 1", 26, new Color32(255, 246, 224, 255), TextAnchor.MiddleLeft);
            SetRect(levelText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(24f, -62f), new Vector2(260f, 34f));

            // 计时
            Text timerText = CreateText("TimerText", hudRoot, "00:00", 30, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(timerText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -24f), new Vector2(220f, 40f));

            // 经验条
            Image xpBackground = CreateImage("XpBar_Background", hudRoot, new Color32(30, 24, 52, 235));
            SetRect(xpBackground.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 22f), new Vector2(-48f, 24f));

            Image xpFill = CreateImage("XpBar_Fill", xpBackground.transform, new Color32(130, 105, 255, 255));
            SetRect(xpFill.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            xpFill.type = Image.Type.Simple;

            Text xpText = CreateText("XpText", xpBackground.transform, "EXP 0 / 5", 16, new Color32(235, 230, 255, 255), TextAnchor.MiddleCenter);
            SetRect(xpText.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            // 击杀数
            Text killsText = CreateText("KillsText", hudRoot, "击杀 0", 24, new Color32(255, 246, 224, 255), TextAnchor.MiddleRight);
            SetRect(killsText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-24f, -24f), new Vector2(260f, 34f));

            // 波次
            Text waveText = CreateText("WaveText", hudRoot, "波次 1", 22, new Color32(180, 172, 200, 255), TextAnchor.MiddleCenter);
            SetRect(waveText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -66f), new Vector2(220f, 30f));

            hud.Configure(healthFill, xpFill, levelText, timerText, killsText, waveText, hudGroup, healthText, xpText);
            return canvas;
        }

        private static UpgradePanel CreateUpgradePanel(Transform parent)
        {
            GameObject rootObject = new GameObject("UpgradePanel", typeof(RectTransform));
            rootObject.transform.SetParent(parent, false);
            SetRect(rootObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image overlay = rootObject.AddComponent<Image>();
            overlay.color = new Color32(11, 10, 18, 230);

            GameObject panelObject = CreateUiObject("Panel", rootObject.transform);
            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color32(36, 30, 51, 255);
            SetRect(panelObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 560f));

            Text title = CreateText("Title", panelObject.transform, "升级", 34, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(-40f, 50f));

            Button[] buttons = new Button[3];
            Text[] titles = new Text[3];
            Text[] descriptions = new Text[3];

            for (int i = 0; i < 3; i++)
            {
                Text label;
                Text desc;
                buttons[i] = CreateOptionButton("Option" + (i + 1), panelObject.transform, new Vector2(0f, 100f - i * 130f), out label, out desc);
                titles[i] = label;
                descriptions[i] = desc;
            }

            UpgradePanel panel = parent.GetComponent<UpgradePanel>();
            if (panel == null) panel = parent.gameObject.AddComponent<UpgradePanel>();
            panel.Configure(rootObject, title, buttons, titles, descriptions);
            rootObject.SetActive(false);
            return panel;
        }

        private static Button CreateOptionButton(string name, Transform parent, Vector2 position, out Text title, out Text description)
        {
            GameObject buttonObject = CreateUiObject(name, parent);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color32(58, 49, 80, 255);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            SetRect(buttonObject.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(640f, 112f));

            title = CreateText("Title", buttonObject.transform, "OPTION", 26, new Color32(255, 246, 224, 255), TextAnchor.MiddleLeft);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(-48f, 38f));

            description = CreateText("Desc", buttonObject.transform, "", 20, new Color32(180, 172, 200, 255), TextAnchor.MiddleLeft);
            SetRect(description.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(-48f, 32f));

            return button;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = CreateUiObject(name, parent);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor alignment)
        {
            GameObject go = CreateUiObject(name, parent);
            Text text = go.AddComponent<Text>();
            Font font = AssetDatabase.LoadAssetAtPath<Font>(FontAssetPath);
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static void CreateMinimap(Transform parent)
        {
            GameObject cameraObject = new GameObject("MinimapCamera");
            Camera minimapCamera = cameraObject.AddComponent<Camera>();
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = 6.5f;
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color32(10, 8, 14, 255);
            minimapCamera.transform.position = new Vector3(0f, 0f, -10f);

            GameObject root = CreateUiObject("Minimap", parent);
            SetRect(root.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-22f, -84f), new Vector2(176f, 176f));

            Image border = root.AddComponent<Image>();
            border.color = new Color32(46, 38, 62, 215);

            GameObject viewObject = CreateUiObject("View", root.transform);
            RawImage view = viewObject.AddComponent<RawImage>();
            view.color = new Color32(255, 255, 255, 225);
            SetRect(view.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(-8f, -8f));

            Text label = CreateText("MapLabel", root.transform, "MAP", 14, new Color32(255, 246, 224, 220), TextAnchor.UpperLeft);
            SetRect(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(8f, -6f), new Vector2(120f, 20f));

            GameObject markerObject = CreateUiObject("PlayerMarker", root.transform);
            Image marker = markerObject.AddComponent<Image>();
            marker.color = new Color32(120, 255, 170, 255);
            SetRect(marker.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(8f, 8f));

            MinimapCamera minimap = cameraObject.AddComponent<MinimapCamera>();
            minimap.Configure(minimapCamera, view, root, null, 6.5f);
            root.SetActive(false);
        }

        private static void CreateBossHealthBar(Transform parent)
        {
            GameObject root = CreateUiObject("BossHealthBar", parent);
            SetRect(root.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -104f), new Vector2(520f, 24f));

            Image background = root.AddComponent<Image>();
            background.color = new Color32(42, 22, 30, 235);

            GameObject fillObject = CreateUiObject("Fill", root.transform);
            Image fill = fillObject.AddComponent<Image>();
            fill.color = new Color32(225, 62, 70, 255);
            SetRect(fill.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            Text label = CreateText("Label", root.transform, "勇者队长", 16, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            BossHealthBar bar = parent.GetComponent<BossHealthBar>();
            if (bar == null) bar = parent.gameObject.AddComponent<BossHealthBar>();
            bar.Configure(root, fill, label);
            root.SetActive(false);
        }

        private static void CreateSkillCooldown(Transform parent, Sprite circleSprite)
        {
            GameObject root = CreateUiObject("SkillCooldown", parent);
            SetRect(root.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-90f, 90f), new Vector2(112f, 112f));

            Image background = root.AddComponent<Image>();
            background.sprite = circleSprite;
            background.color = new Color32(255, 210, 80, 235);

            GameObject fillObject = CreateUiObject("Cooldown", root.transform);
            Image fill = fillObject.AddComponent<Image>();
            fill.sprite = circleSprite;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Radial360;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            fill.color = new Color32(22, 18, 30, 235);
            SetRect(fill.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            Text key = CreateText("Key", root.transform, "SPACE", 18, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(key.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(0f, 22f));

            Text name = CreateText("Name", root.transform, "闪避", 16, new Color32(40, 30, 50, 255), TextAnchor.MiddleCenter);
            SetRect(name.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -6f), new Vector2(0f, 22f));

            SkillCooldownUI skill = parent.GetComponent<SkillCooldownUI>();
            if (skill == null) skill = parent.gameObject.AddComponent<SkillCooldownUI>();
            skill.Configure(root, fill, key);
            root.SetActive(false);
        }

        private static void CreateTorch(string name, Vector2 position, Sprite glowSprite)
        {
            GameObject torch = new GameObject(name);
            torch.transform.position = position;
            SpriteRenderer renderer = torch.AddComponent<SpriteRenderer>();
            renderer.sprite = glowSprite;
            renderer.color = new Color32(255, 150, 60, 190);
            renderer.sortingOrder = -5;
            torch.transform.localScale = Vector3.one * 2.5f;
            torch.AddComponent<TorchFlicker>();
        }

        private static void CreateVignette(Transform parent, Sprite vignetteSprite)
        {
            GameObject vignette = CreateUiObject("Vignette", parent);
            SetRect(vignette.transform as RectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image image = vignette.AddComponent<Image>();
            image.sprite = vignetteSprite;
            image.color = Color.white;
            image.raycastTarget = false;
            vignette.transform.SetAsFirstSibling();
        }

        private static ResultPanel CreateResultPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("ResultPanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(760f, 560f));

            Text title = CreateText("Title", panel.transform, "结算", 40, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -35f), new Vector2(-40f, 60f));

            Text stats = CreateText("Stats", panel.transform, "时间 00:00    击杀 0    等级 1", 24,
                new Color32(180, 172, 200, 255), TextAnchor.MiddleCenter);
            SetRect(stats.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -105f), new Vector2(-40f, 40f));

            Text souls = CreateText("Souls", panel.transform, "灵魂 +0", 22, new Color32(217, 164, 65, 255), TextAnchor.MiddleCenter);
            SetRect(souls.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -150f), new Vector2(-40f, 35f));

            Button retry = CreateSimpleButton("Retry", panel.transform, new Vector2(0f, 40f), new Vector2(420f, 68f), "重开");
            Button revive = CreateSimpleButton("Revive", panel.transform, new Vector2(0f, -40f), new Vector2(420f, 68f), "看广告复活");
            Button menu = CreateSimpleButton("Menu", panel.transform, new Vector2(0f, -120f), new Vector2(420f, 68f), "主菜单");
            Button quit = CreateSimpleButton("Quit", panel.transform, new Vector2(0f, -200f), new Vector2(420f, 68f), "退出");

            ResultPanel result = parent.GetComponent<ResultPanel>();
            if (result == null) result = parent.gameObject.AddComponent<ResultPanel>();
            result.Configure(root, title, stats, souls, retry, revive, menu, quit);
            root.SetActive(false);
            return result;
        }

        private static AdMockPanel CreateAdMockPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("AdMockPanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(700f, 380f));

            Text title = CreateText("Title", panel.transform, "激励视频（模拟）", 30, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-30f, 50f));

            Text countdown = CreateText("Countdown", panel.transform, "3 秒后发放奖励", 26,
                new Color32(180, 172, 200, 255), TextAnchor.MiddleCenter);
            SetRect(countdown.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -130f), new Vector2(-30f, 45f));

            Button close = CreateSimpleButton("Close", panel.transform, new Vector2(0f, -100f), new Vector2(420f, 68f), "关闭（放弃奖励）");

            AdMockPanel adPanel = parent.GetComponent<AdMockPanel>();
            if (adPanel == null) adPanel = parent.gameObject.AddComponent<AdMockPanel>();
            adPanel.Configure(root, title, countdown, close);
            root.SetActive(false);
            return adPanel;
        }

        private static PausePanel CreatePausePanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("PausePanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(640f, 460f));

            Text title = CreateText("Title", panel.transform, "暂停", 40, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-40f, 60f));

            Button resume = CreateSimpleButton("Resume", panel.transform, new Vector2(0f, 40f), new Vector2(420f, 70f), "继续");
            Button restart = CreateSimpleButton("Restart", panel.transform, new Vector2(0f, -40f), new Vector2(420f, 70f), "重开");
            Button menu = CreateSimpleButton("Menu", panel.transform, new Vector2(0f, -120f), new Vector2(420f, 70f), "主菜单");

            PausePanel pause = parent.GetComponent<PausePanel>();
            if (pause == null) pause = parent.gameObject.AddComponent<PausePanel>();
            pause.Configure(root, resume, restart, menu);
            root.SetActive(false);
            return pause;
        }

        private static GameObject CreateOverlayRoot(string name, Transform parent)
        {
            GameObject root = CreateUiObject(name, parent);
            SetRect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image overlay = root.AddComponent<Image>();
            overlay.color = new Color32(11, 10, 18, 230);
            return root;
        }

        private static GameObject CreatePanelBox(string name, Transform parent, Vector2 size)
        {
            GameObject go = CreateUiObject(name, parent);
            Image image = go.AddComponent<Image>();
            image.color = new Color32(36, 30, 51, 255);
            SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, size);
            return go;
        }

        private static Button CreateSimpleButton(string name, Transform parent, Vector2 position, Vector2 size, string label)
        {
            GameObject go = CreateUiObject(name, parent);
            Image image = go.AddComponent<Image>();
            image.color = new Color32(58, 49, 80, 255);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, size);

            Text text = CreateText("Label", go.transform, label, 28, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(text.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return button;
        }

        private class MainMenuRefs
        {
            public GameObject Root;
            public Button Start;
            public Button Daily;
            public Button Codex;
            public Button Talents;
            public Button Settings;
            public Button Achievements;
            public Button Quit;
            public MainMenuController Controller;
        }

        private static MainMenuRefs CreateMainMenu(Transform parent)
        {
            MainMenuRefs refs = new MainMenuRefs();
            refs.Root = CreateOverlayRoot("MainMenu", parent);
            GameObject panel = CreatePanelBox("Panel", refs.Root.transform, new Vector2(760f, 660f));

            Text title = CreateText("Title", panel.transform, "DUNGEON SURVIVORS", 44, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -45f), new Vector2(-40f, 70f));

            Text subtitle = CreateText("Subtitle", panel.transform, "你是地牢最后一只怪物", 20,
                new Color32(217, 164, 65, 255), TextAnchor.MiddleCenter);
            SetRect(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -105f), new Vector2(-40f, 36f));

            refs.Start = CreateSimpleButton("Start", panel.transform, new Vector2(0f, 140f), new Vector2(430f, 54f), "开始游戏");
            refs.Daily = CreateSimpleButton("Daily", panel.transform, new Vector2(0f, 75f), new Vector2(430f, 54f), "每日挑战");
            refs.Codex = CreateSimpleButton("Codex", panel.transform, new Vector2(0f, 10f), new Vector2(430f, 54f), "图鉴");
            refs.Achievements = CreateSimpleButton("Achievements", panel.transform, new Vector2(0f, -55f), new Vector2(430f, 54f), "成就");
            refs.Talents = CreateSimpleButton("Talents", panel.transform, new Vector2(0f, -120f), new Vector2(430f, 54f), "天赋");
            refs.Settings = CreateSimpleButton("Settings", panel.transform, new Vector2(0f, -185f), new Vector2(430f, 54f), "设置");
            refs.Quit = CreateSimpleButton("Quit", panel.transform, new Vector2(0f, -250f), new Vector2(430f, 54f), "退出");

            MainMenuController controller = parent.GetComponent<MainMenuController>();
            if (controller == null) controller = parent.gameObject.AddComponent<MainMenuController>();
            refs.Controller = controller;
            refs.Root.SetActive(false);
            return refs;
        }

        private static TalentPanel CreateTalentPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("TalentPanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(760f, 640f));

            Text title = CreateText("Title", panel.transform, "天赋", 38, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-40f, 55f));

            Text souls = CreateText("Souls", panel.transform, "灵魂 0", 24, new Color32(217, 164, 65, 255), TextAnchor.MiddleCenter);
            SetRect(souls.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -105f), new Vector2(-40f, 36f));

            Button[] buttons = new Button[TalentCatalog.Talents.Count];
            Text[] labels = new Text[TalentCatalog.Talents.Count];
            for (int i = 0; i < buttons.Length; i++)
            {
                Text label;
                buttons[i] = CreateLabeledButton("Talent" + i, panel.transform,
                    new Vector2(0f, 120f - i * 100f), new Vector2(640f, 84f), "天赋", out label);
                labels[i] = label;
            }

            Button back = CreateSimpleButton("Back", panel.transform, new Vector2(0f, -255f), new Vector2(320f, 62f), "返回");

            TalentPanel talentPanel = parent.GetComponent<TalentPanel>();
            if (talentPanel == null) talentPanel = parent.gameObject.AddComponent<TalentPanel>();
            talentPanel.Configure(root, souls, buttons, labels, back);
            root.SetActive(false);
            return talentPanel;
        }

        private static TutorialPanel CreateTutorialPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("TutorialPanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(760f, 560f));

            Text title = CreateText("Title", panel.transform, "操作说明", 38, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-40f, 55f));

            Text content = CreateText("Content", panel.transform,
                "移动        WASD / 方向键\n" +
                "闪避        空格（翻滚 + 短暂无敌）\n" +
                "攻击        自动瞄准最近的敌人\n" +
                "升级        升级后三选一变异\n" +
                "暂停        ESC\n" +
                "数据面板    F1\n" +
                "目标        守住 10 波并击败 Boss",
                20, new Color32(255, 246, 224, 255), TextAnchor.UpperLeft);
            SetRect(content.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -110f), new Vector2(-110f, 320f));

            Button close = CreateSimpleButton("GotIt", panel.transform, new Vector2(0f, -215f), new Vector2(320f, 64f), "知道了");

            TutorialPanel tutorialPanel = parent.GetComponent<TutorialPanel>();
            if (tutorialPanel == null) tutorialPanel = parent.gameObject.AddComponent<TutorialPanel>();
            tutorialPanel.Configure(root, close);
            root.SetActive(false);
            return tutorialPanel;
        }

        private static CodexPanel CreateCodexPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("CodexPanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(760f, 640f));

            Text title = CreateText("Title", panel.transform, "图鉴", 38, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-40f, 55f));

            Text content = CreateText("Content", panel.transform, "", 19, new Color32(255, 246, 224, 255), TextAnchor.UpperLeft);
            SetRect(content.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -100f), new Vector2(-90f, 400f));

            Button back = CreateSimpleButton("Back", panel.transform, new Vector2(0f, -260f), new Vector2(320f, 60f), "返回");

            CodexPanel codexPanel = parent.GetComponent<CodexPanel>();
            if (codexPanel == null) codexPanel = parent.gameObject.AddComponent<CodexPanel>();
            codexPanel.Configure(root, content, back);
            root.SetActive(false);
            return codexPanel;
        }

        private static AchievementPanel CreateAchievementPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("AchievementPanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(760f, 620f));

            Text title = CreateText("Title", panel.transform, "成就", 38, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-40f, 55f));

            Text content = CreateText("Content", panel.transform, "", 20, new Color32(255, 246, 224, 255), TextAnchor.UpperLeft);
            SetRect(content.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -110f), new Vector2(-80f, 380f));

            Button back = CreateSimpleButton("Back", panel.transform, new Vector2(0f, -255f), new Vector2(320f, 62f), "返回");

            AchievementPanel achievementPanel = parent.GetComponent<AchievementPanel>();
            if (achievementPanel == null) achievementPanel = parent.gameObject.AddComponent<AchievementPanel>();
            achievementPanel.Configure(root, content, back);
            root.SetActive(false);
            return achievementPanel;
        }

        private static SettingsPanel CreateSettingsPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("SettingsPanel", parent);
            GameObject panel = CreatePanelBox("Panel", root.transform, new Vector2(640f, 480f));

            Text title = CreateText("Title", panel.transform, "设置", 38, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-40f, 55f));

            Text volumeText = CreateText("VolumeText", panel.transform, "音量 100%", 24,
                new Color32(180, 172, 200, 255), TextAnchor.MiddleCenter);
            SetRect(volumeText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -115f), new Vector2(-40f, 36f));

            Button volumeButton = CreateSimpleButton("VolumeButton", panel.transform, new Vector2(0f, 30f), new Vector2(420f, 70f), "切换音量");

            Text fullscreenText = CreateText("FullscreenText", panel.transform, "全屏 开", 24,
                new Color32(180, 172, 200, 255), TextAnchor.MiddleCenter);
            SetRect(fullscreenText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -200f), new Vector2(-40f, 36f));

            Button fullscreenButton = CreateSimpleButton("FullscreenButton", panel.transform, new Vector2(0f, -55f), new Vector2(420f, 70f), "切换全屏");

            Button back = CreateSimpleButton("Back", panel.transform, new Vector2(0f, -170f), new Vector2(320f, 62f), "返回");

            SettingsPanel settingsPanel = parent.GetComponent<SettingsPanel>();
            if (settingsPanel == null) settingsPanel = parent.gameObject.AddComponent<SettingsPanel>();
            settingsPanel.Configure(root, volumeText, fullscreenText, volumeButton, fullscreenButton, back);
            root.SetActive(false);
            return settingsPanel;
        }

        private static ScreenFlash CreateScreenFlash(Transform parent)
        {
            GameObject root = CreateUiObject("ScreenFlash", parent);
            SetRect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image image = root.AddComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = false;

            ScreenFlash flash = parent.GetComponent<ScreenFlash>();
            if (flash == null) flash = parent.gameObject.AddComponent<ScreenFlash>();
            flash.Configure(image);
            return flash;
        }

        private static DevDataPanel CreateDevDataPanel(Transform parent)
        {
            GameObject root = CreateOverlayRoot("DevDataPanel", parent);
            root.GetComponent<Image>().color = new Color32(11, 10, 18, 235);

            Text content = CreateText("Text", root.transform, "DEV DATA", 20, new Color32(255, 246, 224, 255), TextAnchor.UpperLeft);
            SetRect(content.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -40f), new Vector2(-80f, 620f));

            DevDataPanel panel = parent.GetComponent<DevDataPanel>();
            if (panel == null) panel = parent.gameObject.AddComponent<DevDataPanel>();
            panel.Configure(root, content);
            root.SetActive(false);
            return panel;
        }

        private static Button CreateLabeledButton(string name, Transform parent, Vector2 position, Vector2 size, string initialText, out Text label)
        {
            GameObject go = CreateUiObject(name, parent);
            Image image = go.AddComponent<Image>();
            image.color = new Color32(58, 49, 80, 255);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = image;
            SetRect(go.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                position, size);

            label = CreateText("Label", go.transform, initialText, 24, new Color32(255, 246, 224, 255), TextAnchor.MiddleCenter);
            SetRect(label.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return button;
        }

        private static void CreateWall(string name, Vector2 position, Vector2 size)
        {
            GameObject wall = new GameObject(name);
            wall.transform.position = position;
            BoxCollider2D box = wall.AddComponent<BoxCollider2D>();
            box.size = size;

            Sprite wallSprite = LoadExternalSprite("Assets/ThirdParty/Kenney/Dungeon/wall_tile.png");
            if (wallSprite != null)
            {
                SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
                renderer.sprite = wallSprite;
                renderer.drawMode = SpriteDrawMode.Tiled;
                renderer.size = size;
                renderer.sortingOrder = -8;
                renderer.color = new Color(0.55f, 0.5f, 0.58f, 1f);
            }
        }

        private static void EnsureFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }
        }

        private static Sprite CreateSprite(string fileName, int size, System.Func<int, int, Color32> pixelFunc, bool repeat)
        {
            string assetPath = PlaceholderDir + "/" + fileName;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    pixels[y * size + x] = pixelFunc(x, y);
                }
            }

            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = size;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Sprite LoadExternalSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static AudioClip LoadAudioClip(string path)
        {
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }

        private static Color32 EnemyArrowPixel(int x, int y)
        {
            const int size = 16;
            float dx = x - (size - 1) * 0.5f;
            float dy = y - (size - 1) * 0.5f;
            bool shaft = Mathf.Abs(dy) <= 1f && dx <= 3f;
            bool head = dx > 3f && Mathf.Abs(dy) <= (7f - dx);
            return (shaft || head) ? new Color32(255, 220, 160, 255) : new Color32(0, 0, 0, 0);
        }

        private static Color32 CirclePixel(int x, int y)
        {
            const int size = 128;
            float dx = (x - (size - 1) * 0.5f) / (size * 0.5f);
            float dy = (y - (size - 1) * 0.5f) / (size * 0.5f);
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            float alpha = distance <= 1f ? 1f : Mathf.Clamp01(1f - (distance - 1f) * 6f);
            if (distance < 0.78f) alpha = 0.18f;
            return new Color32(255, 255, 255, (byte)(alpha * 255f));
        }

        private static Color32 GlowPixel(int x, int y)
        {
            const int size = 128;
            float dx = x - (size - 1) * 0.5f;
            float dy = y - (size - 1) * 0.5f;
            float distance = Mathf.Sqrt(dx * dx + dy * dy) / (size * 0.5f);
            float alpha = Mathf.Clamp01(1f - distance);
            alpha *= alpha;
            return new Color32(255, 255, 255, (byte)(alpha * 255f));
        }

        private static Color32 VignettePixel(int x, int y)
        {
            const int size = 256;
            float dx = (x - (size - 1) * 0.5f) / ((size - 1) * 0.5f);
            float dy = (y - (size - 1) * 0.5f) / ((size - 1) * 0.5f);
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            float alpha = Mathf.Clamp01((distance - 0.55f) / 0.75f) * 0.72f;
            return new Color32(0, 0, 0, (byte)(alpha * 255f));
        }

        private static Color32 PlayerPixel(int x, int y)
        {
            const int size = 32;
            Color32 fill = new Color32(52, 214, 190, 255);
            Color32 border = new Color32(14, 36, 40, 255);
            Color32 eye = new Color32(255, 246, 224, 255);

            bool isBorder = x < 2 || y < 2 || x >= size - 2 || y >= size - 2;
            bool isEye = (x >= 8 && x <= 12 && y >= 18 && y <= 22) ||
                         (x >= 20 && x <= 24 && y >= 18 && y <= 22);
            return isBorder ? border : (isEye ? eye : fill);
        }

        private static Color32 GroundPixel(int x, int y)
        {
            const int size = 32;
            Color32 baseColor = new Color32(24, 20, 33, 255);
            Color32 lineColor = new Color32(36, 30, 51, 255);
            bool isLine = x == 0 || y == 0;
            return isLine ? lineColor : baseColor;
        }

        private static Color32 SlimePixel(int x, int y)
        {
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 fill = new Color32(123, 224, 91, 255);
            Color32 border = new Color32(34, 72, 28, 255);
            Color32 eye = new Color32(255, 246, 224, 255);

            float dx = x - 15.5f;
            float dy = y - 12.5f;
            float normalized = (dx * dx) / (13f * 13f) + (dy * dy) / (10f * 10f);
            if (normalized > 1f) return transparent;

            bool isBorder = normalized > 0.72f;
            bool isEye = (x >= 10 && x <= 12 && y >= 13 && y <= 16) ||
                         (x >= 19 && x <= 21 && y >= 13 && y <= 16);
            return isBorder ? border : (isEye ? eye : fill);
        }

        private static Color32 ProjectilePixel(int x, int y)
        {
            const int size = 16;
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 fill = new Color32(255, 179, 71, 255);
            Color32 core = new Color32(255, 246, 224, 255);

            float dx = x - 7.5f;
            float dy = y - 7.5f;
            float distance = Mathf.Sqrt(dx * dx + dy * dy);
            if (distance > 7f) return transparent;
            if (distance > 5.5f) return fill;
            return distance < 2.5f ? core : fill;
        }

        private static Color32 GemPixel(int x, int y)
        {
            Color32 transparent = new Color32(0, 0, 0, 0);
            Color32 fill = new Color32(60, 224, 200, 255);
            Color32 border = new Color32(18, 88, 82, 255);
            Color32 core = new Color32(255, 246, 224, 255);

            int manhattan = Mathf.Abs(x - 7) + Mathf.Abs(y - 7);
            if (manhattan > 6) return transparent;
            if (manhattan >= 5) return border;
            if (manhattan <= 1) return core;
            return fill;
        }
    }
}
