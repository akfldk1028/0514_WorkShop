using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public Transform player;
    public float interactDistance = 2.5f; //상호작용 거리

    private bool canInteract = false;
    private bool interacted = false;

    public GameObject Text;
    public CanvasGroup cg;

    //f키 상호작용 시 탑뷰 고정 및 플레이어 움직임 고정

    private void Awake()
    {
        Managers.Init();
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
            if (Input.GetKeyDown(KeyCode.E))
            {
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
           Managers.Ingame.InteractWith(this);
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
