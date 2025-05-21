using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

using static Define;


/*
 🧩 큐 시스템 통합 설계

 ✅ NPC
   - 자동 상태 기반 priority 설정
   - 큐에 추가되어 대기
   - 자동 상태머신 기반 조리 수행

 ✅ 플레이어
   - int.MaxValue 등으로 우선순위 최고로 설정하거나
   - 큐를 완전히 생략하고 UseDirectly() 수행
   - 조리 UI/애니메이션 수동 조작

 ✅ 장점
   - 유지보수 편리 (플레이어/AI 공통 시스템 사용)
   - 멀티플레이 or AI 대체 가능
   - 직관적 큐 시각화 및 상태 흐름 처리 용이
*/


public class Item : BaseObject
{
    public int plateCount;
    public List<GameObject> chefs;
    public List<GameObject> dishwashers;
    public List<Transform> createdQueueTransform;
    public Transform chefPlace;
    public List<Transform> platePlaces;

    private PriorityQueue<Unit> _queue = new PriorityQueue<Unit>();

    // ✅ 외부에서 접근 가능하게 public getter 제공
    public PriorityQueue<Unit> queue => _queue;

    public override bool Init()
    {
        if (!base.Init())
            return false;

        ObjectType = EObjectType.Item; // 너희 게임에 맞게 Enum 지정
        return true;
    }

    public void CreateQueue(Unit unit)
    {
        _queue.Push(unit);
        UpdateQueueVisual();
    }

    public void UpdateQueue(Unit unit)
    {
        // Remove 시뮬레이션
        List<Unit> temp = new List<Unit>();
        while (_queue.Count > 0)
        {
            var u = _queue.Pop();
            if (u != unit)
                temp.Add(u);
        }

        foreach (var u in temp)
            _queue.Push(u);

        UpdateQueueVisual();
    }

    private void UpdateQueueVisual()
    {
        List<Unit> orderedUnits = new List<Unit>();
        while (_queue.Count > 0)
        {
            orderedUnits.Add(_queue.Pop());
        }

        for (int i = 0; i < orderedUnits.Count && i < createdQueueTransform.Count; i++)
        {
            Unit unit = orderedUnits[i];
            unit.queueState.queuePlace = createdQueueTransform[i].position;
            unit.queueState.isUpdate = true;
            unit.currState = unit.queueState;
        }

        foreach (var unit in orderedUnits)
        {
            _queue.Push(unit);
        }
    }
}




// public class Item : MonoBehaviour
// {
//     public int plateCount;
//     public List<GameObject> chefs;
//     public List<GameObject> dishwashers;
//     public List<Transform> createdQueueTransform;
//     [SerializeField] private List<Unit> _queue;
//     public List<Unit> queue { get => _queue; set { _queue = value;}}
//     public Transform chefPlace;
//     public List <Transform> platePlaces;
//     public void CreateQueue(Unit unit)
//     {
//         if(!queue.Contains(unit))
//         {
//             queue.Add(unit);
//         }
//     }
//     public void UpdateQueue(Unit unit)
//     {
//         if(queue.Contains(unit))
//         {
//             queue.Remove(unit);
//         }
//         for (int i = 0; i < queue.Count; i++)
//         {
            
//             // queue[i].queueState.oncekiState = queue[i].currState;
//             queue[i].queueState.queuePlace = createdQueueTransform[i].position;
//             queue[i].queueState.isUpdate = true;
//             queue[i].currState = queue[i].queueState;
//         }
//     }
    
// }
