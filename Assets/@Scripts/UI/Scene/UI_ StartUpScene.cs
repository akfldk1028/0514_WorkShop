using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Define;

public class UI_StartUpScene : UI_Scene
{
    enum GameObjects
    {
        StartImage
    }

    enum Texts
    {
        DisplayText
    }

	enum Buttons
	{
		StartButton,
		RecipeButton,
		ExitButton,
	}

	[Header("BGM Settings")]
	[SerializeField] private string bgmFileName = "StartUpScene_bgm"; // BGM 파일명
	[SerializeField] private float bgmVolume = 0.5f; // BGM 볼륨

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindObjects(typeof(GameObjects));
        BindTexts(typeof(Texts));
		BindButtons(typeof(Buttons));
		// GetObject((int)GameObjects.StartImage).BindEvent((evt) =>
		// {
		// 	Debug.Log("ChangeScene");
		// 	Managers.Scene.LoadScene(EScene.IngameScene);
		// });

		GetObject((int)GameObjects.StartImage).gameObject.SetActive(false);
		// GetText((int)Texts.DisplayText).text = $"StartUpScene";
		Debug.Log($"<color=cyan>[UI_StartUpScene]</color> Asset Load 합니다.");
		
	
		GetButton((int)Buttons.RecipeButton).gameObject.BindEvent(OnClickRecipeButton);
		GetButton((int)Buttons.ExitButton).gameObject.BindEvent(OnClickExitButton);

		StartLoadAssets();
// PlayBGM();
		return true;
    }

	void OnClickStartButton(PointerEventData evt)
	{
		Debug.Log("StartButton");
		Managers.Scene.LoadScene(EScene.IngameScene);
	}

	void OnClickRecipeButton(PointerEventData evt)
	{
		Debug.Log("RecipeButton");
		// Managers.Scene.LoadScene(EScene.RecipeScene);
	}


	void OnClickExitButton(PointerEventData evt)
	{
		Debug.Log("ExitButton");
		Application.Quit();
	}

	async void StartLoadAssets() {
		await Managers.Data.StartLoadAssetsAsync();
		
		// Canvas 레이아웃 강제 업데이트 - 버튼 크기 문제 해결
		Canvas.ForceUpdateCanvases();
		
		// 모든 버튼 크기를 0.4로 강제 설정
		GetButton((int)Buttons.StartButton).transform.localScale = Vector3.one * 0.4f;
		GetButton((int)Buttons.RecipeButton).transform.localScale = Vector3.one * 0.4f;
		GetButton((int)Buttons.ExitButton).transform.localScale = Vector3.one * 0.4f;
		
		// 버튼 위치 설정
		RectTransform recipeRect = GetButton((int)Buttons.RecipeButton).GetComponent<RectTransform>();
		recipeRect.anchoredPosition = new Vector2(recipeRect.anchoredPosition.x, -300f);
		
		RectTransform exitRect = GetButton((int)Buttons.ExitButton).GetComponent<RectTransform>();
		exitRect.anchoredPosition = new Vector2(exitRect.anchoredPosition.x, -540f);
		
		GetObject((int)GameObjects.StartImage).gameObject.SetActive(true);
		GetButton((int)Buttons.StartButton).gameObject.BindEvent(OnClickStartButton);
		// GetText((int)Texts.DisplayText).text = "Touch To Start";
		
		// 에셋 로딩 완료 후 BGM이 재생되고 있는지 확인
		Debug.Log("<color=yellow>[UI_StartUpScene]</color> 에셋 로딩 완료 - BGM 재생 중");
	}

	/// <summary>
	/// BGM을 재생합니다.
	/// </summary>
	private void PlayBGM()
	{
		try 
		{
			// SoundManager를 통해 BGM 재생
			Managers.Sound.Play(Define.ESound.Bgm, bgmFileName);
			Debug.Log($"<color=green>[UI_StartUpScene]</color> BGM 재생 시작: {bgmFileName}");
		}
		catch (System.Exception e)
		{
			Debug.LogError($"<color=red>[UI_StartUpScene]</color> BGM 재생 실패: {e.Message}");
		}
	}

	/// <summary>
	/// BGM을 정지합니다.
	/// </summary>
	private void StopBGM()
	{
		Managers.Sound.Stop(Define.ESound.Bgm);
		Debug.Log("<color=yellow>[UI_StartUpScene]</color> BGM 정지");
	}

	// 씬이 파괴될 때 호출 (선택사항)
	private void OnDestroy()
	{
		// 필요한 경우 BGM 정지
		// StopBGM();
	}

	// void StartLoadAssets()
	// {
	// 	Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
	// 	{
	// 		Debug.Log($"<color=cyan>[UI_StartUpScene]</color> {key} {count}/{totalCount}");

	// 		if (count == totalCount)
	// 		{
	// 			Managers.Data.Init();

	// 			// // 데이터 있는지 확인
	// 			// if (Managers.Game.LoadGame() == false)
	// 			// {
	// 			// 	Managers.Game.InitGame();
	// 			// 	Managers.Game.SaveGame();
	// 			// }

	// 			GetObject((int)GameObjects.StartImage).gameObject.SetActive(true);
	// 			GetText((int)Texts.DisplayText).text = "Touch To Start";
	// 		}
	// 	});
	// }
}
