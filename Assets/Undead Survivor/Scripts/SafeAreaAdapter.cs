using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaAdapter : MonoBehaviour
{
    [SerializeField]
    private RectTransform topSafeAreaBackground;

    private RectTransform rect;
    private bool wechatReady;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ApplySafeArea();
    }

    private void Start()
    {
#if (UNITY_WEBGL || WEIXINMINIGAME) && !UNITY_EDITOR
        // 普通浏览器运行时，不启用微信专用适配。
        if (string.IsNullOrEmpty(
            WeChatWASM.WX.GetSystemInfoSync().platform))
            return;

        WeChatWASM.WX.InitSDK(code =>
        {
            if (code != 200 || this == null)
                return;

            wechatReady = true;
            ApplySafeArea();
        });
#endif
    }

    private void OnRectTransformDimensionsChange()
    {
        if (rect != null && isActiveAndEnabled)
            ApplySafeArea();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        if (rect == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safe = Screen.safeArea;

        Vector2 min = new Vector2(
            safe.xMin / Screen.width,
            safe.yMin / Screen.height);

        Vector2 max = new Vector2(
            safe.xMax / Screen.width,
            safe.yMax / Screen.height);

#if (UNITY_WEBGL || WEIXINMINIGAME) && !UNITY_EDITOR
        if (wechatReady)
        {
            var window = WeChatWASM.WX.GetWindowInfo();
            var menu = WeChatWASM.WX.GetMenuButtonBoundingClientRect();

            float width = (float)window.windowWidth;
            float height = (float)window.windowHeight;

            if (width > 0 && height > 0)
            {
                // 微信坐标从左上角开始，Unity 锚点从左下角开始。
                if (window.safeArea != null)
                {
                    min.x = (float)window.safeArea.left / width;
                    max.x = (float)window.safeArea.right / width;
                    min.y = 1f - (float)window.safeArea.bottom / height;
                    max.y = 1f - (float)window.safeArea.top / height;
                }

                // HUD 整行放在胶囊下方，再留 8 个逻辑像素间距。
                if (menu.bottom > 0 && menu.bottom < height)
                {
                    max.y = Mathf.Min(
                        max.y,
                        1f - ((float)menu.bottom + 8f) / height);
                }
            }
        }
#endif

        min.x = Mathf.Clamp01(min.x);
        min.y = Mathf.Clamp01(min.y);
        max.x = Mathf.Clamp01(max.x);
        max.y = Mathf.Clamp01(max.y);

        if (max.x <= min.x || max.y <= min.y)
            return;

        UpdateTopBackground(max.y);

        // 避免重复修改布局。
        if (rect.anchorMin == min && rect.anchorMax == max &&
            rect.offsetMin == Vector2.zero &&
            rect.offsetMax == Vector2.zero)
            return;

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void UpdateTopBackground(float safeTop)
    {
        if (topSafeAreaBackground == null)
            return;

        // 黑条与安全区容器同属 Canvas，使用相同的归一化坐标。
        topSafeAreaBackground.anchorMin = new Vector2(0f, safeTop);
        topSafeAreaBackground.anchorMax = Vector2.one;
        topSafeAreaBackground.offsetMin = Vector2.zero;
        topSafeAreaBackground.offsetMax = Vector2.zero;
    }
}
