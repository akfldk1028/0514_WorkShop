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
    private int[] recipeIdList = { 200001, 200002, 200003, 200004, 200005 }; // 예시: 원하는 id들로 채우세요

    //게임상태확인
    public bool isInteracting = false;
    public bool isRhythmGameStarted = false;

    // private async void Awake()
    // {
    //     if (Instance == null)
    //         Instance = this;
    //     else
    //     {
    //         Destroy(gameObject);
    //         return;
    //     }
    //     SetInfo();

    //     AutoAssign();
    // }



    public Data.RecipeData getRandomRecipe()
    {
        int randomIndex = Random.Range(0, recipeIdList.Length);
        int randomId = recipeIdList[randomIndex];
        Data.RecipeData data = Managers.Data.RecipeDic[randomId];
        return data;
    }


    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space) && isInteracting && !isRhythmGameStarted)
    //     {
    //         isRhythmGameStarted = true;
    //         StartText.SetActive(false);
    //         rhythmGameManager?.StartRhythmSequence();
    //     }

    //     if (Input.GetKeyDown(KeyCode.Escape) && isInteracting)
    //     {
    //         rhythmGameManager?.ForceStopAndFail();
    //         Resume();
    //     }
    // }

    public void Init()
    {
        // InGameScene에서 찾은 오브젝트들 참조
        playerObj = InGameScene.PlayerObj;
        cameraController = InGameScene.CameraController;
        interactionCanvas = InGameScene.InteractionCanvas;
        StartText = InGameScene.StartTextObj;
        rhythmGameManager = InGameScene.RhythmGameManager;
    }

    // F키 눌렀을 때 실행
    // public void InteractWith() //InteractagleObject의 Interact에서 호출
    // {
    //     isInteracting = true;

    //     if (playerObj != null)
    //         playerObj.SetActive(false);

    //     if (playerMove != null)
    //         playerMove.enabled = false;

    //     if (cameraControl != null)
    //         cameraControl.enabled = false;

    //     if (cameraTransform != null)
    //     {
    //         fixedCameraPosition = new Vector3(-6.38f, 4.5f, 9.5f);
    //         fixedCameraRotation = new Vector3(70f, -90f, 0f);

    //         cameraTransform.position = fixedCameraPosition;
    //         cameraTransform.rotation = Quaternion.Euler(fixedCameraRotation);
    //     }

    //     if (interactionCanvas != null)
    //         interactionCanvas.SetActive(true);
    // }


    public void InteractWith()
    {
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


    //ESC
    // public void Resume() //InteractableIObject의 Update문에서 상호작용중+ESC 눌렀을 때 호출
    // {
    //     isInteracting = false;

    //     if (StartText != null)
    //         StartText.SetActive(true);

    //     if (playerObj != null)
    //         playerObj.SetActive(true);

    //     if (playerMove != null)
    //         playerMove.enabled = true;

    //     // if (cameraControl != null)
    //     //     cameraControl.enabled = true;

    //     if (interactionCanvas != null)
    //         interactionCanvas.SetActive(false);
    // }

    public void EndRhythmGame(RhythmResult result)
    {
        switch (result)
        {
            case RhythmResult.Fail:
                Debug.Log("리듬게임 결과: 실패");
                break;
            case RhythmResult.Good:
                Debug.Log("리듬게임 결과: 굿");
                break;
            case RhythmResult.Perfect:
                Debug.Log("리듬게임 결과: 퍼펙트");
                break;
        }

        isRhythmGameStarted = false;

        // 기타 후처리 로직 추가 가능
    }

}

