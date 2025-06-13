using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public Transform player;
    public float interactDistance = 2.5f; //��ȣ�ۿ� �Ÿ�

    private bool canInteract = false;
    private bool interacted = false;

    public GameObject Text;
    public CanvasGroup cg;

    //fŰ ��ȣ�ۿ� �� ž�� ���� �� �÷��̾� ������ ����

    private void Awake()
    {
        // Managers.Init();
        player = Managers.Game.Player.transform;
    }


    private void Start()
    {
        cg = Text.GetComponent<CanvasGroup>();
        ShowText();
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        canInteract = distance < interactDistance;

        if (canInteract && !interacted)
        {
            ShowText();
            if (Input.GetKeyDown(KeyCode.F)) //플레이 시 딱 한번만 정상실행되고 그 다음에는 안에 있는 코드가 실행이 안되는데 왜인지 모르겠음
            {
                Debug.Log("getkeydown FFFFFFFF");
                interacted = true;
                Managers.Ingame.InteractWith();
                HideText();
            }
        }
        else if (!canInteract)
        {
            interacted = false;
            HideText();
        }

        if (interacted && Input.GetKeyDown(KeyCode.Escape))
        {
            //Managers.Ingame.Resume();
            interacted = false;
        }

    }

    void Interact()
    {
        if (interacted)
        {

        }
    }

    void ShowText()
    {
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    void HideText()
    {
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
