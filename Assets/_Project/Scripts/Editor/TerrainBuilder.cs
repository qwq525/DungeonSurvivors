using System.Collections.Generic;
using DungeonSurvivors.Systems;
using UnityEditor;
using UnityEngine;

namespace DungeonSurvivors.EditorTools
{
    /// <summary>
    /// 用 Green Dungeon 32×32 图集搭建"伪高差"地形：草地 + 石台 + 崖壁碰撞 + 坡道开口 + 障碍物，
    /// 并生成 TerrainGrid 供敌人流场寻路。全部由编辑器脚本生成，可重复执行。
    /// </summary>
    public static class TerrainBuilder
    {
        private const int GridWidth = 30;
        private const int GridHeight = 18;
        private const int CellSize = 32;
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
            Sprite stone = Get(sprites, "green32_r0_c0");
            Sprite wall = Get(sprites, "green32_r7_c1");
            Sprite bush = Get(sprites, "green32_r3_c9");

            if (grass == null || stone == null || wall == null)
            {
                Debug.LogWarning("[Terrain] Required tiles missing, skip.");
                return;
            }

            bool[] blocked = new bool[GridWidth * GridHeight];
            GameObject root = new GameObject("Terrain");
            Transform rootTransform = root.transform;

            // 草地底
            CreateTiled("Grass", rootTransform, grass, Vector2.zero, new Vector2(GridWidth, GridHeight), -11, Color.white);

            // 石台（cell 坐标）：A 左中，B 右下
            BuildPlatform(rootTransform, blocked, stone, wall, new RectInt(6, 10, 8, 4), true, 9, 11);
            BuildPlatform(rootTransform, blocked, stone, wall, new RectInt(16, 4, 9, 4), false, 19, 21);

            // 障碍物（灌木/石头），带碰撞
            Vector2Int[] props = new Vector2Int[]
            {
                new Vector2Int(2, 3), new Vector2Int(27, 14), new Vector2Int(14, 2),
                new Vector2Int(3, 16), new Vector2Int(26, 5), new Vector2Int(12, 17),
                new Vector2Int(20, 1), new Vector2Int(1, 8),
                new Vector2Int(4, 8), new Vector2Int(25, 10), new Vector2Int(7, 2),
                new Vector2Int(23, 16), new Vector2Int(9, 6), new Vector2Int(18, 9),
                new Vector2Int(13, 14), new Vector2Int(5, 12), new Vector2Int(22, 2),
                new Vector2Int(15, 6), new Vector2Int(10, 15), new Vector2Int(27, 8)
            };
            foreach (Vector2Int cell in props)
            {
                if (blocked[cell.y * GridWidth + cell.x]) continue;
                blocked[cell.y * GridWidth + cell.x] = true;
                CreateProp(rootTransform, bush ?? wall, cell, -7);
            }

            // 边框不可走（物理墙另有 collider）
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

            GameObject oldGround = GameObject.Find("Ground");
            if (oldGround != null)
            {
                SpriteRenderer oldRenderer = oldGround.GetComponent<SpriteRenderer>();
                if (oldRenderer != null) oldRenderer.enabled = false;
            }

            string[] wallNames = { "Wall_Top", "Wall_Bottom", "Wall_Left", "Wall_Right" };
            foreach (string wallName in wallNames)
            {
                GameObject wallObject = GameObject.Find(wallName);
                if (wallObject == null) continue;
                SpriteRenderer wallRenderer = wallObject.GetComponent<SpriteRenderer>();
                if (wallRenderer != null) wallRenderer.enabled = false;
            }

            GameObject gridObject = new GameObject("TerrainGrid");
            TerrainGrid grid = gridObject.AddComponent<TerrainGrid>();
            grid.Configure(new Vector2(-GridWidth * 0.5f, -GridHeight * 0.5f), GridWidth, GridHeight, blocked);

            Debug.Log("[Terrain] Green dungeon terrain generated (" + GridWidth + "x" + GridHeight + ").");
        }

        private static void BuildPlatform(Transform parent, bool[] blocked, Sprite floor, Sprite wall,
            RectInt cells, bool rampOnSouth, int rampStartX, int rampEndX)
        {
            int xMin = cells.xMin;
            int yMin = cells.yMin;
            int xMax = cells.xMax - 1;
            int yMax = cells.yMax - 1;

            float centerX = (xMin + cells.width * 0.5f) - GridWidth * 0.5f;
            float centerY = (yMin + cells.height * 0.5f) - GridHeight * 0.5f;
            GameObject floorObject = new GameObject("Platform_" + xMin + "_" + yMin);
            floorObject.transform.SetParent(parent, false);
            SpriteRenderer floorRenderer = floorObject.AddComponent<SpriteRenderer>();
            floorRenderer.sprite = floor;
            floorRenderer.drawMode = SpriteDrawMode.Tiled;
            floorRenderer.size = new Vector2(cells.width, cells.height);
            floorRenderer.sortingOrder = -9;
            floorObject.transform.position = new Vector3(centerX, centerY, 0f);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if (x != xMin && x != xMax && y != yMin && y != yMax) continue;
                    bool isRamp = (rampOnSouth && y == yMin) || (!rampOnSouth && y == yMax);
                    isRamp = isRamp && x >= rampStartX && x <= rampEndX;
                    if (isRamp)
                    {
                        CreateCellTile(parent, floor, x, y, -8, new Color(0.92f, 0.9f, 0.95f, 1f), false);
                        continue;
                    }

                    blocked[y * GridWidth + x] = true;
                    CreateCellTile(parent, wall, x, y, -8, Color.white, true);
                }
            }
        }

        private static void CreateCellTile(Transform parent, Sprite sprite, int x, int y, int sortingOrder, Color color, bool collider)
        {
            GameObject tile = new GameObject("Tile_" + x + "_" + y);
            tile.transform.SetParent(parent, false);
            tile.transform.position = CellToWorld(x, y);
            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;

            if (collider)
            {
                BoxCollider2D box = tile.AddComponent<BoxCollider2D>();
                box.size = new Vector2(0.96f, 0.96f);
            }
        }

        private static void CreateProp(Transform parent, Sprite sprite, Vector2Int cell, int sortingOrder)
        {
            GameObject prop = new GameObject("Prop_" + cell.x + "_" + cell.y);
            prop.transform.SetParent(parent, false);
            prop.transform.position = CellToWorld(cell.x, cell.y);
            SpriteRenderer renderer = prop.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            BoxCollider2D box = prop.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.8f, 0.8f);
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
            return new Vector2(
                x - GridWidth * 0.5f + 0.5f,
                y - GridHeight * 0.5f + 0.5f);
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
