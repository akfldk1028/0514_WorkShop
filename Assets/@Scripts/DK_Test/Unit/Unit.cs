using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Define;
using System;

public class Unit : BaseObject, IComparable<Unit>
{	
    public Data.CreatureData CreatureData { get; private set; }

    public bool isHandFull;
    public Transform hand;
    public Slider slider;
    public Image queueImage;
    public NavMeshAgent agent;
    public int priority;  // 우선순위 숫자 (높을수록 먼저 처리되게 할 수도 있음)
public Action_Test action;  // Inspector에서 할당

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

        action = GetComponent<Action_Test>();
        return true;
    }

    public virtual void SetInfo<T>(int templateID, T clientCreature) 
    {
    	DataTemplateID = templateID;

		if (ObjectType == EObjectType.Customer)
			CreatureData = Managers.Data.CustomerDic[templateID];


    }
    protected ECreatureState _creatureState = ECreatureState.None;

	public virtual ECreatureState CreatureState
	{
		get { return _creatureState; }
		set
		{
			if (_creatureState != value)
			{
				_creatureState = value;
				UpdateAnimation();
			}
		}
	}

    public int CompareTo(Unit other)
    {
        if (other == null) return 1;
        return other.priority.CompareTo(this.priority); // 높은 priority가 먼저
    }
}
