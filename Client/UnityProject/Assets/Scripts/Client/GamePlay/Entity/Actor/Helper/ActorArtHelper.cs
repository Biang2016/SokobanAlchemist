using UnityEngine;

public class ActorArtHelper : ActorMonoHelper
{
    [SerializeField]
    private Animator ActorArtRootAnim;

    [SerializeField]
    private Animator ActorModelAnim;

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        if (ActorModelAnim != null)
        {
            foreach (AnimatorControllerParameter parameter in ActorModelAnim.parameters)
            {
                if (parameter.type == AnimatorControllerParameterType.Bool)
                    ActorModelAnim.SetBool(parameter.name, false);
                if (parameter.type == AnimatorControllerParameterType.Float)
                    ActorModelAnim.SetFloat(parameter.name, 0);
                if (parameter.type == AnimatorControllerParameterType.Int)
                    ActorModelAnim.SetInteger(parameter.name, 0);
                if (parameter.type == AnimatorControllerParameterType.Trigger)
                    ActorModelAnim.ResetTrigger(parameter.name);
            }
        }
    }

    public void Vault()
    {
        if (ActorArtRootAnim != null)
        {
            ActorArtRootAnim.SetTrigger("Vault");
            Actor.SetModelSmoothMoveLerpTime(0f);
        }
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SwapBox()
    {
        Actor.SwapBox();
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void VaultEnd()
    {
        Actor.SetModelSmoothMoveLerpTime(Actor.DefaultSmoothMoveLerpTime);
    }

    public void Kick()
    {
        if (ActorArtRootAnim != null)
        {
            ActorArtRootAnim.SetTrigger("Kick");
        }
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void KickBox()
    {
        Actor.KickBox();
    }

    public void Dash()
    {
        if (ActorArtRootAnim != null)
        {
            ActorArtRootAnim.SetTrigger("Dash");
        }
    }

    #region 第一优先级

    public void SetIsFrozen(bool isFrozen)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetBool("IsFrozen", isFrozen);
        }
    }

    #endregion

    #region 第二优先级

    public void SetIsAttacking()
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetTrigger("Action");
            ActorModelAnim.SetTrigger("Attack");
        }
    }

    public void SetIsPushing(bool isPushing)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetBool("IsPushing", isPushing);
        }
    }

    #endregion

    #region 第三优先级

    public void SetIsChasing(bool isChasing)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetBool("IsChasing", isChasing);
        }
    }

    public void SetIsWalking(bool isWalking)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetBool("IsWalking", isWalking);
        }
    }

    public void SetIsEscaping(bool isEscaping)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetBool("IsEscaping", isEscaping);
        }
    }

    #endregion
}