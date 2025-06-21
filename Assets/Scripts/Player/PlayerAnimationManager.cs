using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationManager : MonoBehaviour
{
    public InputActionAsset survivorsInput;

    private InputAction moveAction;

    [SerializeField] private Vector2 moveAmt;

    [SerializeField] private Animator animator;

    private void OnEnable()
    {
        survivorsInput.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        survivorsInput.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        moveAmt = moveAction.ReadValue<Vector2>();
        if (moveAmt.x > 0)
        {
            animator.SetFloat("Run", 1);
        }
        else if (moveAmt.x <= 0)
        {
            animator.SetFloat("Run", 0);

        }
    }
}
