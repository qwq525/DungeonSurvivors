using System.Collections.Generic;
using DungeonSurvivors.Gameplay.Player;
using UnityEngine;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 地形导航网格：记录墙/断崖/障碍，按玩家位置定期 BFS 出流场，敌人沿流场绕路。
    /// 面试可讲点：为什么用流场而不是每个敌人 A*（多敌人共享一次计算）；与 Tilemap/碰撞解耦。
    /// </summary>
    public class TerrainGrid : MonoBehaviour
    {
        public static TerrainGrid Instance { get; private set; }

        [SerializeField] private Vector2 origin = new Vector2(-15f, -9f);
        [SerializeField] private int width = 30;
        [SerializeField] private int height = 18;
        [SerializeField] private bool[] blocked;
        [SerializeField] private float refreshInterval = 0.3f;

        private int[] distance;
        private float refreshTimer;
        private readonly Queue<int> queue = new Queue<int>();

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Configure(Vector2 gridOrigin, int gridWidth, int gridHeight, bool[] blockedCells)
        {
            origin = gridOrigin;
            width = Mathf.Max(1, gridWidth);
            height = Mathf.Max(1, gridHeight);
            blocked = blockedCells;
            distance = new int[width * height];
        }

        public bool IsWalkable(Vector2 world)
        {
            int x, y;
            return TryGetCell(world, out x, out y) && !IsBlocked(x, y);
        }

        public bool IsBlocked(int x, int y)
        {
            if (blocked == null || x < 0 || y < 0 || x >= width || y >= height) return true;
            return blocked[y * width + x];
        }

        public bool TryGetCell(Vector2 world, out int x, out int y)
        {
            x = Mathf.FloorToInt((world.x - origin.x) / 1f);
            y = Mathf.FloorToInt((world.y - origin.y) / 1f);
            return x >= 0 && y >= 0 && x < width && y < height;
        }

        public Vector2 CellCenter(int x, int y)
        {
            return origin + new Vector2(x + 0.5f, y + 0.5f);
        }

        private void Update()
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer > 0f) return;
            refreshTimer = Mathf.Max(0.1f, refreshInterval);
            RebuildFlowField();
        }

        private void RebuildFlowField()
        {
            if (distance == null || blocked == null) return;

            int size = width * height;
            for (int i = 0; i < size; i++) distance[i] = int.MaxValue;

            PlayerController player = PlayerController.Instance;
            if (player == null) return;

            int px, py;
            if (!TryGetCell(player.transform.position, out px, out py)) return;
            if (IsBlocked(px, py)) return;

            int start = py * width + px;
            distance[start] = 0;
            queue.Clear();
            queue.Enqueue(start);

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int cx = index % width;
                int cy = index / width;
                int nextDistance = distance[index] + 1;

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = cx + dx;
                        int ny = cy + dy;
                        if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                        if (IsBlocked(nx, ny)) continue;
                        int neighbor = ny * width + nx;
                        if (nextDistance >= distance[neighbor]) continue;
                        distance[neighbor] = nextDistance;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        /// <summary>沿流场返回朝玩家的下一步方向；找不到时返回 zero。</summary>
        public Vector2 GetDirection(Vector2 world)
        {
            int x, y;
            if (distance == null || !TryGetCell(world, out x, out y) || IsBlocked(x, y)) return Vector2.zero;
            int current = distance[y * width + x];
            if (current == int.MaxValue) return Vector2.zero;

            int bestX = x;
            int bestY = y;
            int best = current;
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = x + dx;
                    int ny = y + dy;
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    if (IsBlocked(nx, ny)) continue;
                    int value = distance[ny * width + nx];
                    if (value < best)
                    {
                        best = value;
                        bestX = nx;
                        bestY = ny;
                    }
                }
            }

            if (best == current) return Vector2.zero;
            Vector2 target = CellCenter(bestX, bestY);
            Vector2 direction = target - world;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.zero;
        }

        public Vector2 FindSpawnPosition(Vector2 from, float minDistance)
        {
            if (blocked == null) return from + Vector2.right * 8f;

            for (int attempt = 0; attempt < 64; attempt++)
            {
                int x = Random.Range(0, width);
                int y = Random.Range(0, height);
                if (IsBlocked(x, y)) continue;
                Vector2 candidate = CellCenter(x, y);
                if (Vector2.Distance(candidate, from) < minDistance) continue;
                return candidate;
            }

            return FindNearestWalkable(from + Random.insideUnitCircle.normalized * 8f);
        }

        public Vector2 FindNearestWalkable(Vector2 world)
        {
            int cx, cy;
            if (!TryGetCell(world, out cx, out cy)) return world;

            for (int radius = 1; radius <= 12; radius++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int x = cx + dx;
                        int y = cy + dy;
                        if (x < 0 || y < 0 || x >= width || y >= height) continue;
                        if (!IsBlocked(x, y)) return CellCenter(x, y);
                    }
                }
            }

            return world;
        }
    }
}
