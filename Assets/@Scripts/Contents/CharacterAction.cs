using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAction : MonoBehaviour
{
    public Animator animator;
 
    public void SetAnimator(Animator anim)
    {
        animator = anim;
    }
    public void WaitWithFood()
    {
        if(animator.GetBool("tasirkenDur") == true)
            return;
        animator.SetBool("tasirkenDur",true);
        animator.SetBool("yuru",false);
        animator.SetBool("idle",false);
        animator.SetBool("tasi",false);
    }
    public void Walk()
    {
        if(animator.GetBool("yuru") == true)
            return;
        animator.SetBool("tasirkenDur",false);
        animator.SetBool("yuru",true);
        animator.SetBool("idle",false);
        animator.SetBool("tasi",false);
    }
    public void WalkIdle()
    {

        animator.SetBool("tasirkenDur",false);
        animator.SetBool("yuru",true);
        animator.SetBool("idle",false);
        animator.SetBool("tasi",false);
    }
    public void Idle()
    {
        if(animator.GetBool("idle") == true)
            return;
        animator.SetBool("tasirkenDur",false);
        animator.SetBool("yuru",false);
        animator.SetBool("idle",true);
        animator.SetBool("tasi",false);
    }

    
    public void Carry()
    {
        if(animator.GetBool("tasi") == true)
            return;
        animator.SetBool("tasirkenDur",false);
        animator.SetBool("yuru",false);
        animator.SetBool("idle",false);
        animator.SetBool("tasi",true);
    }
    public void CustomerWalk()
    {
        if (animator == null)
        {
            Debug.Log("[CharacterAction] animator가 할당되지 않았습니다! (CustomerWalk)");
            return;
        }
        animator.SetBool("ayaktaIdle",false);
        animator.SetBool("yuru",true);
        animator.SetBool("eating",false);
        animator.SetBool("idle",false);        
    }
    public void CustomerSit()
    {
          Debug.Log("CustomerSit() 호출됨");
        animator.SetBool("ayaktaIdle",false);
        animator.SetTrigger("otur");
        animator.SetBool("yuru",false);
        animator.SetBool("eating",false);
        animator.SetBool("idle",false); 
    }
    public void CustomerOrder()
    {
        animator.SetBool("ayaktaIdle",false);
        animator.SetTrigger("siparis");
        animator.SetBool("yuru",false);
        animator.SetBool("eating",false);
        animator.SetBool("idle",false); 
    }
    public void CustomerSitIdle()
    {
        animator.SetBool("ayaktaIdle",false);
        animator.SetBool("yuru",false);
        animator.SetBool("eating",false);
        animator.SetBool("idle",true); 
    }
    public void CustomerStand()
    {
        animator.SetBool("ayaktaIdle",false);
        animator.SetTrigger("kalk");
        animator.SetBool("yuru",false);
        animator.SetBool("eating",false);
        animator.SetBool("idle",false); 
    }
    public void CustomerEat()
    {
        animator.SetBool("ayaktaIdle",false);
        animator.SetBool("idle",false); 
        animator.SetBool("yuru",false);
        animator.SetBool("eating",false);
        animator.SetBool("eating",true);
    }
    public void CustomerStandIdle()
    {
        if (animator == null)
        {
            Debug.Log("[CharacterAction] animator가 할당되지 않았습니다! (CustomerStandIdle)");
            return;
        }
        animator.SetBool("ayaktaIdle",true); 
        animator.SetBool("idle",false); 
        animator.SetBool("yuru",false);
        animator.SetBool("eating",false);
        animator.SetBool("eating",false);
    }

   
}
