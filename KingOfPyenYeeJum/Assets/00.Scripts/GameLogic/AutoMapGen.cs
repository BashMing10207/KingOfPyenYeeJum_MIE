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

    // ▼▼ 추가: 목표 R(1), R(2) (에디터에서 세팅)
    [Header("Residue Targets (R(3)=0은 자동보장)")]
    [SerializeField] private int _targetR1 = 7;
    [SerializeField] private int _targetR2 = 7;

    // ========================= 기존 메서드 (수정 X) =========================
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

    // ========================= 신규: 역생성 메인 엔트리 =========================
    [ContextMenu("GenerateByResidueTargets")]
    private void GenerateByResidueTargets()
    {
        if (_mapGenSource == null || _mapGenHint == null || _mapGenHint.ExistItems == null || _mapGenHint.ExistItems.Length == 0)
        {
            Debug.LogError("[GenerateByResidueTargets] Missing MapGenSource or Hint/Items.");
            return;
        }

        // 1) 맵 구조 보정: 모든 슬롯이 3개 레이어를 갖도록 맞춤
        EnsureThreeLayers();

        // 2) 맵 비우기
        ClearAllItems();

        // 3) (r1,r2) 블록으로 targetR1, targetR2 맞추기
        var blocks = BuildBlocksGreedy(_targetR1, _targetR2); // List<(r1,r2)>

        // ★ 추가: "중간(2층)이 완전히 비면 안 됨" 보정
        EnsureNoEmptyMiddleLayer(ref blocks);

        // 4) 각 블록을 (d1,d2,d3)로 변환하고, 실제 아이템SO를 골라 레이어별 필요 개수 누적
        var layerNeeds = new List<LayerNeed>(); // per-type needs
        foreach (var block in blocks)
        {
            var (d1, d2) = MapResidueToD(block.r1, block.r2);           // (d1,d2) ∈ {0,1,2}
            int d3 = Mod3(-d1 - d2);                                    // r3=0 보장
            var item = _mapGenHint.ExistItems[Random.Range(0, _mapGenHint.ExistItems.Length)];
            layerNeeds.Add(new LayerNeed(item, d1, d2, d3));
        }

        // 5) 용량 체크: 각 레이어에 채울 수 있는 빈칸(=null 슬롯) 확인
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

        // 6) 실제 배치: 레이어별로 null 칸을 순회하며 채움 (최소 배치; 밀도 높이고 싶으면 +3k씩 더 올리면 됨)
        foreach (var need in layerNeeds)
        {
            PlaceIntoLayer(0, need.item, need.d1);
            PlaceIntoLayer(1, need.item, need.d2);
            PlaceIntoLayer(2, need.item, need.d3);
        }




        // 7) 검증: 계산된 R(1),R(2),R(3) 출력
        var R = ComputeR();
        Debug.Log($"[GenerateByResidueTargets] R(1)={R[0]}, R(2)={R[1]}, R(3)={R[2]}  (Target {_targetR1}, {_targetR2}, 0)");
    }

    // ========================= 헬퍼들: 구조 보정/배치/검증 =========================

    // 클래스 내부에 추가
    private void EnsureNoEmptyMiddleLayer(ref List<Block> blocks)
    {
        // 최대 몇 번까지 시도 (무한루프 방지)
        for (int attempt = 0; attempt < 64; attempt++)
        {
            var sums = SumDForBlocks(blocks); // (sum d1, d2, d3)
            int s1 = sums.d1, s2 = sums.d2, s3 = sums.d3;

            // 조건 충족: 뒤 레이어(d3)에 아이템이 있는데, 중간 레이어(d2)가 0 → 금지
            if (s3 > 0 && s2 == 0)
            {
                // r1==r2(>0)인 블록을 찾아 분해 (가장 간단한 두 케이스 우선)
                int idx = blocks.FindIndex(b => b.r1 == b.r2 && b.r1 > 0);
                if (idx < 0)
                {
                    Debug.LogWarning("[EnsureNoEmptyMiddleLayer] 교체 가능한 (r1==r2) 블록이 없어 보정 실패. 타겟 R을 재설계 필요.");
                    break;
                }

                var b = blocks[idx];
                blocks.RemoveAt(idx);

                if (b.r1 == 1) // (1,1) → (1,0) + (0,1)
                {
                    blocks.Add(new Block(1, 0));
                    blocks.Add(new Block(0, 1));
                }
                else if (b.r1 == 2) // (2,2) → (1,2) + (1,0)  (다른 조합도 가능: (2,1)+(0,1))
                {
                    blocks.Add(new Block(1, 2));
                    blocks.Add(new Block(1, 0));
                }
                // 루프 다시 돌며 s2가 생겼는지 재확인
                continue;
            }

            // 이미 조건 만족 (중간 레이어 비지 않음)
            break;
        }

        // (선택) 추가 제약: "레이어2에 아이템 있으면 레이어1도 최소 1"을 강제하고 싶다면
        // 아래처럼 s2>0 && s1==0일 때의 보정도 같은 방식으로 만들 수 있음.
        // 다만 targetR1==0인 맵 요구사항과 충돌할 수 있으므로 기본은 생략.
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



    // 맵의 모든 슬롯에 N개 레이어가 있도록 보정 (부족하면 추가)
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
        //    Debug.LogError("[EnsureThreeLayers] _mapGenSource.map 이 비어있습니다. 슬롯/레이아웃을 먼저 준비하세요.");
        //    return;
        //}

        //for (int i = 0; i < _mapGenSource.map.Count; i++)
        //{
        //    var blist = _mapGenSource.map[i];
        //    if (blist.List == null)
        //        blist.List = new List<InsideArray<ItemSO>>();

        //    if (blist.List.Count == 0)
        //    {
        //        // 최소 1개 레이어라도 없으면 기본 1칸짜리 생성
        //        blist.List.Add(new InsideArray<ItemSO>(1));
        //    }

        //    // 기준 길이(레이어0의 슬롯 수)
        //    int baseLen = Mathf.Max(1, blist.List[0].items?.Length ?? 1);

        //    // 레이어 수 3개로 맞춤
        //    while (blist.List.Count < _mapGenHint.LayerCnt)
        //    {
        //        blist.List.Add(new InsideArray<ItemSO>(baseLen));
        //    }

        //    // 모든 레이어의 items 배열이 null이면 새로 생성
        //    for (int k = 0; k < 3; k++)
        //    {
        //        if (blist.List[k].items == null || blist.List[k].items.Length == 0)
        //            blist.List[k] = new InsideArray<ItemSO>(baseLen);
        //    }
        //}
    }

    // 모든 아이템 비우기(null로)
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

    // 특정 레이어에서 null 칸 개수 세기
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

    // 특정 레이어에 item을 count개 배치 (null 칸을 순회하며 채움)
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

    // 현재 맵의 R(1),R(2),R(3) 계산
    // R(k) = sum_t ( (∑_{ℓ≤k} a_{t,ℓ}) % 3 )
    private int[] ComputeR()
    {
        // 종류별 누적 개수 집계용
        Dictionary<ItemSO, int[]> cumPerType = new Dictionary<ItemSO, int[]>();
        // null도 한 종류로 취급 X (빈칸은 무시)

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
                        cum = new int[3]; // 레이어별 누적
                        cumPerType[it] = cum;
                    }
                    // k층까지의 누적을 계산해야 하므로, 일단 레이어별 개수를 따로 세서 나중에 prefix로 바꿀 수도 있지만
                    // 여기선 단순화를 위해 "k에 도달하면 그때 누적 더하기" 방식으로 2-pass를 사용
                }
            }
        }

        // 먼저 종류별 레이어 개수 countPerLayer[it][k] 세기
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

    // ========================= 잔여도 블록/변환 로직 =========================
    private struct Block { public int r1, r2; public Block(int a, int b) { r1 = a; r2 = b; } }
    private struct LayerNeed
    {
        public ItemSO item;
        public int d1, d2, d3;
        public LayerNeed(ItemSO item, int d1, int d2, int d3)
        { this.item = item; this.d1 = d1; this.d2 = d2; this.d3 = d3; }
    }

    // targetR1, targetR2를 (r1,r2) 블록들의 합으로 정확히 맞추는 간단 그리디
    private List<Block> BuildBlocksGreedy(int targetR1, int targetR2)
    {
        List<Block> list = new List<Block>();
        int S1 = 0, S2 = 0;

        while (S1 < targetR1 || S2 < targetR2)
        {
            if (targetR1 - S1 >= 2)
            {
                int want2 = Mathf.Clamp(targetR2 - S2, 0, 2);
                // (2,2) 우선 → (2,1) → (2,0)
                if (want2 == 2) { list.Add(new Block(2, 2)); S1 += 2; S2 += 2; }
                else if (want2 == 1) { list.Add(new Block(2, 1)); S1 += 2; S2 += 1; }
                else { list.Add(new Block(2, 0)); S1 += 2; }
            }
            else if (targetR1 - S1 == 1)
            {
                int want2 = Mathf.Clamp(targetR2 - S2, 0, 2);
                // (1,2) → (1,1) → (1,0)
                if (want2 == 2) { list.Add(new Block(1, 2)); S1 += 1; S2 += 2; }
                else if (want2 == 1) { list.Add(new Block(1, 1)); S1 += 1; S2 += 1; }
                else { list.Add(new Block(1, 0)); S1 += 1; }
            }
            else // S1 == targetR1
            {
                int want2 = Mathf.Clamp(targetR2 - S2, 0, 2);
                // (0,2) → (0,1) → (0,0)
                if (want2 == 2) { list.Add(new Block(0, 2)); S2 += 2; }
                else if (want2 == 1) { list.Add(new Block(0, 1)); S2 += 1; }
                else { list.Add(new Block(0, 0)); /* no change */ }
            }
        }

        return list;
    }

    // (r1,r2) → (d1,d2) 매핑 테이블 (본문 설명의 표)
    private (int d1, int d2) MapResidueToD(int r1, int r2)
    {
        // 기본값 안전장치
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