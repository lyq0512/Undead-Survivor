using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchieveManager : MonoBehaviour
{
    public GameObject[] lockCharacter;
    public GameObject[] unlockCharacter;
    public GameObject uiNotice;

    enum Achieve { unlockJasper, unlockTater }
    Achieve[] achieves;
    WaitForSecondsRealtime wait;
    private GameManager eventSource;

    private void Awake()
    {
        achieves = (Achieve[])Enum.GetValues(typeof(Achieve));
        wait = new WaitForSecondsRealtime(5);

        if (!PlayerPrefs.HasKey("MyData"))
            Init();
    }

    private void OnEnable()
    {
        BindEvents();
    }

    private void Start()
    {
        UnlockCharacter();

        // 确保首次加载时 GameManager.Awake 已执行。
        BindEvents();
    }

    private void OnDisable()
    {
        UnbindEvents();
    }

    private void BindEvents()
    {
        GameManager source = GameManager.instance;

        if (source == null || eventSource == source)
            return;

        UnbindEvents();

        eventSource = source;
        eventSource.KillChanged += OnKillChanged;
        eventSource.VictoryAchieved += OnVictoryAchieved;

        // 首次绑定或重新启用时，补查当前状态。
        OnKillChanged();
        OnVictoryAchieved();
    }

    private void UnbindEvents()
    {
        if (eventSource != null)
        {
            eventSource.KillChanged -= OnKillChanged;
            eventSource.VictoryAchieved -= OnVictoryAchieved;
        }

        eventSource = null;
    }

    private void OnKillChanged()
    {
        CheckAchieve(Achieve.unlockJasper);
    }

    private void OnVictoryAchieved()
    {
        CheckAchieve(Achieve.unlockTater);
    }

        // 查询指定角色是否已经解锁。
    public bool IsCharacterUnlocked(int characterId)
    {
        switch (characterId)
        {
            case 0:
            case 1:
                return true;

            case 2:
                return PlayerPrefs.GetInt("unlockJasper", 0) == 1;

            case 3:
                return PlayerPrefs.GetInt("unlockTater", 0) == 1;

            default:
                return false;
        }
    }

    // 供角色选择面板主动刷新锁定和解锁状态。
    public void RefreshCharacterViews()
    {
        UnlockCharacter();
    }

    private void UnlockCharacter()
    {
        for (int index = 0; index < lockCharacter.Length; index++)
        {
            string achieveName = achieves[index].ToString();
            bool isUnlock = PlayerPrefs.GetInt(achieveName) == 1;
            lockCharacter[index].SetActive(!isUnlock);
            unlockCharacter[index].SetActive(isUnlock);
        }
    }

    private void Init()
    {
        PlayerPrefs.SetInt("MyData", 1);

        foreach (Achieve achieve in achieves)
        {
            PlayerPrefs.SetInt(achieve.ToString(), 0);
        }

        //PlayerPrefs.SetInt("unlockJasper", 0);
        //PlayerPrefs.SetInt("unlockTater", 0);
    }

    private void CheckAchieve(Achieve achieve)
    {
        bool isAchieve = false;

        switch (achieve)
        {
            case Achieve.unlockJasper:
                isAchieve = GameManager.instance.kill >= 10;
                break;
            case Achieve.unlockTater:
                isAchieve = GameManager.instance.gameTime == GameManager.instance.maxGameTime;
                break;
        }

        if (isAchieve && PlayerPrefs.GetInt(achieve.ToString()) == 0)
        {
            PlayerPrefs.SetInt(achieve.ToString(), 1);

            for (int index = 0; index < uiNotice.transform.childCount; index++)
            {
                bool active = index == (int)achieve;
                uiNotice.transform.GetChild(index).gameObject.SetActive(active);
            }

            StartCoroutine(NoticeRoutine());
        }
    }
    IEnumerator NoticeRoutine()
    {
        uiNotice.SetActive(true);
        AudioManager.instance.PlayerSfx(AudioManager.Sfx.LevelUp);

        yield return wait;

        uiNotice.SetActive(false);
    }
}