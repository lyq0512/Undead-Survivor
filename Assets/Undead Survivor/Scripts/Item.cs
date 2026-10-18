using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData data;
    public int level;
    public Weapon weapon;
    public Gear gear;

    private Image icon;
    private Text textLevel;
    private Text textName;
    private Text textDesc;

    private void Awake()
    {
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];
        textName = texts[1];
        textDesc = texts[2];
        textName.text = data.itemName;
    }

    private void OnEnable()
    {
        // 1. 消耗品不显示等级文字，装备显示等级
        if (data.itemType == ItemData.ItemType.Heal)
        {
            textLevel.text = "";
        }
        else
        {
            textLevel.text = "Lv." + (level + 1);
        }

        // 2. 描述格式化与防越界保护
        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (level == 0)
                {
                    textDesc.text = string.Format(data.itemDesc, data.baseDamage * 100, data.baseCount);
                }
                else
                {
                    int index = Mathf.Min(level, data.damages.Length - 1);
                    textDesc.text = string.Format(data.itemDesc, data.damages[index] * 100, data.counts[index]);
                }
                break;

            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    textDesc.text = string.Format(data.itemDesc, data.baseDamage * 100);
                }
                else
                {
                    int index = Mathf.Min(level, data.damages.Length - 1);
                    textDesc.text = string.Format(data.itemDesc, data.damages[index] * 100);
                }
                break;

            default:
                textDesc.text = string.Format(data.itemDesc);
                break;
        }
    }

    public void OnClick()
    {
        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (level == 0)
                {
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                else
                {
                    float nextDamage = data.baseDamage;
                    int nextCount = 0;

                    nextDamage += data.baseDamage * data.damages[level];
                    nextCount += data.counts[level];

                    weapon.LevelUp(nextDamage, nextCount);
                }
                level++;
                break;

            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else
                {
                    float nextRate = data.damages[level];
                    gear.LevelUp(nextRate);
                }
                level++;
                break;

            case ItemData.ItemType.Heal:
                GameManager.instance.health = GameManager.instance.maxHealth;
                GameManager.instance.NotifyHealthChanged();
                // 消耗品不增加 level，也不会禁用按钮，直接退出
                return;
        }

        // 3. 只有可升级且配置了数值的装备才在满级时禁用按钮
        if (data.damages != null && data.damages.Length > 0 && level == data.damages.Length)
        {
            GetComponent<Button>().interactable = false;
        }
    }
}