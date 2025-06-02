using UnityEngine;

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

    [Header("리듬 게임")]
    public RhythmGameManager rhythmGameManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        AutoAssign();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartText.SetActive(false);
            if (rhythmGameManager != null)
                rhythmGameManager.StartRhythmSequence();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (rhythmGameManager != null)
                rhythmGameManager.ForceStopAndFail();

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
        }

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

    public void InteractWith(InteractableObject obj)
    {
        if (playerObj != null)
            playerObj.SetActive(false);

        if (playerMove != null)
            playerMove.enabled = false;

        if (cameraControl != null)
            cameraControl.enabled = false;

        if (cameraTransform != null)
        {
            fixedCameraPosition = new Vector3(-6.38f, 4.5f, 9.55f);
            fixedCameraRotation = new Vector3(70f, -90f, 0f);

            cameraTransform.position = fixedCameraPosition;
            cameraTransform.rotation = Quaternion.Euler(fixedCameraRotation);
        }

        if (interactionCanvas != null)
            interactionCanvas.SetActive(true);
    }

    public void Resume()
    {
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

    public void OnRhythmResult(bool isSuccess)
    {
        Debug.Log("리듬 게임 결과: " + (isSuccess ? "성공" : "실패"));

        if (isSuccess)
        {
            // 성공 시 처리
        }
        else
        {
            // 실패 시 처리
        }
    }
}
