using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Keyboard
{
    

    
    public Keyboard()
    {
        Debug.Log("<color=cyan>[Keyboard]</color> 생성됨");
    }
    
    public void SetInfo()
    {

 
    }
    public void Update()
    {
        Vector2 moveDir = Vector2.zero;
        if (Input.GetKey(KeyCode.W)) moveDir.y += 1;
        if (Input.GetKey(KeyCode.S)) moveDir.y -= 1;
        if (Input.GetKey(KeyCode.A)) moveDir.x -= 1;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1;

        // 방향키가 눌렸을 때만 갱신
        if (moveDir != Managers.Game.MoveDir)
        {
            Managers.Game.MoveDir = moveDir.normalized;
        }
    }
}