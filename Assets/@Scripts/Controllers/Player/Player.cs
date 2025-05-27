using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static Define;
using System.Collections.Generic;
using TMPro;


public class Player : Unit
{
    
        private Vector2 _inputDir = Vector2.zero;

    public GameObject modelInstance;
    public Animator modelAnimator;
    public float turnSpeed = 20f;



    public override bool Init()
    {
        if (!base.Init())
            return false;

        ObjectType = EObjectType.Player;
        return true;
    }

    public override void SetInfo<T>(int templateID, T client)
    {
        base.SetInfo(templateID, client);
        ClientPlayer clientPlayer = client as ClientPlayer;
        if (clientPlayer?.ModelPrefab != null)
        {
            SetupCustomerModel(clientPlayer);
        }
     
    }

        void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log("[Player] FixedUpdate 호출");
        if (rb != null)
        {
            float speed = 5f;
            Vector3 moveDir = new Vector3(_inputDir.x, 0, _inputDir.y);
            // 이동
            rb.linearVelocity = moveDir * speed;
            // 회전 (입력 방향이 있을 때만)
            if (moveDir.sqrMagnitude > 0.01f)
            {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDir, Vector3.up);
                    rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime));
                    Debug.Log("[Player] moveDir : " + moveDir);
            }

            if (modelAnimator != null && action != null)
            {
                Debug.Log("[Player] action : " + action);
                if (moveDir.sqrMagnitude > 0.01f)
                {
                    action.WalkIdle();
                }
                else
                {
                    action.Idle();
                }
            }
        }
    }
    
    public void Move(Vector2 dir)
    {
        Debug.Log("[Player] Move 호출: " + dir);
        _inputDir = dir;

    }
    
    private void SetupCustomerModel(ClientPlayer clientPlayer)
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        modelInstance = Instantiate(clientPlayer.ModelPrefab, transform.position, Quaternion.identity);
     
        modelInstance.transform.SetParent(transform);

        modelAnimator = modelInstance.GetComponent<Animator>();
        if (modelAnimator != null && clientPlayer.AnimatorController != null)
        {
            modelAnimator.runtimeAnimatorController = clientPlayer.AnimatorController;
        }

            // CharacterAction에 Animator 할당
        if (action != null)
        {
            action.SetAnimator(modelAnimator);
        
        }
    }
}

