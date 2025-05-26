using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Item 클래스: PriorityQueue<Unit> 기반 대기열 관리 및 비주얼 업데이트 기능을 제공합니다.
/// </summary>
public class Item : BaseObject
{
    [Header("Queue Visualization")]
    [Tooltip("대기열 시각화를 위한 트랜스폼 리스트 (0번이 가장 앞)")]
    public List<Transform> queueSlots;

    // 내부 힙 기반 우선순위 큐
    private PriorityQueue<Unit> _queue = new PriorityQueue<Unit>();

    /// <summary>
    /// 현재 대기열 (외부에서 Peek/Count 사용 가능)
    /// </summary>
    public PriorityQueue<Unit> Queue => _queue;

    public override bool Init()
    {
        if (!base.Init())
            return false;
        return true;
    }

    /// <summary>
    /// 대기열에 Unit을 추가합니다. 우선순위에 따라 자동 정렬됩니다.
    /// </summary>
    public void Enqueue(Unit unit)
    {
        if (unit == null) return;
        _queue.Push(unit);
        Debug.Log($"Enqueue: {unit.name}");
    }

    /// <summary>
    /// 대기열에서 Unit을 제거합니다. 선두 단위만 Pop합니다.
    /// </summary>
    public void Dequeue(Unit unit)
    {
        if (_queue.Count == 0) return;
        // 단위가 선두에 있을 때만 Pop
        if (_queue.Peek() == unit)
            _queue.Pop();
    }
    public bool IsQueueEmpty => _queue.Count == 0;

    /// <summary>
    /// 현재 대기열에 등록된 Unit 수를 반환합니다.
    /// </summary>
    public int QueueCount => _queue.Count;
}
