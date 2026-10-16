using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreenController : MonoBehaviour
{
    public enum TitlePanelState
    {
        MainMenu,
        CharacterSelect,
        Settings,
        PauseMenu,
        Closed
    }

    [Header("菜单面板")]
    [SerializeField] private CanvasGroup mainMenuPanel;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup characterSelectPanel;
    [SerializeField] private CanvasGroup pauseMenuPanel;
    [SerializeField] private UIPanel pausePanelUI;
    [SerializeField] private UIPanel mainMenuPanelUI;
    [SerializeField] private UIPanel characterSelectPanelUI;

    [Header("设置面板脚本")]
    [SerializeField] private SettingsPanel settingsView;
    [SerializeField] private UIPanel settingsPanelUI;

    [Header("角色选择脚本")]
    [SerializeField] private CharacterSelectPanel characterSelectView;

    [Header("游戏管理器")]
    [SerializeField] private GameManager gameManager;

    [Header("进入游戏时切换的界面")]
    [SerializeField] private CanvasGroup titleScreenRoot;
    [SerializeField] private GameObject gameplayHud;
    [SerializeField] private GameplayViewController gameplayViewController;

    private bool isStartingGame;
    // 重新加载场景后，是否直接打开角色选择。
    private static bool openCharacterSelectOnStart;

    private TitlePanelState currentState = TitlePanelState.MainMenu;
    private TitlePanelState settingsReturnState = TitlePanelState.MainMenu;
    private bool isInitialized;
    // 正在切换面板时，忽略重复导航操作。
    private bool isTransitioning;

    public TitlePanelState CurrentState
    {
        get { return currentState; }
    }

    private void Start()
    {
        if (mainMenuPanel == null ||
            settingsPanel == null ||
            characterSelectPanel == null ||
            settingsView == null ||
            settingsPanelUI == null ||
            mainMenuPanelUI == null ||
            characterSelectPanelUI == null ||
            characterSelectView == null ||
            gameManager == null ||
            titleScreenRoot == null ||
            gameplayHud == null ||
            pauseMenuPanel == null ||
            pausePanelUI == null ||
            gameplayViewController == null)
        {
            Debug.LogError("TitleScreenController：请绑定全部引用。", this);
            return;
        }

        gameManager.Stop();
        gameplayViewController.SetGameplayLayout(false);
        gameplayHud.SetActive(false);
        SetPanelVisible(titleScreenRoot, true);

        SetPanelVisible(pauseMenuPanel, false);
        SetPanelVisible(settingsPanel, false);
        SetPanelVisible(characterSelectPanel, false);
        SetPanelVisible(mainMenuPanel, true);

        currentState = TitlePanelState.MainMenu;
        isInitialized = true;

        characterSelectView.CharacterSelected += BeginGame;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBgm(true);
        }

        if (openCharacterSelectOnStart)
        {
            openCharacterSelectOnStart = false;

            isTransitioning = true;
            StartCoroutine(OpenCharacterSelectRoutine(true));
        }
    }

    // 场景销毁时，移除角色选择事件监听。
    private void OnDestroy()
    {
        if (characterSelectView != null)
        {
            characterSelectView.CharacterSelected -= BeginGame;
        }
    }

    // 立即暂停游戏，再播放菜单打开动画。
    public void OpenPauseMenu()
    {
        if (!isInitialized ||
            isTransitioning ||
            currentState != TitlePanelState.Closed ||
            !gameManager.isLive)
        {
            return;
        }

        gameManager.Stop();

        if (gameManager.player != null)
        {
            gameManager.player.inputVec = Vector2.zero;
        }

        currentState = TitlePanelState.PauseMenu;
        isTransitioning = true;

        StartCoroutine(OpenPauseMenuRoutine());
    }

    private IEnumerator OpenPauseMenuRoutine()
    {
        try
        {
            SetPanelVisible(mainMenuPanel, false);
            SetPanelVisible(settingsPanel, false);
            SetPanelVisible(characterSelectPanel, false);

            SetPanelVisible(titleScreenRoot, true);

            // 等待暂停菜单淡入完成。
            yield return pausePanelUI.Show();
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // 先播放关闭动画，再恢复当前游戏。
    public void ResumeGame()
    {
        if (!isInitialized ||
            isTransitioning ||
            currentState != TitlePanelState.PauseMenu)
        {
            return;
        }

        isTransitioning = true;

        StartCoroutine(ResumeGameRoutine());
    }

    private IEnumerator ResumeGameRoutine()
    {
        try
        {
            // 淡出期间游戏继续保持暂停。
            yield return pausePanelUI.Hide();

            SetPanelVisible(titleScreenRoot, false);

            currentState = TitlePanelState.Closed;

            // 必须放在关闭动画之后。
            gameManager.Resume();
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // 关闭暂停菜单后，结束当前局并返回角色选择。
    public void ReturnToCharacterSelect()
    {
        if (!isInitialized ||
            isTransitioning ||
            currentState != TitlePanelState.PauseMenu)
        {
            return;
        }

        Scene currentScene = SceneManager.GetActiveScene();

        if (currentScene.buildIndex < 0)
        {
            Debug.LogError(
                "TitleScreenController：当前场景没有加入构建列表。",
                this);
            return;
        }

        isTransitioning = true;

        StartCoroutine(
            ReturnToCharacterSelectRoutine(currentScene.buildIndex));
    }

    private IEnumerator ReturnToCharacterSelectRoutine(int sceneIndex)
    {
        try
        {
            settingsView.Save();

            // 淡出完成之前，当前游戏保持暂停。
            yield return pausePanelUI.Hide();

            // 新场景启动后，直接打开角色选择。
            openCharacterSelectOnStart = true;

            currentState = TitlePanelState.Closed;

            // 新场景初始化时会重新暂停。
            Time.timeScale = 1f;

            SceneManager.LoadScene(sceneIndex);
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // 从主菜单进入角色选择。
    public void OpenCharacterSelect()
    {
        if (!isInitialized ||
            isTransitioning ||
            currentState != TitlePanelState.MainMenu)
        {
            return;
        }

        isTransitioning = true;

        StartCoroutine(OpenCharacterSelectRoutine(false));
    }

    private IEnumerator OpenCharacterSelectRoutine(bool skipMainMenuAnimation)
    {
        try
        {
            characterSelectView.Refresh();

            if (skipMainMenuAnimation)
            {
                // 场景重载后直接进入选人，不展示主菜单动画。
                mainMenuPanelUI.HideImmediate();
            }
            else
            {
                yield return mainMenuPanelUI.Hide();
            }

            currentState = TitlePanelState.CharacterSelect;

            yield return characterSelectPanelUI.Show();
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // 从主菜单或暂停菜单进入设置。
    public void OpenSettings()
    {
        if (!isInitialized || isTransitioning)
        {
            return;
        }

        if (currentState != TitlePanelState.MainMenu &&
            currentState != TitlePanelState.PauseMenu)
        {
            return;
        }

        settingsReturnState = currentState;

        isTransitioning = true;
        StartCoroutine(OpenSettingsRoutine());
    }

    private IEnumerator OpenSettingsRoutine()
    {
        try
        {
            settingsView.RefreshFromAudio();

            if (settingsReturnState == TitlePanelState.PauseMenu)
            {
                yield return pausePanelUI.Hide();
            }
            else
            {
                yield return mainMenuPanelUI.Hide();
            }

            SetPanelVisible(characterSelectPanel, false);

            currentState = TitlePanelState.Settings;

            yield return settingsPanelUI.Show();
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // 从设置页或角色选择页返回。
    public void Back()
    {
        if (!isInitialized || isTransitioning)
        {
            return;
        }

        if (currentState == TitlePanelState.Settings)
        {
            isTransitioning = true;
            StartCoroutine(CloseSettingsRoutine());
            return;
        }

        if (currentState == TitlePanelState.CharacterSelect)
        {
            isTransitioning = true;
            StartCoroutine(ReturnToMainMenuRoutine());
        }
    }

    private IEnumerator ReturnToMainMenuRoutine()
    {
        try
        {
            yield return characterSelectPanelUI.Hide();

            currentState = TitlePanelState.MainMenu;

            yield return mainMenuPanelUI.Show();
        }
        finally
        {
            isTransitioning = false;
        }
    }

    private IEnumerator CloseSettingsRoutine()
    {
        try
        {
            settingsView.Save();

            yield return settingsPanelUI.Hide();

            if (settingsReturnState == TitlePanelState.PauseMenu)
            {
                yield return pausePanelUI.Show();

                currentState = TitlePanelState.PauseMenu;
            }
            else
            {
                yield return mainMenuPanelUI.Show();

                currentState = TitlePanelState.MainMenu;
            }
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // 角色选择通过检查后，播放关闭动画再开局。
    private void BeginGame(int characterId)
    {
        if (!isInitialized ||
            isTransitioning ||
            isStartingGame ||
            currentState != TitlePanelState.CharacterSelect)
        {
            return;
        }

        if (characterId < 0 ||
            characterId > 3 ||
            gameManager.player == null ||
            gameManager.player.animCon == null ||
            characterId >= gameManager.player.animCon.Length ||
            gameManager.player.animCon[characterId] == null)
        {
            Debug.LogError(
                "TitleScreenController：角色 ID 或玩家动画配置无效。",
                this);
            return;
        }

        isStartingGame = true;
        isTransitioning = true;

        StartCoroutine(BeginGameRoutine(characterId));
    }

    private IEnumerator BeginGameRoutine(int characterId)
    {
        try
        {
            settingsView.Save();

            // 选人页关闭期间，游戏尚未开始。
            yield return characterSelectPanelUI.Hide();

            SetPanelVisible(mainMenuPanel, false);
            SetPanelVisible(settingsPanel, false);
            SetPanelVisible(pauseMenuPanel, false);
            SetPanelVisible(titleScreenRoot, false);

            // 切换为带顶部信息栏的游戏布局。
            gameplayViewController.SetGameplayLayout(true);
            gameplayHud.SetActive(true);

            currentState = TitlePanelState.Closed;

            // 动画结束后才激活玩家、发放武器并恢复游戏。
            gameManager.GameStart(characterId);
        }
        finally
        {
            isTransitioning = false;
        }
    }

    // 四个业务面板统一交给 UIPanel 控制立即显隐。
    private void SetPanelVisible(CanvasGroup panel, bool visible)
    {
        UIPanel panelUI = null;

        if (panel == mainMenuPanel)
        {
            panelUI = mainMenuPanelUI;
        }
        else if (panel == characterSelectPanel)
        {
            panelUI = characterSelectPanelUI;
        }
        else if (panel == settingsPanel)
        {
            panelUI = settingsPanelUI;
        }
        else if (panel == pauseMenuPanel)
        {
            panelUI = pausePanelUI;
        }

        if (panelUI != null)
        {
            if (visible)
            {
                panelUI.ShowImmediate();
            }
            else
            {
                panelUI.HideImmediate();
            }

            return;
        }

        // 菜单总容器继续直接控制 CanvasGroup。
        panel.alpha = visible ? 1f : 0f;
        panel.interactable = visible;
        panel.blocksRaycasts = visible;
    }
}