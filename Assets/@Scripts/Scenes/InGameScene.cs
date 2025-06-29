using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class InGameScene : BaseScene
{
	public GameObject interactionCanvas;
    public GameObject StartText;
    
    [Header("Debug")]
    public bool isInteractionCanvasFound = false;
    public bool isStartTextFound = false;
    
    // InGameManager가 참조할 static 프로퍼티들
    public static GameObject PlayerObj { get; private set; }
    public static PlayerMove PlayerMove { get; private set; }
    public static CameraController CameraController { get; private set; }
    public static GameObject InteractionCanvas { get; private set; }
    public static GameObject StartTextObj { get; private set; }
    public static RhythmGameManager RhythmGameManager { get; private set; }

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
		
		AutoAssign();

		// BGM 재생 (StartUpScene에서 이어서 재생)
		PlayBGM();

		return true;
	}
	
	private void AutoAssign()
	{
		// Player 찾기


		PlayerObj = Managers.Game.Player.gameObject;
		if (PlayerObj == null)
			Debug.Log("<color=magenta>[InGameScene]</color> Player 못 찾음");
		// CameraController 찾기
		if (CameraController == null)
        	CameraController = FindObjectOfType<CameraController>();
		// InteractionCanvas 찾기
		if (interactionCanvas == null)
            {
				Debug.Log("<color=magenta>[InGameScene]</color> interactionCanvas 찾기");
                interactionCanvas = GameObject.Find("GameCanvas");
				if (interactionCanvas != null){
					isInteractionCanvasFound = true;
					Debug.Log("<color=magenta>[InGameScene]</color> interactionCanvas 찾음");
					interactionCanvas.SetActive(false);
				}
            	if (StartText == null && interactionCanvas != null)
				{
					Transform found = interactionCanvas.transform.Find("StartText");
					if (found != null)
					{
						isStartTextFound = true;
						Debug.Log("<color=magenta>[InGameScene]</color> StartText 찾음");
						StartText = found.gameObject;
					}
				}
			}
		
		// Static 프로퍼티에 할당
		InteractionCanvas = interactionCanvas;
		StartTextObj = StartText;
		if (RhythmGameManager == null)
		{
			GameObject obj = GameObject.Find("RhythmManager");
			if (obj != null)
			{
				RhythmGameManager = obj.GetComponent<RhythmGameManager>();
				Debug.Log("<color=magenta>[InGameScene]</color> RhythmManager 찾음");
			}
		}
	}

	/// <summary>
	/// BGM을 재생합니다.
	/// </summary>
	private void PlayBGM()
	{
		try 
		{
			AudioClip audioClip = Managers.Resource.Load<AudioClip>("spring-day");
			if (audioClip == null)
			{
				Debug.LogWarning("<color=yellow>[InGameScene]</color> spring-day AudioClip을 찾을 수 없습니다.");
				return;
			}
			
			Managers.Sound.Play(Define.ESound.Bgm, audioClip);
		}
		catch (System.Exception e)
		{
			Debug.LogError($"<color=red>[InGameScene]</color> BGM 재생 실패: {e.Message}");
		}
	}

  public override void Clear()
    {
        // 씬 종료 시 정리
        Managers.Game.CustomerCreator.StopAutoSpawn();
    }
}
