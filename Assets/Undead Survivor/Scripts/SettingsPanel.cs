using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("音频管理器")]
    [SerializeField] private AudioManager audioManager;

    [Header("音量滑动条")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        RefreshFromAudio();
    }

    // 将当前音量显示到两个滑动条上。
    public void RefreshFromAudio()
    {
        if (audioManager == null || bgmSlider == null || sfxSlider == null)
        {
            Debug.LogError("SettingsPanel：请绑定音频管理器和两个滑动条。", this);
            return;
        }

        bgmSlider.SetValueWithoutNotify(audioManager.BgmVolume);
        sfxSlider.SetValueWithoutNotify(audioManager.SfxVolume);
    }

    // BGM 滑动条数值变化时调用。
    public void OnBgmVolumeChanged(float value)
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.SetBgmVolume(value);
    }

    // SFX 滑动条数值变化时调用。
    public void OnSfxVolumeChanged(float value)
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.SetSfxVolume(value);
    }

    // 保存当前音量。
    public void Save()
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.SaveVolumeSettings();
    }
}