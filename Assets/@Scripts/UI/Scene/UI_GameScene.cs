using UnityEngine;
using UnityEngine.EventSystems;
using System; // IDisposable 사용을 위해 추가
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    enum Buttons
    {
        // DiaPlusButton,
        // SettingButton,
        // QuestButton,
        // ChallengeButton,
       
        // CheatButton,
        shopButton,

        
    }

    enum Texts
    {
        // LevelText,
        // BattlePowerText,
        // GoldCountText,
        // DiaCountText,
        // MeatCountText,
        // WoodCountText,
        // MineralCountText,
        OrderButtonText,
        GoldCountText,
    }

    enum Sliders
    {
        // MeatSlider,
        // WoodSlider,
        // MineralSlider,
    }
 
    enum GameObjects
    {
        ReadyToServeItem,
    }

    private IDisposable _orderTextSubscription;
    private string _lastOrderText = ""; // 마지막 주문 텍스트 캐시
    private int completedRecipeCount = 0;

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));
        BindObjects(typeof(GameObjects));
        // GetButton((int)Buttons.GoldPlusButton).gameObject.BindEvent(OnClickGoldPlusButton);
        // GetButton((int)Buttons.DiaPlusButton).gameObject.BindEvent(OnClickDiaPlusButton);
        // GetButton((int)Buttons.SettingButton).gameObject.BindEvent(OnClickSettingButton);
        // GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(OnClickInventoryButton);
        // GetButton((int)Buttons.QuestButton).gameObject.BindEvent(OnClickQuestButton);
        // GetButton((int)Buttons.ChallengeButton).gameObject.BindEvent(OnClickChallengeButton);
        // GetButton((int)Buttons.CheatButton).gameObject.BindEvent(OnClickCheatButton);
        GetButton((int)Buttons.shopButton).gameObject.BindEvent(OnClickShopButton);
        // GetText((int)Texts.GoldCountText).text = "0";
        // 주문 텍스트 업데이트 액션 구독
        _orderTextSubscription = Managers.Subscribe(ActionType.GameScene_UpdateOrderText, OnUpdateOrderText);
        
        // 완료된 레시피 아이콘 추가 액션 구독
        Managers.Subscribe(ActionType.GameScene_AddCompletedRecipe, OnAddCompletedRecipe);
        
        // 카메라 뷰 전환 액션 구독
        Managers.Subscribe(ActionType.Camera_TopViewActivated, OnTopViewActivated);
        Managers.Subscribe(ActionType.Camera_BackViewActivated, OnBackViewActivated);
        
        Refresh();
        
        return true;
    }

    private float _elapsedTime = 0.0f;
    private float _updateInterval = 1.0f;

    private void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _updateInterval)
        {
            float fps = 1.0f / Time.deltaTime;
            float ms = Time.deltaTime * 1000.0f;
            string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
            // GetText((int)Texts.GoldCountText).text = text;

            _elapsedTime = 0;
        }
    }
    
    public void SetInfo()
    {
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;
    }

    void OnClickShopButton(PointerEventData evt)
    {
        Debug.Log("<color=magenta>[UI_GameScene]</color> OnClickShopButton");
        UI_TableSetting popup = Managers.UI.ShowPopupUI<UI_TableSetting>();
        popup.GetComponent<Canvas>().sortingOrder = 101;
        popup.SetInfo();
    }
	public void RefreshGoldText()
	{
		GetText((int)Texts.GoldCountText).text = Managers.Game.Gold.ToString();
	}
    void OnClickGoldPlusButton(PointerEventData evt)
    {
        Debug.Log("OnOnClickGoldPlusButton");
    }

    void OnClickDiaPlusButton(PointerEventData evt)
    {
        Debug.Log("OnClickDiaPlusButton");
    }

    void OnClickHeroesListButton(PointerEventData evt)
    {
		Debug.Log("OnClickHeroesListButton");
	}

    void OnClickSetHeroesButton(PointerEventData evt)
    {
		Debug.Log("OnClickSetHeroesButton");
	}

    void OnClickSettingButton(PointerEventData evt)
    {
		Debug.Log("OnClickSettingButton");
	}

    void OnClickInventoryButton(PointerEventData evt)
    {
        Debug.Log("OnClickInventoryButton");
    }

    void OnClickWorldMapButton(PointerEventData evt)
    {
        Debug.Log("OnClickWorldMapButton");
    }

    void OnClickQuestButton(PointerEventData evt)
    {
        Debug.Log("OnClickQuestButton");
    }

    void OnClickChallengeButton(PointerEventData evt)
    {
        Debug.Log("OnOnClickChallengeButton");
    }

    void OnClickCampButton(PointerEventData evt)
    {
        Debug.Log("OnClickCampButton");
    }

    void OnClickPortalButton(PointerEventData evt)
    {
        Debug.Log("OnClickPortalButton");
	}

    void OnClickCheatButton(PointerEventData evt)
    {
		Debug.Log("OnClickCheatButton");
	}

    private void OnDestroy() // Scene이 파괴될 때 구독 해제
    {
        _orderTextSubscription?.Dispose(); // IDisposable을 사용하여 구독 해제
    }

    private void OnTopViewActivated()
    {
        Debug.Log("<color=green>[UI_GameScene]</color> 탑뷰 활성화 - 테이블 설정 팝업 표시");
        UI_TableSetting popup = Managers.UI.ShowPopupUI<UI_TableSetting>();
        popup.GetComponent<Canvas>().sortingOrder = 101;
        popup.SetInfo();
    }

    private void OnBackViewActivated()
    {
        Debug.Log("<color=yellow>[UI_GameScene]</color> 백뷰 활성화 - 테이블 설정 팝업 숨김");
        // 현재 열려있는 테이블 설정 팝업이 있다면 닫기
        if (Managers.UI.GetPopupCount() > 0)
        {
            Managers.UI.ClosePopupUI(); // 가장 최근 팝업 닫기
        }
    }

    private void OnUpdateOrderText()
    {
        // TableManager에 접근하여 LastOrderSummary 가져오기 (TableManager가 GameManager에 등록되어 있다고 가정)
        if (Managers.Game != null && Managers.Game.CustomerCreator.TableManager != null) 
        {
            string newOrderText = Managers.Game.CustomerCreator.TableManager.LastOrderSummary;
            
            // 텍스트가 변경된 경우에만 업데이트
            if (_lastOrderText != newOrderText)
            {
                _lastOrderText = newOrderText;
                GetText((int)Texts.OrderButtonText).text = newOrderText;
                Debug.Log($"<color=cyan>[UI_GameScene]</color> 주문 텍스트 업데이트됨: {newOrderText}");
            }
        }
        else
        {
            if (_lastOrderText != "주문 정보 없음")
            {
                _lastOrderText = "주문 정보 없음";
                GetText((int)Texts.OrderButtonText).text = _lastOrderText;
                Debug.LogWarning("[UI_GameScene] GameManager 또는 TableManager에 접근할 수 없습니다.");
            }
        }
    }

    private void OnAddCompletedRecipe()
    {
        var sprite = InGameManager.CompletedOrderData.LastCompletedSprite;
        var recipeId = InGameManager.CompletedOrderData.LastCompletedRecipeId;
        var prefabName = InGameManager.CompletedOrderData.LastCompletedPrefabName;
        
        if (sprite == null)
        {
            Debug.LogError("완료된 레시피 스프라이트가 null입니다.");
            return;
        }

        CreateRecipeIcon(sprite, recipeId, prefabName);
    }

    private void CreateRecipeIcon(Sprite sprite, int recipeId, string prefabName)
    {
        var iconObj = new GameObject($"CompletedRecipe_{recipeId}");
        var readyToServeParent = GetObject((int)GameObjects.ReadyToServeItem);
        iconObj.transform.SetParent(readyToServeParent.transform, false);
        
        var image = iconObj.AddComponent<Image>();
        image.sprite = sprite;
        
        SetupRectTransform(iconObj, completedRecipeCount);
        completedRecipeCount++;
        
        // prefab 정보를 활용한 추가 처리 (필요시)
        Debug.Log($"완료된 레시피: {recipeId}, Prefab: {prefabName}");
    }

    private void SetupRectTransform(GameObject iconObj, int index)
    {
        // Anchor와 Pivot 설정 (왼쪽 정렬)
        RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0.5f);
        rectTransform.anchorMax = new Vector2(0, 0.5f);
        rectTransform.pivot = new Vector2(0, 0.5f);
        
        // 크기 및 위치 설정
        rectTransform.sizeDelta = new Vector2(100, 100);
        rectTransform.anchoredPosition = new Vector2(index * 110, 0);
    }

}