using UnityEngine;
using System.Threading.Tasks;

public class InGameManager
{
    public static InGameManager Instance;

    [Header("Player Control")]
    public GameObject playerObj;
    public PlayerMove playerMove;

    [Header("Camera Control")]
    public CameraController cameraController;

    [Header("Interaction UI")]
    public GameObject interactionCanvas;
    public GameObject StartText;


    [Header("         ")]
    public RhythmGameManager rhythmGameManager;
    private InteractableObject _currentInteractable; // 현재 상호작용 중인 오브젝트를 기억

    private int[] recipeIdList = { 200001, 200003, 200006, 200009, 200010, 200011, 200012, 200015, 200017, 200019 }; // 예시: 원하는 id들로 채우세요

    //게임상태확인
    public bool isInteracting = false;
    public bool isRhythmGameStarted = false;

  

    public Data.RecipeData getRandomRecipe()
    {
        int randomIndex = Random.Range(0, recipeIdList.Length);
        int randomId = recipeIdList[randomIndex];
        Data.RecipeData data = Managers.Data.RecipeDic[randomId];
        return data;
    }


 
    public void Init()
    {
        // InGameScene에서 찾은 오브젝트들 참조
        playerObj = InGameScene.PlayerObj;
        cameraController = InGameScene.CameraController;
        interactionCanvas = InGameScene.InteractionCanvas;
        StartText = InGameScene.StartTextObj;
        rhythmGameManager = InGameScene.RhythmGameManager;
    }




    public void InteractWith()
    {
        Debug.Log("InteractWith");
        isInteracting = true;
        
        if (playerObj != null)
            playerObj.SetActive(false);
            
        if (playerMove != null)
            playerMove.enabled = false;
            
        // 기존 방식 제거
        // if (cameraControl != null)
        //     cameraControl.enabled = false;
        
        // 새로운 방식
        if (cameraController != null)
            cameraController.SetFixedView(true);
            
        if (interactionCanvas != null)
            interactionCanvas.SetActive(true);
            Debug.Log("InteractWithFinish");
    }


    public void Resume()
{
    isInteracting = false;
    
    if (StartText != null)
        StartText.SetActive(true);
        
    if (playerObj != null)
        playerObj.SetActive(true);
        
    if (playerMove != null)
        playerMove.enabled = true;
        
    // 기존 방식 제거
    // if (cameraControl != null)
    //     cameraControl.enabled = true;
    
    // 새로운 방식
    if (cameraController != null)
        cameraController.SetFixedView(false);
        
    if (interactionCanvas != null)
        interactionCanvas.SetActive(false);
}



    public void EndRhythmGame(RhythmResult result)
    {
        switch (result)
        {
            case RhythmResult.Fail:
                Debug.Log("리듬게임 결과: 실패");
                ShowCompletedRecipeIcon();
                break;
            case RhythmResult.Good:
                Debug.Log("리듬게임 결과: 굿");
                ShowCompletedRecipeIcon();
                break;
            case RhythmResult.Perfect:
                Debug.Log("리듬게임 결과: 퍼펙트");
                ShowCompletedRecipeIcon();
                break;
            case RhythmResult.Pause:
                Debug.Log("리듬게임 결과: 일시정지/실패패");
                break;
        }

        //isRhythmGameStarted = false;

        
    }

    private void ShowCompletedRecipeIcon()
    {
        if (rhythmGameManager?.CurrentRecipe == null) return;

        var recipe = rhythmGameManager.CurrentRecipe;
        var iconSprite = Managers.Resource.Load<Sprite>(recipe.IconImage);
        
        if (iconSprite == null)
        {
            Debug.LogError($"<color=red>[InGameManager]</color> 스프라이트 로드 실패: {recipe.IconImage}");
            return;
        }

        // 완료된 주문 데이터 설정 및 액션 발행
        CompletedOrderData.LastCompletedSprite = iconSprite;
        CompletedOrderData.LastCompletedRecipeId = recipe.NO;
        CompletedOrderData.LastCompletedPrefabName = recipe.Prefab;
        
        // GameManager 인벤토리에도 추가
        Managers.Game.AddRecipeToInventory(recipe.NO, recipe.RecipeName, recipe.Prefab);
        
        Managers.PublishAction(ActionType.GameScene_AddCompletedRecipe);
    }

    // 완료된 주문 데이터를 전달하기 위한 정적 클래스
    public static class CompletedOrderData
    {
        public static Sprite LastCompletedSprite;
        public static int LastCompletedRecipeId;
        public static string LastCompletedPrefabName;
    }
}

