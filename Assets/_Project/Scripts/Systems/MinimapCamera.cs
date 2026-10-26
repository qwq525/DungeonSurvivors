using DungeonSurvivors.Core;
using DungeonSurvivors.Gameplay.Player;
using UnityEngine;
using UnityEngine.UI;

namespace DungeonSurvivors.Systems
{
    /// <summary>
    /// 右上角小地图：独立正交摄像机 + RenderTexture + UI RawImage，跟随玩家。
    /// 面试可讲点：多相机/RenderTexture 的用法、按玩家位置跟随、暂停/菜单时隐藏。
    /// </summary>
    public class MinimapCamera : MonoBehaviour
    {
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private RawImage display;
        [SerializeField] private GameObject root;
        [SerializeField] private Transform target;
        [SerializeField] private int textureSize = 256;
        [SerializeField] private float orthographicSize = 6.5f;

        private RenderTexture renderTexture;

        public void Configure(Camera cameraComponent, RawImage rawImage, GameObject panelRoot, Transform followTarget, float size = 6.5f)
        {
            minimapCamera = cameraComponent;
            display = rawImage;
            root = panelRoot;
            target = followTarget;
            orthographicSize = size;
        }

        private void Start()
        {
            if (minimapCamera == null) minimapCamera = GetComponent<Camera>();
            if (minimapCamera == null) return;

            renderTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
            renderTexture.name = "MinimapRT";
            minimapCamera.targetTexture = renderTexture;
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = orthographicSize;
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color32(10, 8, 14, 255);

            if (display != null) display.texture = renderTexture;
        }

        private void LateUpdate()
        {
            if (root != null)
            {
                bool inMenu = GameManager.Instance != null && GameManager.Instance.State == GameState.Menu;
                if (root.activeSelf == inMenu) root.SetActive(!inMenu);
                if (minimapCamera != null) minimapCamera.enabled = !inMenu;
            }

            if (target == null)
            {
                PlayerController player = PlayerController.Instance;
                if (player != null) target = player.transform;
            }

            if (target != null)
            {
                transform.position = new Vector3(target.position.x, target.position.y, -10f);
            }
        }

        private void OnDestroy()
        {
            if (minimapCamera != null) minimapCamera.targetTexture = null;
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}
