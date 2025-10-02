using System.Collections.Generic;
using UnityEngine;

public class AutoMapGen : MonoBehaviour
{
    [SerializeField]
    private MapGenSourceSO _mapGenSource;

    [SerializeField]
    private SlotCompo[] _slotTypes; // 0: 1slot / 1:2slot / 2:3slot  * * *

    [SerializeField]
    private Transform _slotParent;

    [ContextMenu("RunGenrerateHEHEHA")]
    private void RunMapGen()
    { 
        if(_mapGenSource == null) return;

        List<SlotCompo> slots = new();

        for (int i = 0; i < _mapGenSource.map.Count; i++)
        {
            Debug.Log(_mapGenSource.map[i].List[0].items.Length);
            slots.Add(Instantiate(_slotTypes[_mapGenSource.map[i].List[0].items.Length], _slotParent));

            slots[i].SetMap(_mapGenSource.map[i]);
        }



    }

}
