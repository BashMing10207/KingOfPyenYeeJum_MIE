
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapGenSource", menuName = "SO/MapGenSource")]
public class MapGenSourceSO : ScriptableObject
{
    public List<BashList<InsideArray<ItemSO>>> map;
}
