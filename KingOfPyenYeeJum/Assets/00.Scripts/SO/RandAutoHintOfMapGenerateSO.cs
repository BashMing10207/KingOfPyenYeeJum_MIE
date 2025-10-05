using UnityEngine;
[CreateAssetMenu(fileName="MapHintSO",menuName = "SO/MapHintSO")]
public class RandAutoHintOfMapGenerateSO : ScriptableObject
{

    public ItemSO[] ExistItems;

    public float Difficulty = 0;

    public int LayerCnt = 3;

    public int SlotCnt = 12;

    public int itemcnt = 18;

}
