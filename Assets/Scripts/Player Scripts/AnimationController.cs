using UnityEngine;
using UnityEngine.InputSystem;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    public InputActionReference moveAction;

    private Vector2 movementInput;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (moveAction != null)
        {
            moveAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.action.Disable();
        }
    }

    void Update()
    {
        if (moveAction != null)
        {
            movementInput = moveAction.action.ReadValue<Vector2>();
        }
        if (movementInput != Vector2.zero)
        {
            animator.SetFloat("MoveX", movementInput.x);
            animator.SetFloat("MoveY", movementInput.y);

            animator.SetFloat("Speed", movementInput.sqrMagnitude);
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }
}
