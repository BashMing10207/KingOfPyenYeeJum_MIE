using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoMapGen : MonoBehaviour
{
    [SerializeField]
    private MapGenSourceSO _mapGenSource;

    [SerializeField]
    private SlotCOmpo[] _slotTypes; // 0: 1slot / 1:2slot / 2:3slot

    [SerializeField]
    private Transform _slotParent;

    [SerializeField]
    private RandAutoHintOfMapGenerateSO _mapGenHint;

    // ��� �߰�: ��ǥ R(1), R(2) (�����Ϳ��� ����)
    [Header("Residue Targets (R(3)=0�� �ڵ�����)")]
    [SerializeField] private int _targetR1 = 7;
    [SerializeField] private int _targetR2 = 7;

    // ========================= ���� �޼��� (���� X) =========================
    [ContextMenu("RunGenrerateHEHEHA")]
    private void RunMapGen()
    {
        if (_mapGenSource == null) return;

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

        if (_mapGenHint.LayerCnt == 1)
        {
            int tmp = 0; // <- itemSlotcount!
            for (int i = 0; i < _mapGenSource.map.Count; i++)
            {
                for (int j = 0; j < _mapGenSource.map[i].List.Count; j++)
                {
                    for (int k = 0; k < _mapGenSource.map[i].List[j].items.Length; k++)
                    {
                        tmp++;
                    }
                }
            }

            if (_mapGenHint.itemcnt < tmp)
            {
                Debug.LogWarning("NoWay;; MingMingTT");
                return;
            }

            List<ItemSO> randomitemlist = new List<ItemSO>();

            int itemcnttmp = _mapGenHint.itemcnt;
            while (itemcnttmp > 0)
            {
                ItemSO inertItemming = _mapGenHint.ExistItems[Random.Range(0, _mapGenHint.ExistItems.Length - 1)];
                for (int i = 0; i < 3; i++)
                {
                    randomitemlist.Add(inertItemming);
                    itemcnttmp--;
                }
            }

            while (randomitemlist.Count > 0)
            {
                for (int i = 0; i < _mapGenSource.map.Count; i++)
                {
                    for (int j = 0; j < _mapGenSource.map[i].List.Count; j++)
                    {
                        for (int k = 0; k < _mapGenSource.map[i].List[j].items.Length; k++)
                        {
                            if (Random.Range(0, 9) < 2)
                            {
                                if (randomitemlist.Count <= 0)
                                {
                                    return;
                                }

                                _mapGenSource.map[i].List[j].items[k] = randomitemlist[0];
                                randomitemlist.RemoveAt(0);
                            }
                        }
                    }
                }
            }
        }
    }

    // ========================= �ű�: ������ ���� ��Ʈ�� =========================
    [ContextMenu("GenerateByResidueTargets")]
    private void GenerateByResidueTargets()
    {
        if (_mapGenSource == null || _mapGenHint == null || _mapGenHint.ExistItems == null || _mapGenHint.ExistItems.Length == 0)
        {
            Debug.LogError("[GenerateByResidueTargets] Missing MapGenSource or Hint/Items.");
            return;
        }

        // 1) �� ���� ����: ��� ������ 3�� ���̾ ������ ����
        EnsureThreeLayers();

        // 2) �� ����
        ClearAllItems();

        // 3) (r1,r2) ������� targetR1, targetR2 ���߱�
        var blocks = BuildBlocksGreedy(_targetR1, _targetR2); // List<(r1,r2)>

        // �� �߰�: "�߰�(2��)�� ������ ��� �� ��" ����
        EnsureNoEmptyMiddleLayer(ref blocks);

        // 4) �� ����� (d1,d2,d3)�� ��ȯ�ϰ�, ���� ������SO�� ��� ���̾ �ʿ� ���� ����
        var layerNeeds = new List<LayerNeed>(); // per-type needs
        foreach (var block in blocks)
        {
            var (d1, d2) = MapResidueToD(block.r1, block.r2);           // (d1,d2) �� {0,1,2}
            int d3 = Mod3(-d1 - d2);                                    // r3=0 ����
            var item = _mapGenHint.ExistItems[Random.Range(0, _mapGenHint.ExistItems.Length)];
            layerNeeds.Add(new LayerNeed(item, d1, d2, d3));
        }

        // 5) �뷮 üũ: �� ���̾ ä�� �� �ִ� ��ĭ(=null ����) Ȯ��
        int cap1 = CountEmptySlotsInLayer(0);
        int cap2 = CountEmptySlotsInLayer(1);
        int cap3 = CountEmptySlotsInLayer(2);

        int need1 = layerNeeds.Sum(n => n.d1);
        int need2 = layerNeeds.Sum(n => n.d2);
        int need3 = layerNeeds.Sum(n => n.d3);

        if (need1 > cap1 || need2 > cap2 || need3 > cap3)
        {
            Debug.LogError($"[GenerateByResidueTargets] Not enough capacity. Need (L1:{need1}, L2:{need2}, L3:{need3}) / Cap (L1:{cap1}, L2:{cap2}, L3:{cap3})");
            return;
        }

        // 6) ���� ��ġ: ���̾�� null ĭ�� ��ȸ�ϸ� ä�� (�ּ� ��ġ; �е� ���̰� ������ +3k�� �� �ø��� ��)
        foreach (var need in layerNeeds)
        {
            PlaceIntoLayer(0, need.item, need.d1);
            PlaceIntoLayer(1, need.item, need.d2);
            PlaceIntoLayer(2, need.item, need.d3);
        }




        // 7) ����: ���� R(1),R(2),R(3) ���
        var R = ComputeR();
        Debug.Log($"[GenerateByResidueTargets] R(1)={R[0]}, R(2)={R[1]}, R(3)={R[2]}  (Target {_targetR1}, {_targetR2}, 0)");
    }

    // ========================= ���۵�: ���� ����/��ġ/���� =========================

    // Ŭ���� ���ο� �߰�
    private void EnsureNoEmptyMiddleLayer(ref List<Block> blocks)
    {
        // �ִ� �� ������ �õ� (���ѷ��� ����)
        for (int attempt = 0; attempt < 64; attempt++)
        {
            var sums = SumDForBlocks(blocks); // (sum d1, d2, d3)
            int s1 = sums.d1, s2 = sums.d2, s3 = sums.d3;

            // ���� ����: �� ���̾�(d3)�� �������� �ִµ�, �߰� ���̾�(d2)�� 0 �� ����
            if (s3 > 0 && s2 == 0)
            {
                // r1==r2(>0)�� ����� ã�� ���� (���� ������ �� ���̽� �켱)
                int idx = blocks.FindIndex(b => b.r1 == b.r2 && b.r1 > 0);
                if (idx < 0)
                {
                    Debug.LogWarning("[EnsureNoEmptyMiddleLayer] ��ü ������ (r1==r2) ����� ���� ���� ����. Ÿ�� R�� �缳�� �ʿ�.");
                    break;
                }

                var b = blocks[idx];
                blocks.RemoveAt(idx);

                if (b.r1 == 1) // (1,1) �� (1,0) + (0,1)
                {
                    blocks.Add(new Block(1, 0));
                    blocks.Add(new Block(0, 1));
                }
                else if (b.r1 == 2) // (2,2) �� (1,2) + (1,0)  (�ٸ� ���յ� ����: (2,1)+(0,1))
                {
                    blocks.Add(new Block(1, 2));
                    blocks.Add(new Block(1, 0));
                }
                // ���� �ٽ� ���� s2�� ������� ��Ȯ��
                continue;
            }

            // �̹� ���� ���� (�߰� ���̾� ���� ����)
            break;
        }

        // (����) �߰� ����: "���̾�2�� ������ ������ ���̾�1�� �ּ� 1"�� �����ϰ� �ʹٸ�
        // �Ʒ�ó�� s2>0 && s1==0�� ���� ������ ���� ������� ���� �� ����.
        // �ٸ� targetR1==0�� �� �䱸���װ� �浹�� �� �����Ƿ� �⺻�� ����.
    }

    private (int d1, int d2, int d3) SumDForBlocks(List<Block> blocks)
    {
        int s1 = 0, s2 = 0, s3 = 0;
        foreach (var b in blocks)
        {
            var (d1, d2) = MapResidueToD(b.r1, b.r2);
            int d3 = Mod3(-d1 - d2);
            s1 += d1; s2 += d2; s3 += d3;
        }
        return (s1, s2, s3);
    }



    // ���� ��� ���Կ� N�� ���̾ �ֵ��� ���� (�����ϸ� �߰�)
    private void EnsureThreeLayers()
    {
        if (_mapGenSource == null || _mapGenHint == null)
        {
            return;
        }
        _mapGenSource.map.Clear();

        for (int i = 0; i < _mapGenHint.SlotCnt / 3; i++) // 3Item Slot Only!
        {
            var list = new BashList<InsideArray<ItemSO>>();

            for(int j =0; j < 3;j++)
            {
                list.List.Add(new InsideArray<ItemSO>(3));
                for(int k = 0; k < _mapGenHint.LayerCnt;k++)
                {
                    //list.List[j].items[k];
                }
            }

            _mapGenSource.map.Add(list);
        }
        //if (_mapGenSource.map == null || _mapGenSource.map.Count == 0)
        //{
        //    Debug.LogError("[EnsureThreeLayers] _mapGenSource.map �� ����ֽ��ϴ�. ����/���̾ƿ��� ���� �غ��ϼ���.");
        //    return;
        //}

        //for (int i = 0; i < _mapGenSource.map.Count; i++)
        //{
        //    var blist = _mapGenSource.map[i];
        //    if (blist.List == null)
        //        blist.List = new List<InsideArray<ItemSO>>();

        //    if (blist.List.Count == 0)
        //    {
        //        // �ּ� 1�� ���̾�� ������ �⺻ 1ĭ¥�� ����
        //        blist.List.Add(new InsideArray<ItemSO>(1));
        //    }

        //    // ���� ����(���̾�0�� ���� ��)
        //    int baseLen = Mathf.Max(1, blist.List[0].items?.Length ?? 1);

        //    // ���̾� �� 3���� ����
        //    while (blist.List.Count < _mapGenHint.LayerCnt)
        //    {
        //        blist.List.Add(new InsideArray<ItemSO>(baseLen));
        //    }

        //    // ��� ���̾��� items �迭�� null�̸� ���� ����
        //    for (int k = 0; k < 3; k++)
        //    {
        //        if (blist.List[k].items == null || blist.List[k].items.Length == 0)
        //            blist.List[k] = new InsideArray<ItemSO>(baseLen);
        //    }
        //}
    }

    // ��� ������ ����(null��)
    private void ClearAllItems()
    {
        for (int i = 0; i < _mapGenSource.map.Count; i++)
        {
            var blist = _mapGenSource.map[i];
            for (int k = 0; k < Mathf.Min(3, blist.List.Count); k++)
            {
                var arr = blist.List[k].items;
                for (int c = 0; c < arr.Length; c++)
                    arr[c] = null;
            }
        }
    }

    // Ư�� ���̾�� null ĭ ���� ����
    private int CountEmptySlotsInLayer(int layer)
    {
        int cnt = 0;
        for (int i = 0; i < _mapGenSource.map.Count; i++)
        {
            var blist = _mapGenSource.map[i];
            if (blist.List.Count <= layer) continue;
            var arr = blist.List[layer].items;
            for (int c = 0; c < arr.Length; c++)
                if (arr[c] == null) cnt++;
        }
        return cnt;
    }

    // Ư�� ���̾ item�� count�� ��ġ (null ĭ�� ��ȸ�ϸ� ä��)
    private void PlaceIntoLayer(int layer, ItemSO item, int count)
    {
        if (count <= 0) return;
        for (int i = 0; i < _mapGenSource.map.Count && count > 0; i++)
        {
            var blist = _mapGenSource.map[i];
            if (blist.List.Count <= layer) continue;
            var arr = blist.List[layer].items;
            for (int c = 0; c < arr.Length && count > 0; c++)
            {
                if (arr[c] == null)
                {
                    arr[c] = item;
                    count--;
                }
            }
        }
    }

    // ���� ���� R(1),R(2),R(3) ���
    // R(k) = sum_t ( (��_{����k} a_{t,��}) % 3 )
    private int[] ComputeR()
    {
        // ������ ���� ���� �����
        Dictionary<ItemSO, int[]> cumPerType = new Dictionary<ItemSO, int[]>();
        // null�� �� ������ ��� X (��ĭ�� ����)

        for (int k = 0; k < 3; k++)
        {
            for (int i = 0; i < _mapGenSource.map.Count; i++)
            {
                var arr = _mapGenSource.map[i].List[k].items;
                foreach (var it in arr)
                {
                    if (it == null) continue;
                    if (!cumPerType.TryGetValue(it, out var cum))
                    {
                        cum = new int[3]; // ���̾ ����
                        cumPerType[it] = cum;
                    }
                    // k�������� ������ ����ؾ� �ϹǷ�, �ϴ� ���̾ ������ ���� ���� ���߿� prefix�� �ٲ� ���� ������
                    // ���⼱ �ܼ�ȭ�� ���� "k�� �����ϸ� �׶� ���� ���ϱ�" ������� 2-pass�� ���
                }
            }
        }

        // ���� ������ ���̾� ���� countPerLayer[it][k] ����
        Dictionary<ItemSO, int[]> countPerLayer = new Dictionary<ItemSO, int[]>();
        for (int k = 0; k < 3; k++)
        {
            for (int i = 0; i < _mapGenSource.map.Count; i++)
            {
                var arr = _mapGenSource.map[i].List[k].items;
                foreach (var it in arr)
                {
                    if (it == null) continue;
                    if (!countPerLayer.TryGetValue(it, out var vec))
                    {
                        vec = new int[3];
                        countPerLayer[it] = vec;
                    }
                    vec[k]++;
                }
            }
        }

        int[] R = new int[3];
        foreach (var kv in countPerLayer)
        {
            var vec = kv.Value; // (c1, c2, c3)
            int pref1 = vec[0];
            int pref2 = vec[0] + vec[1];
            int pref3 = vec[0] + vec[1] + vec[2];

            R[0] += pref1 % 3;
            R[1] += pref2 % 3;
            R[2] += pref3 % 3;
        }

        return R;
    }

    // ========================= �ܿ��� ���/��ȯ ���� =========================
    private struct Block { public int r1, r2; public Block(int a, int b) { r1 = a; r2 = b; } }
    private struct LayerNeed
    {
        public ItemSO item;
        public int d1, d2, d3;
        public LayerNeed(ItemSO item, int d1, int d2, int d3)
        { this.item = item; this.d1 = d1; this.d2 = d2; this.d3 = d3; }
    }

    // targetR1, targetR2�� (r1,r2) ��ϵ��� ������ ��Ȯ�� ���ߴ� ���� �׸���
    private List<Block> BuildBlocksGreedy(int targetR1, int targetR2)
    {
        List<Block> list = new List<Block>();
        int S1 = 0, S2 = 0;

        while (S1 < targetR1 || S2 < targetR2)
        {
            if (targetR1 - S1 >= 2)
            {
                int want2 = Mathf.Clamp(targetR2 - S2, 0, 2);
                // (2,2) �켱 �� (2,1) �� (2,0)
                if (want2 == 2) { list.Add(new Block(2, 2)); S1 += 2; S2 += 2; }
                else if (want2 == 1) { list.Add(new Block(2, 1)); S1 += 2; S2 += 1; }
                else { list.Add(new Block(2, 0)); S1 += 2; }
            }
            else if (targetR1 - S1 == 1)
            {
                int want2 = Mathf.Clamp(targetR2 - S2, 0, 2);
                // (1,2) �� (1,1) �� (1,0)
                if (want2 == 2) { list.Add(new Block(1, 2)); S1 += 1; S2 += 2; }
                else if (want2 == 1) { list.Add(new Block(1, 1)); S1 += 1; S2 += 1; }
                else { list.Add(new Block(1, 0)); S1 += 1; }
            }
            else // S1 == targetR1
            {
                int want2 = Mathf.Clamp(targetR2 - S2, 0, 2);
                // (0,2) �� (0,1) �� (0,0)
                if (want2 == 2) { list.Add(new Block(0, 2)); S2 += 2; }
                else if (want2 == 1) { list.Add(new Block(0, 1)); S2 += 1; }
                else { list.Add(new Block(0, 0)); /* no change */ }
            }
        }

        return list;
    }

    // (r1,r2) �� (d1,d2) ���� ���̺� (���� ������ ǥ)
    private (int d1, int d2) MapResidueToD(int r1, int r2)
    {
        // �⺻�� ������ġ
        int d1 = 0, d2 = 0;

        if (r1 == 0 && r2 == 0) { d1 = 0; d2 = 0; }
        else if (r1 == 0 && r2 == 1) { d1 = 0; d2 = 1; }
        else if (r1 == 0 && r2 == 2) { d1 = 0; d2 = 2; }
        else if (r1 == 1 && r2 == 0) { d1 = 1; d2 = 2; }
        else if (r1 == 1 && r2 == 1) { d1 = 1; d2 = 0; }
        else if (r1 == 1 && r2 == 2) { d1 = 1; d2 = 1; }
        else if (r1 == 2 && r2 == 0) { d1 = 2; d2 = 1; }
        else if (r1 == 2 && r2 == 1) { d1 = 2; d2 = 2; }
        else if (r1 == 2 && r2 == 2) { d1 = 2; d2 = 0; }

        return (d1, d2);
    }

    private int Mod3(int x)
    {
        int m = x % 3;
        return (m < 0) ? m + 3 : m;
    }
}


