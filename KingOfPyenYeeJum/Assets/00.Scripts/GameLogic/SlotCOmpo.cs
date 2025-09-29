using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct InsideArray<T> where T : class
{
    public T[] items;

    public InsideArray(int idxsize)
        {
            items = new T[idxsize];
        }
}

public class SlotCOmpo : MonoBehaviour
{
    [SerializeField]
    private List<InsideArray<ItemSO>> _itemsSOs;

    [SerializeField]
    private List<InsideArray<ItemCompo>> _itemCompos;

    [SerializeField]
    private Transform[] _itemParents;

    [SerializeField]
    private ItemCompo _itemCompoPefab;

    private void Start()
    {
        for (int i = 0; i < _itemParents.Length; i++)
        {
            //InsideArray<ItemCompo> itemCompo;
            //itemCompo.items = _itemParents[i].GetComponentsInChildren<ItemCompo>();
            //_itemCompos.Add(itemCompo);

            InsideArray<ItemCompo> itemCompo;

            {
                itemCompo.items = _itemParents[i].GetComponentsInChildren<ItemCompo>();
                _itemCompos.Add(itemCompo);
            }
            if (_itemCompos[i].items.Length ==0)
            {
                for(int j =0;  j < _itemsSOs[i].items.Length; j++)
                {
                    ItemCompo itemcompoinst = Instantiate(_itemCompoPefab);
                    itemcompoinst.transform.parent = _itemParents[i];
                }

                itemCompo.items = _itemParents[i].GetComponentsInChildren<ItemCompo>();
                _itemCompos.Add(itemCompo);
            }


        }

        InitItemCompo();

    }

    public bool InsertItem(ItemSO newitem)
    {
        if (_itemsSOs[0].items.Contains(null))
            for (int i = 0; i < _itemsSOs.Count; i++)
            {
                if(_itemsSOs[0].items[i] == null)
                {
                    _itemsSOs[0].items[i] = newitem;
                    _itemCompos[0].items[i].InitItem(newitem);

                    return true;
                }
            }

        return false;
    }

    public bool RemoveItem(ItemSO newitem)
    {
        if (_itemsSOs[0].items.Contains(newitem))
            for (int i = 0; i < _itemsSOs.Count; i++)
            {
                if (_itemsSOs[0].items[i] == newitem)
                {
                    _itemCompos[0].items[i].InitItem(null);

                    return true;
                }
            }

        return false;
    }

    public void SwapNextLayer()
    {
        if (_itemsSOs.Count == 0)
            return;
            
        _itemsSOs.RemoveAt(0);

        InitItemCompo();
    }

    public void InitItemCompo()
    {
        for (int i = 0; i < _itemCompos.Count; i++)
        {
            for (int j = 0; j < _itemsSOs[i].items.Length; j++)
            {
                if (i == 0)
                    _itemCompos[i].items[j].InitItem(_itemsSOs[i].items[j]);
                else
                    _itemCompos[i].items[j].InitItem(_itemsSOs[i].items[j], Color.black);
            }
        }
    }
}
