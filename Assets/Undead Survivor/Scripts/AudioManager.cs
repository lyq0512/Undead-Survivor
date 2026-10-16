using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private const string BgmVolumeKey = "UndeadSurvivor.Audio.BgmVolume";
    private const string SfxVolumeKey = "UndeadSurvivor.Audio.SfxVolume";

    // 供设置面板读取当前背景音乐音量。
    public float BgmVolume
    {
        get { return bgmVolume; }
    }

    // 供设置面板读取当前音效音量。
    public float SfxVolume
    {
        get { return sfxbgmVolume; }
    }

    [Header("BGM")]
    public AudioClip bgmClip;
    public float bgmVolume;
    private AudioSource bgmPlayer;
    AudioHighPassFilter bgmEffect;

    [Header("SFX")]
    public AudioClip[] sfxClips;
    public float sfxbgmVolume;
    public int channels;
    private AudioSource[] sfxPlayers;
    private int channelIndex;

    public enum Sfx { Dead, Hit, LevelUp = 3, Lose, Melee, Range = 7, Select, Win }

    private void Awake()
    {
        instance = this;
        Init();
        LoadVolumeSettings();
    }

    private void Init()
    {
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        bgmPlayer.clip = bgmClip;
        bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();

        GameObject sfxObject = new GameObject("sfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].bypassListenerEffects = true;
            sfxPlayers[index].volume = sfxbgmVolume;
        }
    }

    // 应用进入后台时保存音量。
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveVolumeSettings();
        }
    }

    // 正常退出应用时保存音量。
    private void OnApplicationQuit()
    {
        SaveVolumeSettings();
    }

    public void PlayBgm(bool isPlay)
    {
        if (bgmPlayer == null)
        {
            return;
        }

        if (isPlay)
        {
            // 已经播放时保持当前进度，避免重新开始。
            if (!bgmPlayer.isPlaying)
            {
                bgmPlayer.Play();
            }
        }
        else
        {
            bgmPlayer.Stop();
        }
    }

        // 设置背景音乐音量，范围为 0～1。
    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);

        if (bgmPlayer != null)
        {
            bgmPlayer.volume = bgmVolume;
        }
    }

        // 设置所有音效通道的音量，范围为 0～1。
    public void SetSfxVolume(float value)
    {
        sfxbgmVolume = Mathf.Clamp01(value);

        if (sfxPlayers == null)
        {
            return;
        }

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            if (sfxPlayers[index] != null)
            {
                sfxPlayers[index].volume = sfxbgmVolume;
            }
        }
    }

    // 读取已保存的音量；没有存档时使用 Inspector 中的默认值。
    public void LoadVolumeSettings()
    {
        float savedBgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, bgmVolume);
        float savedSfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxbgmVolume);

        SetBgmVolume(savedBgmVolume);
        SetSfxVolume(savedSfxVolume);
    }

    // 保存当前的背景音乐和音效音量。
    public void SaveVolumeSettings()
    {
        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxbgmVolume);

        PlayerPrefs.Save();
    }

    public void EffectBgm(bool isPlay)
    {
        bgmEffect.enabled = isPlay;
    }

    public void PlayerSfx(Sfx sfx)
    {
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            int ranIndex = 0;
            if (sfx == Sfx.Hit || sfx ==Sfx.Melee)
            {
                ranIndex = Random.Range(0, 2);
            }

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }
}
