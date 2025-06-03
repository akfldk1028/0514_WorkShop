using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class InGameScene : BaseScene
{
	public override bool Init()
	{
		if (base.Init() == false)
			return false;
		Debug.Log("<color=magenta>[GameScene]</color> Init");

		SceneType = EScene.IngameScene;
		Managers.Map.LoadMap("RestaurantBar");
        Managers.Game.CustomerCreator.StartAutoSpawn();

		Vector3 playerPos = Managers.Map.PlayerPosition;
		Player player = Managers.Object.Spawn<Player>(playerPos, 201000);
        Managers.Game.SetPlayer(player); // GameManager에 등록
	
    	GameObject highlightPrefab = Managers.Resource.Load<GameObject>("Quad");
		
		Managers.Placement.InitGridControl(highlightPrefab);

		UI_GameScene sceneUI = Managers.UI.ShowSceneUI<UI_GameScene>();
		sceneUI.GetComponent<Canvas>().sortingOrder = 100;
		sceneUI.SetInfo();


		return true;
	}

  public override void Clear()
    {
        // 씬 종료 시 정리
        Managers.Game.CustomerCreator.StopAutoSpawn();
    }
}
