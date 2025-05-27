using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameScene : BaseScene
{
	public override bool Init()
	{
		if (base.Init() == false)
			return false;
		Debug.Log("<color=magenta>[GameScene]</color> Init");

		SceneType = EScene.GameScene;
		Managers.Map.LoadMap("Restaurant");
        Managers.Game.CustomerCreator.StartAutoSpawn();
		// Managers.Game.PlayerManager.SetInfo();
		Vector3 playerPos = new Vector3(39.0f, 0.0f, 3.0f);
		Player player = Managers.Object.Spawn<Player>(playerPos, 201000);
        Managers.Game.SetPlayer(player); // GameManager에 등록
		Debug.Log("player : " + player);
		    // 프리팹 로드
    	GameObject highlightPrefab = Managers.Resource.Load<GameObject>("Quad");
		GameObject placeablePrefab = Managers.Resource.Load<GameObject>("Lock");
		
		// 그리드 컨트롤 초기화
		Managers.Game_DK.InitGridControl(highlightPrefab, placeablePrefab);
		return true;
	}

  public override void Clear()
    {
        // 씬 종료 시 정리
        Managers.Game.CustomerCreator.StopAutoSpawn();
    }
}
