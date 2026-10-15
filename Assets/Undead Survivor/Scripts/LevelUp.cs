using UnityEngine;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
    }

    public void Show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
        AudioManager.instance.PlayerSfx(AudioManager.Sfx.LevelUp);
        AudioManager.instance.EffectBgm(true);
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        AudioManager.instance.PlayerSfx(AudioManager.Sfx.Select);
        AudioManager.instance.EffectBgm(false);
    }

    public void Select(int index)
    {
        items[index].OnClick();
        Hide();
    }

    private void Next()
    {
        // 1. 所有物品被禁用
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 2. 从这些物品中随机选中3个并激活
        int[] ran = new int[3];
        while (true)
        {
            ran[0] = Random.Range(0, items.Length);
            ran[1] = Random.Range(0, items.Length);
            ran[2] = Random.Range(0, items.Length);

            if (ran[0] != ran[1] && ran[1] != ran[2] && ran[0] != ran[2])
            {
                break;
            }
        }

        for (int index = 0; index < ran.Length; index++)
        {
            Item ranItem = items[ran[index]];

            // 3. 将满级物品替换为消耗品
            if (ranItem.level == ranItem.data.damages.Length)
            {
                //1. items[Random.Range(4, 7)].gameObject.SetActive(true);

                //2.
                //int maxIndex = items.Length;
                //int minIndex = Mathf.Min(4, maxIndex - 1);
                //items[Random.Range(minIndex, maxIndex)].gameObject.SetActive(true);

                items[4].gameObject.SetActive(true);
            }
            else
            {
                ranItem.gameObject.SetActive(true);
            }
        }
    }
}