#region WithOutAI

//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class AutoMapGen : MonoBehaviour
//{
//    [SerializeField]
//    private MapGenSourceSO _mapGenSource;

//    [SerializeField]
//    private SlotCOmpo[] _slotTypes; // 0: 1slot / 1:2slot / 2:3slot  * * *

//    [SerializeField]
//    private Transform _slotParent;

//    [SerializeField]
//    private RandAutoHintOfMapGenerateSO _mapGenHint;

//    [ContextMenu("RunGenrerateHEHEHA")]
//    private void RunMapGen()
//    { 
//        if(_mapGenSource == null) return;

//        List<SlotCOmpo> slots = new();

//        for (int i = 0; i < _mapGenSource.map.Count; i++)
//        {
//            Debug.Log(_mapGenSource.map[i].List[0].items.Length);
//            slots.Add(Instantiate(_slotTypes[_mapGenSource.map[i].List[0].items.Length], _slotParent));

//            slots[i].SetMap(_mapGenSource.map[i]);
//        }

//    }

//    [ContextMenu("GetMapData")]
//    private void RunGetMapData()
//    {
//        List<SlotCOmpo> slots = _slotParent.GetComponentsInChildren<SlotCOmpo>().ToList();

//        _mapGenSource.map.Clear();

//        for (int i = 0; i < slots.Count; i++)
//        {
//            _mapGenSource.map.Add(new(slots[i].GetItemSOArr()));
//        }
//    }

