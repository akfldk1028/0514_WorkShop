using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float rotationSpeed = 150f;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float turn = 0f;
        bool IsWalking = false;

        if (Input.GetKey(KeyCode.A))
            turn = -1f;
        else if (Input.GetKey(KeyCode.D))
            turn = 1f;

        transform.Rotate(0f, turn * rotationSpeed * Time.deltaTime, 0f);



        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            IsWalking = true;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-Vector3.forward * moveSpeed * Time.deltaTime);
            IsWalking = true;
        }

        animator.SetBool("isWalking", IsWalking);
    }
}
