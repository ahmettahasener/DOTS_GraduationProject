using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Unity.Physics;

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
        if(Mathf.Abs(moveAmt.x) >= Mathf.Abs(moveAmt.y))
        {
            CleanAnim();
            animator.SetBool("RunSide", true);
            //if (moveAmt.x > 0)
            //{
            //    animator.SetBool("RunSide",true);
            //}
            //else if (moveAmt.x <= 0)
            //{
            //    animator.SetBool("RunSide", true);
            //}
        }
        else //y bigger than x
        {
            if (moveAmt.y >= 0)
            {
                CleanAnim();

                animator.SetBool("RunUp", true);
            }
            else if (moveAmt.y < 0)
            {
                CleanAnim();

                animator.SetBool("RunDown", true);
            }
        }
    }

    private void CleanAnim()
    {
        animator.SetBool("RunUp", false);
        animator.SetBool("RunDown", false);
        animator.SetBool("RunSide", false);
        animator.SetBool("Idle", false);
    }
}
