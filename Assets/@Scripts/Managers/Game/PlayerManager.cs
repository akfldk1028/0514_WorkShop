using UnityEngine;

public class PlayerManager
{
    private Player _player; // 실제 플레이어 오브젝트 참조

    public PlayerManager()
    {
        Debug.Log("<color=orange>[PlayerManager]</color> 생성됨");
        // _player = GameObject.FindWithTag("Player"); // 또는 생성 시점에 할당
    }

    public void SetInfo()
    {

    }

    public void Move(Vector2 dir)
    {
        if (_player == null) return;
        Rigidbody rb = _player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = 5f; // 필요시 속도 조정
            // 2D 입력을 3D로 변환 (예: x,z 평면 이동)
            Vector3 moveDir = new Vector3(dir.x, 0, dir.y);
            rb.linearVelocity = moveDir * speed;
        }
        // Transform 이동 방식 예시:
        // _player.transform.position += new Vector3(dir.x, 0, dir.y) * speed * Time.deltaTime;
    }

    private void SpawnPlayer()
    {
        _player = Managers.Object.Spawn<Player>(Vector3.zero, 201000);
        Debug.Log("[PlayerManager] 플레이어 생성: " + _player);
        if (_player != null)
        {
            Managers.PublishAction(ActionType.Player_Spawned);
        }
    }

}