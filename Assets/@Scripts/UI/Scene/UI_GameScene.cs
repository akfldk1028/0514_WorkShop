using UnityEngine;
using UnityEngine.EventSystems;
using System; // IDisposable 사용을 위해 추가
using UnityEngine.UI;
using DG.Tweening; // DoTween 사용을 위해 추가

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
        BattlePowerText,
        OrderButtonText,
        RecipeButtonText,
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
    private IDisposable _goldAnimationSubscription; // 골드 애니메이션 구독 추가
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

        GetButton((int)Buttons.shopButton).gameObject.BindEvent(OnClickShopButton);
        GetText((int)Texts.GoldCountText).text = "0";
        GetText((int)Texts.BattlePowerText).text = $"{Managers.Game.Glass}개";
        // 주문 텍스트 업데이트 액션 구독
        _orderTextSubscription = Managers.Subscribe(ActionType.GameScene_UpdateOrderText, OnUpdateOrderText);
        // 골드 애니메이션 이벤트 구독 추가
        _goldAnimationSubscription = Managers.Subscribe(ActionType.UI_AnimateGoldIncrease, OnAnimateGoldIncrease);
        // 완료된 레시피 아이콘 추가 액션 구독
        Managers.Subscribe(ActionType.GameScene_AddCompletedRecipe, OnAddCompletedRecipe);
        // 완료된 레시피 아이콘 제거 액션 구독 추가
        Managers.Subscribe(ActionType.GameScene_RemoveCompletedRecipe, OnRemoveCompletedRecipe);
        // 카메라 뷰 전환 액션 구독
        Managers.Subscribe(ActionType.Camera_TopViewActivated, OnTopViewActivated);
        Managers.Subscribe(ActionType.Camera_BackViewActivated, OnBackViewActivated);
        
        // UI 업데이트 액션 구독
        Managers.Subscribe(ActionType.UI_UpdateRecipeText, OnUpdateRecipeText);
        Managers.Subscribe(ActionType.UI_UpdateOrderText, OnUpdateOrderTextFromRhythm);
        Managers.Subscribe(ActionType.UI_UpdateGlassText, OnUpdateGlassText);
        
        Refresh();
        
        return true;
    }

    private float _elapsedTime = 0.0f;
    private float _updateInterval = 1.0f;

    private void Update()
    {
        // _elapsedTime += Time.deltaTime;

        // if (_elapsedTime >= _updateInterval)
        // {
        //     float fps = 1.0f / Time.deltaTime;
        //     float ms = Time.deltaTime * 1000.0f;
        //     string text = string.Format("{0:N1} FPS ({1:N1}ms)", fps, ms);
        //     // GetText((int)Texts.GoldCountText).text = text;

        //     _elapsedTime = 0;
        // }
    }
    
    public void SetInfo()
    {
        Refresh();
    }

    void Refresh()
    {
        if (_init == false)
            return;
            
        RefreshGlassText();
        RefreshGoldText();
    }

    void OnClickShopButton(PointerEventData evt)
    {
        // blub 사운드 재생
        Managers.Sound.Play(Define.ESound.Effect, "blub");
        
        Debug.Log("<color=magenta>[UI_GameScene]</color> OnClickShopButton");
        UI_TableSetting popup = Managers.UI.ShowPopupUI<UI_TableSetting>();
        popup.GetComponent<Canvas>().sortingOrder = 101;
        popup.SetInfo();
    }

    public void RefreshGoldText()
    {
        // 애니메이션 없이 즉시 업데이트하는 경우에만 사용
        GetText((int)Texts.GoldCountText).text = Managers.Game.Gold.ToString();
    }

    public void RefreshGlassText()
    {
        GetText((int)Texts.BattlePowerText).text = $"{Managers.Game.Glass}개";
    }

    private void OnDestroy() // Scene이 파괴될 때 구독 해제
    {
        _orderTextSubscription?.Dispose(); // IDisposable을 사용하여 구독 해제
        _goldAnimationSubscription?.Dispose(); // 골드 애니메이션 구독 해제 추가
    }

    /// <summary>
    /// 골드 증가 애니메이션 처리 - 화끈한 버전! 🔥💰
    /// </summary>
    private void OnAnimateGoldIncrease()
    {
        int currentGold = Managers.Game.Gold;
        TMPro.TMP_Text goldText = GetText((int)Texts.GoldCountText);
        
        // 이전 골드 값을 파싱 (실패하면 0으로 기본값)
        int.TryParse(goldText.text.Replace(",", ""), out int oldGold);
        
        // UIAnimationController 사용으로 간단하게!
        UIAnimationController.AnimateGoldIncrease(goldText, oldGold, currentGold);
        
        Debug.Log($"<color=gold>🔥💰 [UI_GameScene] 화끈한 골드 애니메이션 실행!</color> {oldGold:N0} → {currentGold:N0}");
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

    private void OnRemoveCompletedRecipe()
    {
        // CompletedOrderData에서 제거할 레시피 ID 가져오기
        int recipeIdToRemove = InGameManager.CompletedOrderData.LastCompletedRecipeId;
        
        var readyToServeParent = GetObject((int)GameObjects.ReadyToServeItem);
        if (readyToServeParent == null)
        {
            Debug.LogError("[UI_GameScene] ReadyToServeItem을 찾을 수 없습니다!");
            return;
        }
        
        // 해당 레시피 ID의 아이콘 찾아서 제거
        Transform targetIcon = null;
        for (int i = 0; i < readyToServeParent.transform.childCount; i++)
        {
            var child = readyToServeParent.transform.GetChild(i);
            if (child.name == $"CompletedRecipe_{recipeIdToRemove}")
            {
                targetIcon = child;
                break;
            }
        }
        
        if (targetIcon != null)
        {
            // 아이콘 제거
            Destroy(targetIcon.gameObject);
            completedRecipeCount--;
            
            // 남은 아이콘들의 위치 재정렬
            ReorganizeIcons(readyToServeParent);
            
            Debug.Log($"<color=orange>[UI_GameScene]</color> 레시피 아이콘 제거됨: {recipeIdToRemove}");
        }
        else
        {
            Debug.LogWarning($"[UI_GameScene] 제거할 레시피 아이콘을 찾을 수 없습니다: {recipeIdToRemove}");
        }
    }
    
    private void ReorganizeIcons(GameObject parent)
    {
        // 남은 아이콘들을 왼쪽부터 다시 정렬
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            var child = parent.transform.GetChild(i);
            var rectTransform = child.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(i * 110, 0);
            }
        }
    }

    private void OnUpdateRecipeText()
    {
        // RhythmGameManager에서 현재 레시피 정보 가져와서 텍스트 업데이트
        var rhythmManager = Managers.Ingame.rhythmGameManager;
        string recipeDisplayText = "";
        
        if (rhythmManager != null && rhythmManager.CurrentRecipe != null)
        {
            recipeDisplayText = $"🔥 제작 중: {rhythmManager.CurrentRecipe.RecipeName}";
        }
        else
        {
            recipeDisplayText = "🔥 제작 중: 없음";
        }
        
        TMPro.TMP_Text recipeText = GetText((int)Texts.RecipeButtonText);
        
        // 기존 방식도 유지 + 애니메이션 추가! 🔥👨‍🍳
        recipeText.text = recipeDisplayText;  // 기존 방식
        UIAnimationController.AnimateRecipeUpdate(recipeText, recipeDisplayText); // 애니메이션 추가
    }

    private void OnUpdateOrderTextFromRhythm()
    {
        // OrderManager에서 주문 정보 가져와서 텍스트 업데이트
        var allOrders = Managers.Game.CustomerCreator.OrderManager.GetAllOrders();
        string orderDisplayText = "";
        if (allOrders.Count > 0)
        {
            orderDisplayText = $"📋 대기 주문 ({allOrders.Count}개):\n";
            for (int i = 0; i < allOrders.Count; i++)
            {
                orderDisplayText += $"{i + 1}. {allOrders[i].RecipeName} x{allOrders[i].Quantity}\n";
            }
        }
        else
        {
            orderDisplayText = "📋 대기 주문: 없음";
        }
        
        TMPro.TMP_Text orderText = GetText((int)Texts.OrderButtonText);
        string finalText = orderDisplayText.TrimEnd('\n');
        
        // 기존 방식도 유지 + 애니메이션 추가! 📋⚡
        orderText.text = finalText;  // 기존 방식
        UIAnimationController.AnimateOrderUpdate(orderText, finalText); // 애니메이션 추가
    }

    private void OnUpdateGlassText()
    {
        int currentGlass = Managers.Game.Glass;
        TMPro.TMP_Text glassText = GetText((int)Texts.BattlePowerText);
        
        // 이전 유리잔 개수 파싱
        string currentText = glassText.text.Replace("개", "");
        int.TryParse(currentText, out int oldGlass);
        
        // 기존 방식도 유지 + 애니메이션 추가! 🥃✨
        RefreshGlassText();  // 기존 방식
        UIAnimationController.AnimateGlassUpdate(glassText, oldGlass, currentGlass); // 애니메이션 추가
        
        Debug.Log($"<color=cyan>[UI_GameScene]</color> 유리잔 텍스트 업데이트: {Managers.Game.Glass}개");
    }

}