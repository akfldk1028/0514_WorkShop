using UnityEngine;
using UnityEngine.EventSystems;

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
    }

    enum Sliders
    {
        MeatSlider,
        WoodSlider,
        MineralSlider,
    }

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

}