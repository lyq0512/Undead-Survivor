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

    private void Awake()
    {
        achieves = (Achieve[])Enum.GetValues(typeof(Achieve));
        wait = new WaitForSecondsRealtime(5);

        if (!PlayerPrefs.HasKey("MyData"))
            Init();
    }

    private void Start()
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

    private void LateUpdate()
    {
        foreach (Achieve achieve in achieves)
        {
            CheckAchieve(achieve);
        }
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