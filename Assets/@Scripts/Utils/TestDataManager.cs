using UnityEngine;
using System.Collections.Generic;
using Data;

/// <summary>
/// 테스트용 더미 데이터 매니저
/// 플레이어가 모든 레시피를 가지고 있다고 가정하고 테스트할 수 있게 해줍니다.
/// </summary>
public class TestDataManager : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private bool enableTestMode = true;
    [SerializeField] private bool addAllRecipesOnStart = true;
    [SerializeField] private bool showTestUI = true;
    
    [Header("테스트용 레시피 ID들")]
    [SerializeField] private int[] testRecipeIds = { 200001, 200002, 200003 };
    
    private void Start()
    {
        if (!enableTestMode) return;
        
        // 게임 매니저가 준비될 때까지 기다린 후 실행
        Invoke(nameof(InitializeTestData), 1f);
    }
    
    private void InitializeTestData()
    {
        if (!enableTestMode || Managers.Game == null) return;
        
        Debug.Log("<color=magenta>[TestDataManager]</color> 테스트 모드 활성화!");
        
        if (addAllRecipesOnStart)
        {
            AddAllRecipesToPlayer();
        }
        
        // 테스트용 골드 추가
        Managers.Game.AddGold(10000);
        
        Debug.Log("<color=green>[TestDataManager]</color> 테스트 데이터 초기화 완료!");
    }
    
    /// <summary>
    /// 플레이어에게 모든 테스트 레시피를 추가합니다.
    /// </summary>
    public void AddAllRecipesToPlayer()
    {
        if (Managers.Data?.RecipeDic == null)
        {
            Debug.LogError("[TestDataManager] RecipeDic이 null입니다!");
            return;
        }
        
        int addedCount = 0;
        
        // 모든 레시피 데이터를 순회하면서 플레이어 인벤토리에 추가
        foreach (var recipeKvp in Managers.Data.RecipeDic)
        {
            var recipe = recipeKvp.Value;
            
            // 플레이어 인벤토리에 추가
            Managers.Game.AddRecipeToInventory(recipe.NO, recipe.RecipeName, recipe.Prefab);
            addedCount++;
        }
        
        Debug.Log($"<color=cyan>[TestDataManager]</color> 총 {addedCount}개의 레시피를 플레이어 인벤토리에 추가했습니다.");
        
        // UI에도 아이콘 표시 (테스트용)
        if (showTestUI)
        {
            ShowAllRecipeIconsInUI();
        }
    }
    
    /// <summary>
    /// 특정 레시피들만 플레이어에게 추가합니다.
    /// </summary>
    public void AddSpecificRecipesToPlayer()
    {
        if (Managers.Data?.RecipeDic == null) return;
        
        foreach (int recipeId in testRecipeIds)
        {
            if (Managers.Data.RecipeDic.TryGetValue(recipeId, out var recipe))
            {
                Managers.Game.AddRecipeToInventory(recipe.NO, recipe.RecipeName, recipe.Prefab);
                Debug.Log($"<color=cyan>[TestDataManager]</color> 레시피 추가: {recipe.RecipeName} (ID: {recipeId})");
            }
        }
    }
    
    /// <summary>
    /// UI에 모든 레시피 아이콘을 표시합니다 (테스트용)
    /// </summary>
    private void ShowAllRecipeIconsInUI()
    {
        if (Managers.Data?.RecipeDic == null) return;
        
        foreach (var recipeKvp in Managers.Data.RecipeDic)
        {
            var recipe = recipeKvp.Value;
            
            // CompletedOrderData 설정
            var iconSprite = Managers.Resource.Load<Sprite>(recipe.IconImage);
            if (iconSprite != null)
            {
                InGameManager.CompletedOrderData.LastCompletedSprite = iconSprite;
                InGameManager.CompletedOrderData.LastCompletedRecipeId = recipe.NO;
                InGameManager.CompletedOrderData.LastCompletedPrefabName = recipe.Prefab;
                
                // UI 업데이트 액션 발행
                Managers.PublishAction(ActionType.GameScene_AddCompletedRecipe);
            }
        }
    }
    
    /// <summary>
    /// 플레이어 인벤토리를 모두 비웁니다.
    /// </summary>
    public void ClearPlayerInventory()
    {
        var inventory = Managers.Game.PlayerInventory;
        var recipesToRemove = new List<int>();
        
        foreach (var recipe in inventory)
        {
            recipesToRemove.Add(recipe.recipeId);
        }
        
        foreach (int recipeId in recipesToRemove)
        {
            Managers.Game.RemoveRecipeFromInventory(recipeId);
        }
        
        // UI도 완전히 리셋 (모든 아이콘 제거)
        ClearAllUIIcons();
        
        Debug.Log("<color=orange>[TestDataManager]</color> 플레이어 인벤토리와 UI를 모두 비웠습니다.");
    }
    
    /// <summary>
    /// UI에서 모든 레시피 아이콘을 제거합니다.
    /// </summary>
    private void ClearAllUIIcons()
    {
        // ReadyToServeItem GameObject를 직접 찾아서 자식들 제거
        var readyToServeParent = GameObject.Find("ReadyToServeItem");
        if (readyToServeParent != null)
        {
            // 모든 자식 오브젝트 삭제
            for (int i = readyToServeParent.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(readyToServeParent.transform.GetChild(i).gameObject);
            }
            
            Debug.Log("<color=cyan>[TestDataManager]</color> UI 아이콘들을 모두 제거했습니다.");
        }
        else
        {
            Debug.LogWarning("<color=yellow>[TestDataManager]</color> ReadyToServeItem을 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// 현재 플레이어 인벤토리 상태를 출력합니다.
    /// </summary>
    public void PrintPlayerInventory()
    {
        var inventory = Managers.Game.PlayerInventory;
        Debug.Log($"<color=yellow>[TestDataManager]</color> 플레이어 인벤토리 ({inventory.Count}개):");
        
        foreach (var recipe in inventory)
        {
            Debug.Log($"  - {recipe.recipeName} (ID: {recipe.recipeId}, Prefab: {recipe.prefabName})");
        }
    }
    
    // 테스트용 키보드 단축키
    private void Update()
    {
        if (!enableTestMode) return;
        
        // F1: 모든 레시피 추가 (리셋 후)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ClearPlayerInventory(); // 먼저 인벤토리 비우기
            AddAllRecipesToPlayer(); // 그 다음 모든 레시피 추가
        }
        
        // F2: 특정 레시피들만 추가
        if (Input.GetKeyDown(KeyCode.F2))
        {
            AddSpecificRecipesToPlayer();
        }
        
        // F3: 인벤토리 비우기
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ClearPlayerInventory();
        }
        
        // F4: 인벤토리 출력
        if (Input.GetKeyDown(KeyCode.F4))
        {
            PrintPlayerInventory();
        }
    }
    
    private void OnGUI()
    {
        if (!enableTestMode || !showTestUI) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("=== 테스트 모드 ===");
        GUILayout.Label("F1: 모든 레시피 추가 (리셋 후)");
        GUILayout.Label("F2: 특정 레시피 추가");
        GUILayout.Label("F3: 인벤토리 비우기");
        GUILayout.Label("F4: 인벤토리 출력");
        GUILayout.Label($"현재 인벤토리: {Managers.Game?.PlayerInventory?.Count ?? 0}개");
        GUILayout.EndArea();
    }
} 