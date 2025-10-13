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

    protected bool _bIsFrontSlot = true;

    private void Awake()
    {
        _visual = GetComponentInChildren<Image>();
    }

    public void BeforeInit(SlotCOmpo slotcompo, int idx,bool frontSlot)
    {
        MomSlot = slotcompo;
        _myIdx = idx;
        _bIsFrontSlot = frontSlot;

    }

    public void InitItem(ItemSO item)
    {


        if (item == null)
        {
            _visual.color = Color.clear;
            //_visual.raycastTarget = false;
            if(_bIsFrontSlot && _itemType != item)
            {
                GameManager.Instance.GetCompo<GameRuleCheck>().AddEmptySlots(1);
                GameManager.Instance.GetCompo<GameRuleCheck>().AddFilledSlots(-1);
            }
        }
        else
        {
            _visual.color = Color.white;
            _visual.sprite = item.visual;
            if (_bIsFrontSlot)
            {
                GameManager.Instance.GetCompo<GameRuleCheck>().AddEmptySlots(-1);
                GameManager.Instance.GetCompo<GameRuleCheck>().AddFilledSlots(1);
            }
            //_visual.raycastTarget = true;
        }
        if(_itemType != item)
        GameManager.Instance.GetCompo<GameTurnCheck>().OnSwap?.Invoke();
        _itemType = item;
        _visual.raycastTarget = true;

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

        //MomSlot.CheckMatchAndSwap();
    }

    //public void OnPointerUp(PointerEventData eventData)
    //{
    //    throw new System.NotImplementedException();
    //}
}
