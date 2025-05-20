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
		Managers.Map.LoadMap("Level");
		return true;
	}

	public override void Clear()
	{

	}
}
