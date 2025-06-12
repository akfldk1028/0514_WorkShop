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
            if (Input.GetKeyDown(KeyCode.F))
            {
                Managers.UI.HideSceneUI();
                interacted = true;
                Interact();
            }
        }
        else if (!canInteract)
        {
            HideText();
        }

        if (interacted && Input.GetKeyDown(KeyCode.Escape))
        {
            Managers.Ingame.Resume();
            interacted = false;
        }

    }

    void Interact()
    {
        if (interacted)
        {
           Managers.Ingame.InteractWith();
           HideText();
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
