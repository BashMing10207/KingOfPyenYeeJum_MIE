using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameCursor : GetCompoableBase
{
    [SerializeField]
    private Image _visual;

    private ItemCompo _selectedItemCompo;

    private InputManagerCompo _inputManagerCompo;

    private List<RaycastResult> _hitList = new();

    private void Start()
    {
        if(!!_visual)
        {
            _visual = GetComponentInChildren<Image>();
        }

        _inputManagerCompo = Mom.GetCompo<InputManagerCompo>();
        _inputManagerCompo.OnTouchup.AddListener(DropItem);
    }

    public void SelectItem(ItemCompo itemcompo)
    {
        _selectedItemCompo = itemcompo;
        InitVisual();
    }

    private void LateUpdate()
    {
        _visual.rectTransform.position = Input.mousePosition;// * new Vector2(960,540);
    }

    public void InitVisual()
    {
        _visual.color = Color.clear;

        if (!!_selectedItemCompo)
        {
            if (_selectedItemCompo.GetCurrentItem() != null)
            {
                if ( _selectedItemCompo.GetCurrentItem().visual !=null )
                {
                    _visual.color = Color.white;
                    _visual.sprite = _selectedItemCompo.GetCurrentItem().visual;
                }

            }

        }


      
    }

    public void DropItem()
    {
        if (_selectedItemCompo == null)
            return;


        Vector2 pos = Input.mousePosition;
        var ped = new PointerEventData(EventSystem.current) { position = pos };

        _hitList.Clear();

        EventSystem.current.RaycastAll(ped, _hitList);

        if(_hitList.Count !=0)
        {
            Debug.Log(_hitList[0]);

            ItemCompo slot = _hitList[0].gameObject.GetComponent<ItemCompo>();
            if (slot)
            {
                //if (slot.InsertItem(_selectedItemCompo.GetCurrentItem()))
                if (slot.GetCurrentItem() == null)
                {
                    ItemSO itemtemp = _selectedItemCompo.GetCurrentItem();
                    _selectedItemCompo.RemoveItem();
                    slot.InsertItem(itemtemp);
                    //_selectedItemCompo.MomSlot.CheckMatchAndSwap();
                }

            }
        }


        
        
        

        _selectedItemCompo = null;
        InitVisual();
    }
}
