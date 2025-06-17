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

	public GameObject RecipeUI;

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

		RecipeUI.SetActive(false); //임시로 넣은 레시피이미지 + 버튼
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
		RecipeUI.SetActive(true);

		/////////// 이거 이미지 넣었을 때 내가 지정한 이미지 크기랑 로드했을 때 크기랑 달라서... 근데 왜그런지 잘 모르겠어서 그냥 일단 무식하게 키웠습니다... 나중에 알려주세요요
		// 먼저 RecipeUI 전체를 크게 만들기
		RectTransform recipeUITransform = RecipeUI.GetComponent<RectTransform>();
		if (recipeUITransform != null)
		{
			recipeUITransform.localScale = Vector3.one * 2.0f; // 부모를 2배로 크게
			Debug.Log("<color=magenta>[UI_StartUpScene]</color> RecipeUI 전체 크기 조정: 2.0배");
		}
		
		// 그 다음 내부 레시피 이미지도 추가로 크게 만들기
		bool foundRecipeImage = false;
		
		// 방법 1: "recipe" 이름으로 찾기
		Transform recipeImageTransform = RecipeUI.transform.Find("recipe");
		if (recipeImageTransform != null)
		{
			recipeImageTransform.localScale = Vector3.one * 1.5f; // 자식도 2.5배로 크게
			Debug.Log("<color=green>[UI_StartUpScene]</color> 레시피 이미지 크기 조정: 2.5배 (Find로 발견)");
			foundRecipeImage = true;
		}
		
		// 방법 2: 모든 자식 오브젝트 중에서 이미지 컴포넌트가 있는 것들 찾기
		if (!foundRecipeImage)
		{
			for (int i = 0; i < RecipeUI.transform.childCount; i++)
			{
				Transform child = RecipeUI.transform.GetChild(i);
				if (child.GetComponent<Image>() != null)
				{
					child.localScale = Vector3.one * 1.5f; // 자식도 2.5배로 크게
					Debug.Log($"<color=cyan>[UI_StartUpScene]</color> 자식 이미지 '{child.name}' 크기 조정: 2.5배");
					foundRecipeImage = true;
				}
			}
		}
		
		// Managers.Scene.LoadScene(EScene.RecipeScene);
	}

	public void CloseRecipeUI() //레시피 닫기
	{
		Debug.Log("CloseRecipeUI");
		RecipeUI.SetActive(false);
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
		
		// 에셋 로딩 완료 후 BGM 재생
		PlayBGM();
		Debug.Log("<color=yellow>[UI_StartUpScene]</color> 에셋 로딩 완료 - BGM 재생 시작");
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
				Debug.LogWarning("<color=yellow>[UI_StartUpScene]</color> spring-day AudioClip을 찾을 수 없습니다.");
				return;
			}
			
			Managers.Sound.Play(Define.ESound.Bgm, audioClip);
			Debug.Log("<color=green>[UI_StartUpScene]</color> BGM 재생: spring-day");
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
