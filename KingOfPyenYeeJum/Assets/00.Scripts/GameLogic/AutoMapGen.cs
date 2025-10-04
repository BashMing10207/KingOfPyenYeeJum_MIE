using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoMapGen : MonoBehaviour
{
    [SerializeField]
    private MapGenSourceSO _mapGenSource;

    [SerializeField]
    private SlotCOmpo[] _slotTypes; // 0: 1slot / 1:2slot / 2:3slot  * * *

    [SerializeField]
    private Transform _slotParent;

    [SerializeField]
    private RandAutoHintOfMapGenerateSO _mapGenHint;

    [ContextMenu("RunGenrerateHEHEHA")]
    private void RunMapGen()
    { 
        if(_mapGenSource == null) return;

        List<SlotCOmpo> slots = new();

        for (int i = 0; i < _mapGenSource.map.Count; i++)
        {
            Debug.Log(_mapGenSource.map[i].List[0].items.Length);
            slots.Add(Instantiate(_slotTypes[_mapGenSource.map[i].List[0].items.Length], _slotParent));

            slots[i].SetMap(_mapGenSource.map[i]);
        }

    }

    [ContextMenu("GetMapData")]
    private void RunGetMapData()
    {
        List<SlotCOmpo> slots = _slotParent.GetComponentsInChildren<SlotCOmpo>().ToList();

        _mapGenSource.map.Clear();

        for (int i = 0; i < slots.Count; i++)
        {
            _mapGenSource.map.Add(new(slots[i].GetItemSOArr()));
        }
    }

    [ContextMenu("GetRandomAutoMapData")]
    private void GenerateRandomMapData()
    {
        _mapGenSource.map.Clear();

        _mapGenHint.itemcnt = _mapGenHint.itemcnt - (_mapGenHint.itemcnt % 3);

        if (_mapGenHint.LayerCnt ==1)
        {
            int tmp = 0; // <- itemSlotcount!
            for(int i =0; i<_mapGenSource.map.Count; i++)
            {
                for(int j =0; j<_mapGenSource.map[i].List.Count; j++)
                {
                    for(int k =0; k < _mapGenSource.map[i].List[j].items.Length; k++)
                    {
                        tmp++;
                    }
                }
            }

            if(_mapGenHint.itemcnt < tmp)
            {
                Debug.LogWarning("NoWay;; MingMingTT");

                return;
            }

            List<ItemSO> randomitemlist = new List<ItemSO>();

            int itemcnttmp = _mapGenHint.itemcnt;
            while(itemcnttmp > 0)
            {
                ItemSO inertItemming = _mapGenHint.ExistItems[Random.Range(0, _mapGenHint.ExistItems.Length-1)];
                for(int i =0; i<3;i++)
                {
                    randomitemlist.Add(inertItemming);
                    itemcnttmp--;
                }
                
            }

            while(randomitemlist.Count > 0)
            {
                for (int i = 0; i < _mapGenSource.map.Count; i++)
                {
                    for (int j = 0; j < _mapGenSource.map[i].List.Count; j++)
                    {
                        for (int k = 0; k < _mapGenSource.map[i].List[j].items.Length; k++)
                        {
                            if (Random.Range(0, 9) < 2)
                            {

                                if(randomitemlist.Count <= 0)
                                {
                                    return;
                                }

                                _mapGenSource.map[i].List[j].items[k]=randomitemlist[0];

                                randomitemlist.RemoveAt(0);

                            }
                        }
                    }
                }
            }

            

        }
        else
        {



        }
    }
}