//    [ContextMenu("GetRandomAutoMapData")]
//    private void GenerateRandomMapData()
//    {
//        _mapGenSource.map.Clear();

//        _mapGenHint.itemcnt = _mapGenHint.itemcnt - (_mapGenHint.itemcnt % 3);

//        if (_mapGenHint.LayerCnt ==1)
//        {
//            int tmp = 0; // <- itemSlotcount!
//            for(int i =0; i<_mapGenSource.map.Count; i++)
//            {
//                for(int j =0; j<_mapGenSource.map[i].List.Count; j++)
//                {
//                    for(int k =0; k < _mapGenSource.map[i].List[j].items.Length; k++)
//                    {
//                        tmp++;
//                    }
//                }
//            }

//            if(_mapGenHint.itemcnt < tmp)
//            {
//                Debug.LogWarning("NoWay;; MingMingTT");

//                return;
//            }

//            List<ItemSO> randomitemlist = new List<ItemSO>();

//            int itemcnttmp = _mapGenHint.itemcnt;
//            while(itemcnttmp > 0)
//            {
//                ItemSO inertItemming = _mapGenHint.ExistItems[Random.Range(0, _mapGenHint.ExistItems.Length-1)];
//                for(int i =0; i<3;i++)
//                {
//                    randomitemlist.Add(inertItemming);
//                    itemcnttmp--;
//                }

