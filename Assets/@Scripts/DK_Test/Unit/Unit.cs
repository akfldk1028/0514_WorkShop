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
    public CharacterAction action;  // Inspector에서 할당


    public override bool Init()
    {
        if (!base.Init())
            return false;

        action = GetComponent<CharacterAction>();
        return true;
    }

    public virtual void SetInfo<T>(int templateID, T clientCreature) 
    {
        DataTemplateID = templateID;
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            // ① 혹시 비활성화된 상태였다면 활성화
            if (!agent.enabled)
                agent.enabled = true;

            // ② 이전에 남아 있던 경로를 완전히 지워준다
            agent.ResetPath();

            // ③ (필요하다면) 지금 이 GameObject의 transform.position
            //     을 다시 NavMesh 상에 정확히 올려놓고 싶다면 Warp을 호출
            //     → 보통 SetInfo 직전 Spawn 위치를 제대로 잡아주었다면 생략해도 괜찮다.
            // agent.Warp(transform.position);
        }

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
