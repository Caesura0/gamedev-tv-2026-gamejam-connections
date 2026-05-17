using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] private InputActionReference moveAction;

    private Animator animator; 
    private Vector2 moveInput;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        if (input.x != 0)
        {
            moveInput = new Vector2(Mathf.Sign(input.x), 0);
        }
       
        else if (input.y != 0)
        {
            moveInput = new Vector2(0, Mathf.Sign(input.y));
        }
        else
        {
            moveInput = Vector2.zero;
        }

        bool IsWalking = moveInput != Vector2.zero;

        animator.SetBool("IsWalking",IsWalking);

        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);

    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
    }
}
