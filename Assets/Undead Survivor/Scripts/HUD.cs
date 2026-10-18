using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public enum InfoType { Exp, Level, Kill, Time, Health }
    public InfoType type;

    private Text myText;
    private Slider mySlider;
    private GameManager healthSource;
    private GameManager timeSource;
    private int displayedRemainingSeconds = int.MinValue;
    private GameManager killSource;
    private int displayedKill = int.MinValue;
    private GameManager experienceSource;
    private int displayedLevel = int.MinValue;

    private void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        BindHealth();
        BindTime();
        BindKill();
        BindExperience();
    }

    private void Start()
    {
        // 首次启用时，确保 GameManager.Awake 已执行。
        BindHealth();
        BindTime();
        BindKill();
        BindExperience();
    }

    private void OnDisable()
    {
        if (healthSource != null)
        {
            healthSource.HealthChanged -= RefreshHealth;
        }

        if (timeSource != null)
        {
            timeSource.RemainingSecondsChanged -= RefreshTime;
        }

        if (killSource != null)
        {
            killSource.KillChanged -= RefreshKill;
        }


        if (experienceSource != null)
        {
            experienceSource.ExperienceChanged -= RefreshExperience;
        }

        experienceSource = null;
        displayedLevel = int.MinValue;
        healthSource = null;
        timeSource = null;
        killSource = null;
        displayedRemainingSeconds = int.MinValue;
        displayedKill = int.MinValue;
    }

    private void BindHealth()
    {
        if (type != InfoType.Health || GameManager.instance == null)
            return;

        if (healthSource != GameManager.instance)
        {
            if (healthSource != null)
            {
                healthSource.HealthChanged -= RefreshHealth;
            }

            healthSource = GameManager.instance;
            healthSource.HealthChanged += RefreshHealth;
        }

        RefreshHealth();
    }

    private void RefreshHealth()
    {
        if (healthSource == null || mySlider == null)
            return;

        mySlider.value = healthSource.maxHealth > 0f
            ? healthSource.health / healthSource.maxHealth
            : 0f;
    }

        private void BindTime()
    {
        if (type != InfoType.Time || GameManager.instance == null)
            return;

        if (timeSource != GameManager.instance)
        {
            if (timeSource != null)
            {
                timeSource.RemainingSecondsChanged -= RefreshTime;
            }

            timeSource = GameManager.instance;
            timeSource.RemainingSecondsChanged += RefreshTime;
            displayedRemainingSeconds = int.MinValue;
        }

        int remainingSeconds = Mathf.Max(
            0,
            Mathf.FloorToInt(timeSource.maxGameTime - timeSource.gameTime));

        RefreshTime(remainingSeconds);
    }

    private void RefreshTime(int remainingSeconds)
    {
        if (myText == null || remainingSeconds == displayedRemainingSeconds)
            return;

        int min = remainingSeconds / 60;
        int sec = remainingSeconds % 60;
        myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
        displayedRemainingSeconds = remainingSeconds;
    }

        private void BindKill()
    {
        if (type != InfoType.Kill || GameManager.instance == null)
            return;

        if (killSource != GameManager.instance)
        {
            if (killSource != null)
            {
                killSource.KillChanged -= RefreshKill;
            }

            killSource = GameManager.instance;
            killSource.KillChanged += RefreshKill;
            displayedKill = int.MinValue;
        }

        RefreshKill();
    }

    private void RefreshKill()
    {
        if (killSource == null || myText == null)
            return;

        int currentKill = killSource.kill;
        if (currentKill == displayedKill)
            return;

        myText.text = currentKill.ToString();
        displayedKill = currentKill;
    }

        private void BindExperience()
    {
        if ((type != InfoType.Exp && type != InfoType.Level) ||
            GameManager.instance == null)
            return;

        if (experienceSource != GameManager.instance)
        {
            if (experienceSource != null)
            {
                experienceSource.ExperienceChanged -= RefreshExperience;
            }

            experienceSource = GameManager.instance;
            experienceSource.ExperienceChanged += RefreshExperience;
            displayedLevel = int.MinValue;
        }

        RefreshExperience();
    }

    private void RefreshExperience()
    {
        if (experienceSource == null)
            return;

        if (type == InfoType.Exp && mySlider != null)
        {
            int index = Mathf.Min(
                experienceSource.level,
                experienceSource.nextExp.Length - 1);

            int requiredExp = experienceSource.nextExp[index];

            mySlider.value = requiredExp > 0
                ? (float)experienceSource.exp / requiredExp
                : 0f;
        }
        else if (type == InfoType.Level && myText != null)
        {
            int currentLevel = experienceSource.level;

            if (currentLevel == displayedLevel)
                return;

            myText.text = "Lv." + currentLevel.ToString();
            displayedLevel = currentLevel;
        }
    }
}