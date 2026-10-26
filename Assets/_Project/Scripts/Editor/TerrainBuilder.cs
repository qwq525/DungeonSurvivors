using System.Collections.Generic;
using DungeonSurvivors.Systems;
using UnityEditor;
using UnityEngine;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// 场景优化：只使用 Green Dungeon 32×32 一套素材，生成大场地草地 + 均匀分布的灌木/石块障碍，
    /// 不做高低差；生成 TerrainGrid 供敌人流场寻路。
    /// </summary>
    public static class TerrainBuilder
    {
        private const int GridWidth = 48;
        private const int GridHeight = 28;
        private const string AtlasPath = "Assets/ThirdParty/Craftpix/GreenDungeon/green32.png";

        public static void Build()
        {
            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(AtlasPath))
            {
                Debug.LogWarning("[Terrain] Green dungeon atlas not found, skip.");
                return;
            }

            Dictionary<string, Sprite> sprites = LoadSprites(AtlasPath);
            Sprite grass = Get(sprites, "green32_r0_c1");
            Sprite bush = Get(sprites, "green32_r3_c9");
            Sprite flower = Get(sprites, "green32_r3_c11");
            Sprite rock = Get(sprites, "green32_r11_c7");
            Sprite barrel = Get(sprites, "green32_r5_c5");
            Sprite mud = Get(sprites, "green32_r14_c7");
            Sprite wall = Get(sprites, "green32_r7_c1");
            Sprite grassPatch = Get(sprites, "green32_r0_c3");
            Sprite shadow = LoadSingleSprite("Assets/_Project/Art/Placeholders/glow_placeholder.png");
            if (grass == null) return;

            bool[] blocked = new bool[GridWidth * GridHeight];
            GameObject root = new GameObject("Terrain");
            Transform rootTransform = root.transform;

            CreateTiled("Grass", rootTransform, grass, Vector2.zero, new Vector2(GridWidth, GridHeight), -10, Color.white);

            if (wall != null)
            {
                float halfW = GridWidth * 0.5f;
                float halfH = GridHeight * 0.5f;
                CreateTiled("BorderTop", rootTransform, wall, new Vector2(0f, halfH - 0.5f), new Vector2(GridWidth, 1f), -8, Color.white);
                CreateTiled("BorderBottom", rootTransform, wall, new Vector2(0f, -halfH + 0.5f), new Vector2(GridWidth, 1f), -8, Color.white);
                CreateTiled("BorderLeft", rootTransform, wall, new Vector2(-halfW + 0.5f, 0f), new Vector2(1f, GridHeight), -8, Color.white);
                CreateTiled("BorderRight", rootTransform, wall, new Vector2(halfW - 0.5f, 0f), new Vector2(1f, GridHeight), -8, Color.white);
            }

            if (mud != null)
            {
                int[,] patches = { { 7, 5 }, { 20, 8 }, { 34, 19 }, { 12, 21 }, { 40, 12 } };
                for (int i = 0; i < patches.GetLength(0); i++)
                {
                    CreateTiled("Mud_" + i, rootTransform, mud,
                        CellToWorld(patches[i, 0], patches[i, 1]), new Vector2(3f, 2f), -9, new Color(0.95f, 0.9f, 0.9f, 1f));
                }
            }

            System.Random random = new System.Random(20261026);
            int obstacleIndex = 0;
            for (int gx = 3; gx < GridWidth - 2; gx += 4)
            {
                for (int gy = 3; gy < GridHeight - 2; gy += 4)
                {
                    int x = Mathf.Clamp(gx + random.Next(-2, 3), 2, GridWidth - 3);
                    int y = Mathf.Clamp(gy + random.Next(-2, 3), 2, GridHeight - 3);
                    Vector2 world = CellToWorld(x, y);
                    if (Vector2.Distance(world, Vector2.zero) < 2.5f) continue;
                    if (blocked[y * GridWidth + x]) continue;

                    Sprite sprite;
                    switch (obstacleIndex % 3)
                    {
                        case 0:
                            sprite = rock ?? bush;
                            break;
                        case 1:
                            sprite = barrel ?? bush;
                            break;
                        default:
                            sprite = bush ?? rock;
                            break;
                    }
                    if (sprite == null) sprite = bush ?? rock;
                    if (sprite == null) continue;

                    blocked[y * GridWidth + x] = true;
                    CreateObstacle(rootTransform, sprite, x, y, obstacleIndex);
                    obstacleIndex++;
                }
            }

            if (flower != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    int x = random.Next(2, GridWidth - 2);
                    int y = random.Next(2, GridHeight - 2);
                    if (blocked[y * GridWidth + x]) continue;
                    CreateDeco(rootTransform, flower, x, y, i);
                }
            }

            for (int x = 0; x < GridWidth; x++)
            {
                blocked[x] = true;
                blocked[(GridHeight - 1) * GridWidth + x] = true;
            }
            for (int y = 0; y < GridHeight; y++)
            {
                blocked[y * GridWidth] = true;
                blocked[y * GridWidth + GridWidth - 1] = true;
            }

            HideLegacyGround();

            GameObject gridObject = new GameObject("TerrainGrid");
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.Configure(new Vector2(-GridWidth * 0.5f, -GridHeight * 0.5f), GridWidth, GridHeight, blocked);

            Debug.Log("[Terrain] Large scene generated (" + GridWidth + "x" + GridHeight + "), obstacles=" + obstacleIndex + ".");
        }

        private static void HideLegacyGround()
        {
            GameObject oldGround = GameObject.Find("Ground");
            if (oldGround != null)
            {
                SpriteRenderer renderer = oldGround.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
            }

            string[] wallNames = { "Wall_Top", "Wall_Bottom", "Wall_Left", "Wall_Right" };
            foreach (string wallName in wallNames)
            {
                GameObject wall = GameObject.Find(wallName);
                if (wall == null) continue;
                SpriteRenderer renderer = wall.GetComponent<SpriteRenderer>();
                if (renderer != null) renderer.enabled = false;
            }
        }

        private static void CreateObstacle(Transform parent, Sprite sprite, int x, int y, int index)
        {
            GameObject go = new GameObject("Obstacle_" + index);
            go.transform.SetParent(parent, false);
            go.transform.position = CellToWorld(x, y);
            go.transform.localScale = Vector3.one * 1.5f;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -7;
            renderer.color = index % 3 == 0 ? new Color(0.8f, 0.8f, 0.95f, 1f) : new Color(0.78f, 0.92f, 0.75f, 1f);

            Sprite shadow = LoadSingleSprite("Assets/_Project/Art/Placeholders/glow_placeholder.png");
            if (shadow != null)
            {
                GameObject shadowObject = new GameObject("Shadow");
                shadowObject.transform.SetParent(go.transform, false);
                shadowObject.transform.localPosition = new Vector3(0f, -0.35f, 0f);
                shadowObject.transform.localScale = new Vector3(1.1f, 0.5f, 1f);
                SpriteRenderer shadowRenderer = shadowObject.AddComponent<SpriteRenderer>();
                shadowRenderer.sprite = shadow;
                shadowRenderer.color = new Color(0f, 0f, 0f, 0.42f);
                shadowRenderer.sortingOrder = -8;
            }

            BoxCollider2D box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 0.8f);
        }

        private static void CreateDeco(Transform parent, Sprite sprite, int x, int y, int index)
        {
            GameObject go = new GameObject("Deco_" + index);
            go.transform.SetParent(parent, false);
            go.transform.position = CellToWorld(x, y);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -8;
        }

        private static void CreateTiled(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, int sortingOrder, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = size;
            renderer.sortingOrder = sortingOrder;
            renderer.color = color;
        }

        private static Vector2 CellToWorld(int x, int y)
        {
            return new Vector2(x - GridWidth * 0.5f + 0.5f, y - GridHeight * 0.5f + 0.5f);
        }

        private static Sprite LoadSingleSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Dictionary<string, Sprite> LoadSprites(string path)
        {
            Dictionary<string, Sprite> result = new Dictionary<string, Sprite>();
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                Sprite sprite = asset as Sprite;
                if (sprite != null) result[sprite.name] = sprite;
            }

            return result;
        }

        private static Sprite Get(Dictionary<string, Sprite> sprites, string name)
        {
            Sprite sprite;
            return sprites.TryGetValue(name, out sprite) ? sprite : null;
        }
    }
}
