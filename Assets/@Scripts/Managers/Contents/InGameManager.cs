using UnityEngine;

public class InGameManager : MonoBehaviour
{
    public static InGameManager Instance;

    [Header("Player Control")]
    public GameObject playerObj;
    public PlayerMove playerMove;

    [Header("Camera Control")]
    public CameraControl cameraControl; //���� ����� �ȷο� ī�޶� ����
    public Transform cameraTransform;
    public Vector3 fixedCameraPosition;
    public Vector3 fixedCameraRotation;

    [Header("Interaction UI")]
    public GameObject interactionCanvas;

    //find�� �����ؾ��ϳ�?? ������� �۾��ؾ�����?-?

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

        // if (interactionCanvas == null)
        // {
        //     interactionCanvas = GameObject.Find("GameCanvas");
        //     interactionCanvas.SetActive(false);
        // }

    }


    //FŰ ��ȣ�ۿ� ������Ʈ���� ȣ�� - ���� ����, �÷��̾� ����, UI ǥ��
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

    //ESC �� ���� (������������ ���ư�)
    public void Resume()
    {
        if (playerObj != null)
            playerObj.SetActive(true);

        if (playerMove != null)
            playerMove.enabled = true;

        if (cameraControl != null)
            cameraControl.enabled = true;

        if (interactionCanvas != null)
            interactionCanvas.SetActive(false);
    }
}