using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using Unity.Physics;

public class PlayerAnimationManager : MonoBehaviour
{
    public InputActionAsset survivorsInput;

    private InputAction moveAction;

    public Vector2 moveAmt;

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
        
        if(Mathf.Abs(moveAmt.x) >= Mathf.Abs(moveAmt.y) && Mathf.Abs(moveAmt.x) > 0)
        {
            
            //animator.SetBool("RunSide", true);
            if (moveAmt.x > 0)
            {
                CleanAnim();

                animator.SetBool("RunSide", true);
            }
            else if (moveAmt.x <= 0)
            {
                CleanAnim();
                animator.SetBool("RunSideFlip", true);
            }
        }
        else if(Mathf.Abs(moveAmt.y) > Mathf.Abs(moveAmt.x))
        {
            if (moveAmt.y > 0)
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
        else
        {
            CleanAnim();
        }
    }

    private void CleanAnim()
    {
        animator.SetBool("RunUp", false);
        animator.SetBool("RunDown", false);
        animator.SetBool("RunSide", false);
        animator.SetBool("RunSideFlip", false);
        //animator.SetBool("Idle", false);
    }
}
