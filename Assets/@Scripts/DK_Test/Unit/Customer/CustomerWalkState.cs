    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CustomerWalkState : CustomerBaseState
    {
        public override void StartState(Action_Test action)
        {
            item = Door.instance;
            item.CreateQueue(customer);

            // 가장 앞 유닛이 나인지 확인
            if(item.queue.Peek() != customer)
            {
                customer.queueState.isCarrying = false;
                customer.queueState.previousState = customer.currState;
                customer.currState = customer.queueState;
                return;
            }

            if(customer.chair == null)
            {
                action.CustomerStandIdle();
                customer.currState = customer.musteriChairBekleState;
                return;
            }

            item.UpdateQueue(customer);        
            action.CustomerWalk();
        }
        public override void UpdateState(Action_Test action)
        {
            if(Vector3.Distance(customer.agent.transform.position,customer.placeToSit.transform.position) > .3f)
            {
                return;
            }
            transform.LookAt(customer.chair.platePlace);

            customer.agent.isStopped = true;
            customer.chair.SetMusteri(customer);
            customer.currState = customer.standToSitState;
        }
    }
