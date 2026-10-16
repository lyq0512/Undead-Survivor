using System;
using UnityEngine;

public class CharacterSelectPanel : MonoBehaviour
{
    [Header("成就管理器")]
    [SerializeField] private AchieveManager achieveManager;

    // 角色通过检查后，通知菜单控制器。
    public event Action<int> CharacterSelected;

    // 打开角色选择界面时调用。
    public void Refresh()
    {
        if (achieveManager == null)
        {
            Debug.LogError("CharacterSelectPanel：请绑定成就管理器。", this);
            return;
        }

        achieveManager.RefreshCharacterViews();
    }

    // 点击角色按钮时调用。
    public void SelectCharacter(int characterId)
    {
        if (achieveManager == null)
        {
            Debug.LogError("CharacterSelectPanel：请绑定成就管理器。", this);
            return;
        }

        if (!achieveManager.IsCharacterUnlocked(characterId))
        {
            return;
        }

        CharacterSelected?.Invoke(characterId);
    }
}