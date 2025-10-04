using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemCompo : MonoBehaviour, IPointerDownHandler//, IPointerUpHandler
{
    [SerializeField]
    protected ItemSO _itemType;

    protected Image _visual;

    public SlotCOmpo MomSlot;

    protected int _myIdx=0;

    private void Awake()
    {
        _visual = GetComponentInChildren<Image>();
    }

    public void InitV2(SlotCOmpo slotcompo, int idx)
    {
        MomSlot = slotcompo;
        _myIdx = idx;
    }

    public void InitItem(ItemSO item)
    {
        _itemType = item;
        _visual.raycastTarget = true;
        if (_itemType == null)
        {
            _visual.color = Color.clear;
            //_visual.raycastTarget = false;
        }
        else
        {
            _visual.color = Color.white;
            _visual.sprite = item.visual;
            //_visual.raycastTarget = true;
        }
    }
    public void InitItem(ItemSO item,Color color)
    {
        InitItem(item);
        if(!!item)
        _visual.color = color;
        _visual.raycastTarget = false;
    }

    public ItemSO GetCurrentItem()
    {
        return _itemType;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.GetCompo<GameCursor>().SelectItem(this);
    }

    public bool InsertItem(ItemSO item)
    {
        if (_itemType != null)
        return false;

        InitItem(item);
        MomSlot.GetItemSOArr()[0].items[_myIdx] = item;

        MomSlot.CheckMatchAndSwap();
        return true;
    }

    public void SetItemWithOutCheck(ItemSO item)
    {
        MomSlot.GetItemSOArr()[0].items[_myIdx] = item;

        InitItem(null);

    }

    public void RemoveItem()
    {
        MomSlot.GetItemSOArr()[0].items[_myIdx] = null;

        InitItem(null);

        MomSlot.CheckMatchAndSwap();
    }

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    throw new System.NotImplementedException();
    //}
}
