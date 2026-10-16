using System.Collections;
using UnityEngine;

public enum UIPanelState
{
    Hidden,
    Opening,
    Visible,
    Closing
}

[RequireComponent(typeof(CanvasGroup))]
public class UIPanel : MonoBehaviour
{
    [Header("动画时长，单位：秒")]
    [Min(0f)]
    [SerializeField] private float openDuration = 0.4f;

    [Min(0f)]
    [SerializeField] private float closeDuration = 0.3f;

    private CanvasGroup panelGroup;

    // 每次新操作都会改变编号，让旧动画停止更新。
    private int transitionVersion;

    public UIPanelState State { get; private set; }

    private void Awake()
    {
        panelGroup = GetComponent<CanvasGroup>();

        State = panelGroup.alpha > 0f
            ? UIPanelState.Visible
            : UIPanelState.Hidden;
    }

    // 立即显示，同时中断旧动画。
    public void ShowImmediate()
    {
        transitionVersion++;

        panelGroup.alpha = 1f;
        panelGroup.interactable = true;
        panelGroup.blocksRaycasts = true;

        State = UIPanelState.Visible;
    }

    // 立即隐藏，同时中断旧动画。
    public void HideImmediate()
    {
        transitionVersion++;

        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
        panelGroup.alpha = 0f;

        State = UIPanelState.Hidden;
    }

    // 由控制器启动并等待打开动画。
    public IEnumerator Show()
    {
        return Fade(true, openDuration);
    }

    // 由控制器启动并等待关闭动画。
    public IEnumerator Hide()
    {
        return Fade(false, closeDuration);
    }

    private IEnumerator Fade(bool visible, float duration)
    {
        int version = ++transitionVersion;

        float startAlpha = panelGroup.alpha;
        float targetAlpha = visible ? 1f : 0f;
        float elapsed = 0f;

        State = visible
            ? UIPanelState.Opening
            : UIPanelState.Closing;

        // 动画期间禁止操作面板内的按钮。
        panelGroup.interactable = false;

        // 关闭完成前仍然阻挡下层点击。
        panelGroup.blocksRaycasts = true;

        while (elapsed < duration)
        {
            if (version != transitionVersion)
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            panelGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                easedProgress);

            yield return null;
        }

        if (version != transitionVersion)
        {
            yield break;
        }

        panelGroup.alpha = targetAlpha;
        panelGroup.interactable = visible;
        panelGroup.blocksRaycasts = visible;

        State = visible
            ? UIPanelState.Visible
            : UIPanelState.Hidden;
    }
}