using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    PlayerLocomotion playerLocomotion;
    InputManager inputManager;

    [HideInInspector] public Animator animator;

    int horizontal;
    int vertical;

    private void Awake()
    {
        inputManager = GetComponent<InputManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        animator = GetComponentInChildren<Animator>();

        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void PlayTargetAnimation(string TargetAnimation,bool IsInteracting,float transitionDuration)
    {
        animator.SetBool("IsInteracting", IsInteracting);
        animator.CrossFade(TargetAnimation,transitionDuration);
    }

    public void UpdateAnimatorValues(float HorizontalMovement,float VerticalMovement,bool isRunning,bool isJogging,bool isWalking)
    {
        #region snappedAnimation
        //float snappedHorizontal;
        //float snappedVertical;
        //#region SnappedHorizontal
        //    if (HorizontalMovement > 0 && HorizontalMovement < 0.55f)
        //    {
        //        snappedHorizontal = 0.5f;
        //    }
        //    else if(HorizontalMovement > 0.55f)
        //    {
        //        snappedHorizontal = 1f;
        //    }
        //    else if(HorizontalMovement < 0f && HorizontalMovement > -0.55f)
        //    {
        //        snappedHorizontal = -0.5f;
        //    }
        //    else if(HorizontalMovement < -0.55f)
        //    {
        //        snappedHorizontal = -1f;
        //    }
        //    else
        //    {
        //        snappedHorizontal = 0f;
        //    }
        //#endregion
        //#region SnappedVertical
        //    if (VerticalMovement > 0 && VerticalMovement < 0.55f)
        //    {
        //        snappedVertical = 0.5f;
        //    }
        //    else if (VerticalMovement > 0.55f)
        //    {
        //        snappedVertical = 1f;
        //    }
        //    else if (VerticalMovement < 0f && VerticalMovement > -0.55f)
        //    {
        //        snappedVertical = -0.5f;
        //    }
        //    else if (VerticalMovement < -0.55f)
        //    {
        //        snappedVertical = -1f;
        //    }
        //    else
        //    {
        //        snappedVertical = 0f;
        //    }
        #endregion

        if (isRunning)
        {
            HorizontalMovement = 0f;
            VerticalMovement = 2f;
        }
        if(isJogging)
        {
            HorizontalMovement = 0f;
            VerticalMovement = 1f;
        }
        if (isWalking)
        {
            HorizontalMovement = 0f;
            VerticalMovement = 0.5f;
        }

        animator.SetFloat(horizontal, HorizontalMovement, 0.1f, Time.deltaTime);
        animator.SetFloat(vertical,VerticalMovement,0.1f, Time.deltaTime);
    }
}