//            }

//            while(randomitemlist.Count > 0)
//            {
//                for (int i = 0; i < _mapGenSource.map.Count; i++)
//                {
//                    for (int j = 0; j < _mapGenSource.map[i].List.Count; j++)
//                    {
//                        for (int k = 0; k < _mapGenSource.map[i].List[j].items.Length; k++)
//                        {
//                            if (Random.Range(0, 9) < 2)
//                            {

//                                if(randomitemlist.Count <= 0)
//                                {
//                                    return;
//                                }

//                                _mapGenSource.map[i].List[j].items[k]=randomitemlist[0];

//                                randomitemlist.RemoveAt(0);

//                            }
//                        }
//                    }
//                }
//            }



//        }
//        else if(_mapGenHint.Difficulty < 5)
//        {

//            int tmp = 0; // <- itemSlotcount!
//            for (int i = 0; i < _mapGenSource.map.Count; i++)
//            {
//                for (int j = 0; j < _mapGenSource.map[i].List.Count; j++)
//                {
//                    for (int k = 0; k < _mapGenSource.map[i].List[j].items.Length; k++)
//                    {
//                        tmp++;
//                    }
//                }
//            }

//            if (_mapGenHint.itemcnt < tmp)
//            {
//                Debug.LogWarning("NoWay;; MingMingTT");

