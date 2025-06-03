using UnityEngine;
using System.Threading.Tasks;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;

    [Header("Player Control")]
    public GameObject playerObj;
    public PlayerMove playerMove;

    [Header("Camera Control")]
    public CameraControl cameraControl;

    public Transform cameraTransform;
    public Vector3 fixedCameraPosition;
    public Vector3 fixedCameraRotation;

    [Header("Interaction UI")]
    public GameObject interactionCanvas;
    public GameObject StartText;


    [Header("���� ����")]
    public RhythmGameManager rhythmGameManager;
    private int[] recipeIdList = { 200001, 200002, 200003, 200004, 200005 }; // 예시: 원하는 id들로 채우세요

    //게임상태확인
    private bool isInteracting = false;
    private bool isRhythmGameStarted = false;

    private async void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        await Managers.Data.StartLoadAssetsAsync();
        SetInfo();

        AutoAssign();
    }

    // private Task StartLoadAssetsAsync()
    // {
    //     var tcs = new TaskCompletionSource<bool>();
    //     Managers.Resource.LoadAllAsync<Object>("PreLoad", (key, count, totalCount) =>
    //     {
    //         Debug.Log($"<color=cyan>[UI_StartUpScene]</color> {key} {count}/{totalCount}");

    //         if (count == totalCount)
    //         {
    //             Managers.Data.Init();
    //             tcs.SetResult(true);
    //         }
    //     });
    //     return tcs.Task;
    // }


    private void SetInfo() //게임 시작 전 필요한 데이터 준비
    {
        
    }

    public Data.RecipeData getRandomRecipe()
    {
        int randomIndex = Random.Range(0, recipeIdList.Length);
        int randomId = recipeIdList[randomIndex];
        Data.RecipeData data = Managers.Data.RecipeDic[randomId];
        return data;
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isInteracting && !isRhythmGameStarted)
        {
            isRhythmGameStarted = true;
            StartText.SetActive(false);
            rhythmGameManager?.StartRhythmSequence();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isInteracting)
        {
            rhythmGameManager?.ForceStopAndFail();
            Resume();
        }
    }

    private void AutoAssign()
    {
        if (playerObj == null)
            playerObj = GameObject.FindWithTag("Player");

        if (playerMove == null && playerObj != null)
            playerMove = playerObj.GetComponent<PlayerMove>();

        if (cameraControl == null)
            cameraControl = FindObjectOfType<CameraControl>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (interactionCanvas == null)
        {
            interactionCanvas = GameObject.Find("GameCanvas");
            if (interactionCanvas != null)
                interactionCanvas.SetActive(false);

            // if (interactionCanvas == null)
            // {
            //     interactionCanvas = GameObject.Find("GameCanvas");
            //     interactionCanvas.SetActive(false);
            // }

            if (StartText == null && interactionCanvas != null)
            {
                Transform found = interactionCanvas.transform.Find("StartText");
                if (found != null)
                    StartText = found.gameObject;
            }

            if (rhythmGameManager == null)
            {
                GameObject obj = GameObject.Find("RhythmManager");
                if (obj != null)
                    rhythmGameManager = obj.GetComponent<RhythmGameManager>();
            }
        }

    }

    // F키 눌렀을 때 실행
    public void InteractWith() //InteractagleObject의 Interact에서 호출
    {
        isInteracting = true;

        if (playerObj != null)
            playerObj.SetActive(false);

        if (playerMove != null)
            playerMove.enabled = false;

        if (cameraControl != null)
            cameraControl.enabled = false;

        if (cameraTransform != null)
        {
            fixedCameraPosition = new Vector3(-6.38f, 4.5f, 9.5f);
            fixedCameraRotation = new Vector3(70f, -90f, 0f);

            cameraTransform.position = fixedCameraPosition;
            cameraTransform.rotation = Quaternion.Euler(fixedCameraRotation);
        }

        if (interactionCanvas != null)
            interactionCanvas.SetActive(true);
    }

    //ESC
    public void Resume() //InteractableIObject의 Update문에서 상호작용중+ESC 눌렀을 때 호출
    {
        isInteracting = false;

        if (StartText != null)
            StartText.SetActive(true);

        if (playerObj != null)
            playerObj.SetActive(true);

        if (playerMove != null)
            playerMove.enabled = true;

        if (cameraControl != null)
            cameraControl.enabled = true;

        if (interactionCanvas != null)
            interactionCanvas.SetActive(false);
    }

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

