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

public int EmptyCnt(T obj)
    {
        int cnt = 0;
        for(int i = 0;i<items.Length;i++)
        {
            if(items[i] == obj)
            cnt++;
        }
        return cnt;
    }
}

public class SlotCOmpo : MonoBehaviour
{
    [SerializeField]
    private BashList<InsideArray<ItemSO>> _itemsSOs;

    [SerializeField]
    private BashList<InsideArray<ItemCompo>> _itemCompos;

    [SerializeField]
    private Transform[] _itemParents;

    [SerializeField]
    private ItemCompo _itemCompoPefab;
    public
    List<BashTuple<ItemSO, int>> cnt = new();
    private void Start()
    {
        for (int i = 0; i < _itemParents.Length; i++)
        {
            //InsideArray<ItemCompo> itemCompo;
            //itemCompo.items = _itemParents[i].GetComponentsInChildren<ItemCompo>();
            //_itemCompos.List.Add(itemCompo);

            InsideArray<ItemCompo> itemCompo;

            {
                itemCompo.items = _itemParents[i].GetComponentsInChildren<ItemCompo>();
                _itemCompos.List.Add(itemCompo);
            }
            if (_itemCompos.List[i].items.Length ==0)
            {
                for(int j =0;  j < _itemsSOs.List[i].items.Length; j++)
                {
                    ItemCompo itemcompoinst = Instantiate(_itemCompoPefab);
                    itemcompoinst.transform.parent = _itemParents[i];
                }

                itemCompo.items = _itemParents[i].GetComponentsInChildren<ItemCompo>();
                _itemCompos.List.Add(itemCompo);
            }


        }

        PreInitItemCompos();

        InitItemCompo();

    }

    public void SetMap(BashList<InsideArray<ItemSO>> map)
    {
        _itemsSOs = map;
    }

    public bool InsertItem(ItemSO newitem)
    {
        if (_itemsSOs.List[0].items.Contains(null))
            for (int i = 0; i < _itemsSOs.List.Count; i++)
            {
                if(_itemsSOs.List[0].items[i] == null)
                {
                    _itemsSOs.List[0].items[i] = newitem;
                    _itemCompos.List[0].items[i].InitItem(newitem);
                    CheckMatchAndSwap();
                    return true;
                }
            }

        return false;
    }

    public bool RemoveItemWithOutMatchCheck(ItemSO newitem)
    {

        if (_itemsSOs.List[0].items.Contains(newitem))
            for (int i = 0; i < _itemsSOs.List.Count; i++)
            {
                if (_itemsSOs.List[0].items[i] == newitem)
                {
                    _itemsSOs.List[0].items[i] = null;
                    _itemCompos.List[0].items[i].InitItem(null);
                    CheckSwap();
                    return true;
                }
            }

        return false;
    }
    public bool RemoveItem(ItemSO newitem)
    {
        Debug.LogWarning("Ming");
        if (_itemsSOs.List[0].items.Contains(newitem))
            for (int i = 0; i < _itemsSOs.List.Count; i++)
            {
                if (_itemsSOs.List[0].items[i] == newitem)
                {
                    _itemsSOs.List[0].items[i] = null;
                    _itemCompos.List[0].items[i].InitItem(null);
                    CheckMatchAndSwap();
                    return true;
                }
            }

        return false;
    }

    public void SwapNextLayer()
    {
        if (_itemsSOs.List.Count <2)
        {
            //for (int i = 0; i < _itemCompos.List[0].items.Length; i++)
            //{
            //    _itemCompos.List[0].items[i].RemoveItem();
            //}
            return;
        }
            
        _itemsSOs.List.RemoveAt(0);

        InitItemCompo();
    }

    public void InitItemCompo()
    {
        for (int i = 0; i < _itemCompos.List.Count; i++)
        {
            if(_itemsSOs.List.Count > 1)
            {
                for (int j = 0; j < _itemsSOs.List[i].items.Length; j++)
                {
                    if (i == 0)
                        _itemCompos.List[i].items[j].InitItem(_itemsSOs.List[i].items[j]);
                    else
                        _itemCompos.List[i].items[j].InitItem(_itemsSOs.List[i].items[j], Color.black);
                }
            }
            else
            {
                for (int j = 0; j < _itemsSOs.List[0].items.Length; j++)
                {
                    if (i == 0)
                        _itemCompos.List[0].items[j].InitItem(_itemsSOs.List[0].items[j]);
                    else
                        _itemCompos.List[1].items[j].InitItem(null, Color.clear);
                }
            }

        }
    }
    public void PreInitItemCompos()
    {
        for (int i = 0; i < _itemCompos.List.Count; i++)
        {
            for (int j = 0; j < _itemsSOs.List[i].items.Length; j++)
            {
                _itemCompos.List[i].items[j].InitV2(this,j);
            }
        }
    }

    public void CheckSwap()
    {

        bool bIsEmpty = true;
        for (int i = 0; i < _itemsSOs.List[0].items.Length; i++)
        {
            if (_itemsSOs.List[0].items[i] != null)
            {
                bIsEmpty = false; break;
            }
        }

        if (bIsEmpty)
        {
            SwapNextLayer();
        }
    }

    public void CheckMatch()
    {
        cnt.Clear();

        for (int i = 0; i < _itemsSOs.List[0].items.Length; i++)
        {
            bool isExistvalue = false;
            int idx = 0;
            ItemSO currentCheckItem = _itemsSOs.List[0].items[i];

            if (currentCheckItem == null)
                continue;

            for (int j = 0; j < cnt.Count; j++)
            {
                if (cnt[j].One == currentCheckItem)
                {
                    idx = j;
                    isExistvalue = true;

                    cnt[idx].SetTwo(cnt[idx].Two + 1);
                    if (cnt[idx].Two >= 3)
                    {
                        cnt[idx].SetTwo(0);
                        
                        for(int k =0; k < _itemsSOs.List[0].items.Length; k++)
                        {
                            if(_itemsSOs.List[0].items[k]==currentCheckItem)
                            {
                                _itemCompos.List[0].items[k].SetItemWithOutCheck (null);

                            }
                        }
                        CheckSwap();
                    }
                    break;
                }
            }
            if (isExistvalue)
            {

                //cnt[idx].SetTwo(cnt[idx].Two + 1);
                //if (cnt[idx].Two >= 3)
                //{
                //    cnt[idx].SetTwo(0);
                //    for (int k = 0; k < 3; k++)
                //    {
                //        RemoveItem(currentCheckItem);
                //    }
                //}
            }
            else
            {
                cnt.Add(new(currentCheckItem, 1));
            }
        }
    }

    public void CheckMatchAndSwap()
    {
        CheckMatch();
        CheckSwap();

    }

    public List<InsideArray<ItemSO>> GetItemSOArr()
    {
        return _itemsSOs.List;
    }
}