//                return;
//            }

//            List<ItemSO> randomitemlist = new List<ItemSO>();

//            int itemcnttmp = _mapGenHint.itemcnt;
//            while (itemcnttmp > 0)
//            {
//                ItemSO inertItemming = _mapGenHint.ExistItems[Random.Range(0, _mapGenHint.ExistItems.Length - 1)];
//                for (int i = 0; i < 3; i++)
//                {
//                    randomitemlist.Add(inertItemming);
//                    itemcnttmp--;
//                }

//            }

//            while (randomitemlist.Count > 0)
//            {
//                for (int i = 0; i < _mapGenSource.map.Count; i++)
//                {
//                    for (int j = 0; j < _mapGenSource.map[i].List.Count; j++)
//                    {
//                        for (int k = 0; k < _mapGenSource.map[i].List[j].items.Length; k++)
//                        {



//                            if (Random.Range(0, 9) < 2)
//                            {

//                                if (randomitemlist.Count <= 0)
//                                {
//                                    return;
//                                }

//                                _mapGenSource.map[i].List[j].items[k] = randomitemlist[0];

//                                randomitemlist.RemoveAt(0);

//                            }
//                        }

//                        _mapGenHint.itemcnt

//                        if (j == 0 && _mapGenSource.map[i].List[j].EmptyCnt(null) >  )
//                        {

//                        }
//                    }
//                }
//            }

//        }
//    }
//}
#endregion