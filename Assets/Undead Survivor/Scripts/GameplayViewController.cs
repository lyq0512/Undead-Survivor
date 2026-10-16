using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class GameplayViewController : MonoBehaviour
{
    [Header("游戏摄像机")]
    [SerializeField] private Camera gameplayCamera;

    [Header("画面纹理模板")]
    [SerializeField] private RenderTexture renderTextureTemplate;

    [Header("游戏顶部信息栏")]
    [SerializeField] private RectTransform topBar;

    private RawImage view;
    private RectTransform viewRect;
    private Canvas rootCanvas;

    private RenderTexture runtimeTexture;
    private RenderTexture previousCameraTexture;
    private Texture previousViewTexture;
    private Color previousViewColor;

    private bool layoutDirty = true;

    private void Awake()
    {
        view = GetComponent<RawImage>();
        viewRect = GetComponent<RectTransform>();

        if (view.canvas != null)
        {
            rootCanvas = view.canvas.rootCanvas;
        }
    }

    private void Start()
    {
        if (gameplayCamera == null ||
            renderTextureTemplate == null ||
            rootCanvas == null)
        {
            Debug.LogError(
                "GameplayViewController：请绑定摄像机、纹理模板，并放在 Canvas 下。",
                this);
            return;
        }

        previousCameraTexture = gameplayCamera.targetTexture;
        previousViewTexture = view.texture;
        previousViewColor = view.color;

        Canvas.ForceUpdateCanvases();

        Canvas.willRenderCanvases += RefreshIfNeeded;
        RefreshIfNeeded();
    }

    // 菜单时铺满画面，游戏中为顶部信息栏留出空间。
    public void SetGameplayLayout(bool isGameplay)
    {
        if (viewRect == null || topBar == null)
        {
            Debug.LogError(
                "GameplayViewController：请检查显示区域和 TopBar 引用。",
                this);
            return;
        }

        float topInset = isGameplay
            ? Mathf.Max(0f, topBar.rect.height)
            : 0f;

        Vector2 offset = viewRect.offsetMax;
        offset.y = -topInset;
        viewRect.offsetMax = offset;

        layoutDirty = true;
    }

    // 显示区域尺寸变化时，标记需要重新匹配纹理尺寸。
    private void OnRectTransformDimensionsChange()
    {
        layoutDirty = true;
    }

    private void RefreshIfNeeded()
    {
        if (!layoutDirty ||
            gameplayCamera == null ||
            renderTextureTemplate == null ||
            rootCanvas == null)
        {
            return;
        }

        int width = Mathf.Clamp(
            Mathf.RoundToInt(viewRect.rect.width * rootCanvas.scaleFactor),
            2,
            SystemInfo.maxTextureSize);

        int height = Mathf.Clamp(
            Mathf.RoundToInt(viewRect.rect.height * rootCanvas.scaleFactor),
            2,
            SystemInfo.maxTextureSize);

        if (runtimeTexture != null &&
            runtimeTexture.width == width &&
            runtimeTexture.height == height)
        {
            layoutDirty = false;
            return;
        }

        if (runtimeTexture == null)
        {
            runtimeTexture = new RenderTexture(renderTextureTemplate);
            runtimeTexture.name = "Gameplay Runtime Texture";
        }
        else
        {
            runtimeTexture.Release();
        }

        runtimeTexture.width = width;
        runtimeTexture.height = height;
        runtimeTexture.antiAliasing = 1;
        runtimeTexture.filterMode = FilterMode.Point;
        runtimeTexture.wrapMode = TextureWrapMode.Clamp;
        runtimeTexture.useDynamicScale = false;
        runtimeTexture.useMipMap = false;
        runtimeTexture.autoGenerateMips = false;

        if (!runtimeTexture.Create())
        {
            Debug.LogError("GameplayViewController：创建游戏画面纹理失败。", this);
            return;
        }

        gameplayCamera.targetTexture = runtimeTexture;
        view.texture = runtimeTexture;
        view.color = Color.white;

        layoutDirty = false;
    }

    private void OnDestroy()
    {
        Canvas.willRenderCanvases -= RefreshIfNeeded;

        if (runtimeTexture == null)
        {
            return;
        }

        if (gameplayCamera != null &&
            gameplayCamera.targetTexture == runtimeTexture)
        {
            gameplayCamera.targetTexture = previousCameraTexture;
        }

        if (view != null && view.texture == runtimeTexture)
        {
            view.texture = previousViewTexture;
            view.color = previousViewColor;
        }

        runtimeTexture.Release();
        Destroy(runtimeTexture);
    }
}