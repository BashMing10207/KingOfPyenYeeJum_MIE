using UnityEngine;
using UnityEngine.UI;

public class ItemCompo : MonoBehaviour
{
    [SerializeField]
    protected ItemSO itemType;

    protected Image _visual;

    public void InitItem(ItemSO item)
    {
        itemType = item;
        if (itemType == null)
        {
            _visual.color = Color.clear;
        }
        else
        {
            _visual.color = Color.white;
            _visual.sprite = item.visual;
        }
    }
    public void InitItem(ItemSO item,Color color)
    {
        InitItem(item);
        _visual.color = color;
    }

    public ItemSO GetCurrentItem()
    {
        return itemType;
    }
}
