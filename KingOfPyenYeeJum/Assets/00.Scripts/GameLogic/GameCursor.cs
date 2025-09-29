using UnityEngine;
using UnityEngine.UI;

public class GameCursor : GetCompoableBase
{
    [SerializeField]
    private Image _visual;

    private ItemCompo _selectedItemCompo;

    private void Start()
    {
        if(!!_visual)
        {
            _visual = GetComponentInChildren<Image>();
        }
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
        if(!!_selectedItemCompo)
        {
            _visual.color = Color.white;
            _visual.sprite = _selectedItemCompo.GetCurrentItem().visual;
        }
        else
        {
            _visual.color = Color.clear;
        }
    }

    public void DropItem()
    {
        
    }
}
