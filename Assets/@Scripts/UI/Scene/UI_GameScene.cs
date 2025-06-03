using UnityEngine;
using UnityEngine.EventSystems;
using System; // IDisposable 사용을 위해 추가

public class UI_GameScene : UI_Scene
{
    enum Buttons
    {
        DiaPlusButton,
        SettingButton,
        QuestButton,
        ChallengeButton,
       
        CheatButton,
        shopButton,

        
    }

    enum Texts
    {
        LevelText,
        BattlePowerText,
        GoldCountText,
        DiaCountText,
        MeatCountText,
        WoodCountText,
        MineralCountText,
        OrderButtonText,
    }

    enum Sliders
    {
        MeatSlider,
        WoodSlider,
        MineralSlider,
    }

    private IDisposable _orderTextSubscription;
    private string _lastOrderText = ""; // 마지막 주문 텍스트 캐시

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        BindButtons(typeof(Buttons));
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));

        // GetButton((int)Buttons.GoldPlusButton).gameObject.BindEvent(OnClickGoldPlusButton);
        // GetButton((int)Buttons.DiaPlusButton).gameObject.BindEvent(OnClickDiaPlusButton);
        GetButton((int)Buttons.SettingButton).gameObject.BindEvent(OnClickSettingButton);
        // GetButton((int)Buttons.InventoryButton).gameObject.BindEvent(OnClickInventoryButton);
        GetButton((int)Buttons.QuestButton).gameObject.BindEvent(OnClickQuestButton);
        GetButton((int)Buttons.ChallengeButton).gameObject.BindEvent(OnClickChallengeButton);
        GetButton((int)Buttons.CheatButton).gameObject.BindEvent(OnClickCheatButton);
        GetButton((int)Buttons.shopButton).gameObject.BindEvent(OnClickShopButton);
        
        // 주문 텍스트 업데이트 액션 구독
        _orderTextSubscription = Managers.Subscribe(ActionType.GameScene_UpdateOrderText, OnUpdateOrderText);
        
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
            GetText((int)Texts.GoldCountText).text = text;

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

}