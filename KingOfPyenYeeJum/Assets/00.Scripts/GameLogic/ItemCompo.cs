using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCompo : MonoBehaviour , IPointerClickHandler
{
    [SerializeField]
    protected ItemSO _itemType;

    protected Image _visual;

    private void Awake()
    {
        _visual = GetComponentInChildren<Image>();
    }

    public void InitItem(ItemSO item)
    {
        _itemType = item;
        if (_itemType == null)
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
        if(!!item)
        _visual.color = color;
    }

    public ItemSO GetCurrentItem()
    {
        return _itemType;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.GetCompo<GameCursor>().SelectItem(this);
    }
}
