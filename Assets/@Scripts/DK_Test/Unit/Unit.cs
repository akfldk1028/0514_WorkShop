using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Define;
using System;

public abstract class Unit : BaseObject, IComparable<Unit>
{
    public bool isHandFull;
    public Transform hand;
    public Slider slider;
    public Image queueImage;
    public Action_Test action;
    public NavMeshAgent agent;
    public int priority;  // 우선순위 숫자 (높을수록 먼저 처리되게 할 수도 있음)

    [Header("States")]
    [SerializeField] private StateBase _currState;
    [Space(5)]
    public QueueBekleState queueWaitState;
    public QueueState queueState;

    public StateBase currState
    {
        get => _currState;
        set {
            _currState = value;
            _currState.StartState(action);
        }
    }

    public override bool Init()
    {
        if (!base.Init())
            return false;

        ObjectType = EObjectType.Unit;
        return true;
    }
        public int CompareTo(Unit other)
    {
        if (other == null) return 1;
        return other.priority.CompareTo(this.priority); // 높은 priority가 먼저
    }
}
