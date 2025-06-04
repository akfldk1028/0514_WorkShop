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
    public float turnSpeed = 30f;



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
     
        CameraController cameraController = FindObjectOfType<CameraController>();
        if (cameraController != null)
        {
            cameraController.Target = this;
        }

    }

        void FixedUpdate()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        Debug.Log("[Player] FixedUpdate 호출");
        if (rb != null)
        {
            float speed = 5f;
            
            // 전진/후진 처리 (Y축 입력)
            Vector3 moveDir = Vector3.zero;
            if (_inputDir.y != 0)
            {
                moveDir = transform.forward * _inputDir.y;  // 로컬 forward 방향으로 이동
            }
            
            // 회전 처리 (X축 입력)
            if (_inputDir.x != 0)
            {
                float rotationSpeed = turnSpeed * _inputDir.x;
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0, rotationSpeed * Time.fixedDeltaTime, 0));
            }
            
            // 이동 적용
            rb.linearVelocity = moveDir * speed;

            if (modelAnimator != null && action != null)
            {
                Debug.Log("[Player] action : " + action);
                if (_inputDir.sqrMagnitude > 0.01f)
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

