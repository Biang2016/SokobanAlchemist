using UnityEngine;

public class ActorArtHelper : ActorMonoHelper
{
    [SerializeField]
    private Animator ActorArtRootAnim;

    [SerializeField]
    private Animator ActorModelAnim;

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
        Actor.SetModelSmoothMoveLerpTime(0.02f);
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

    public void SetIsWalking(bool isWalking)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetBool("IsWalking", isWalking);
        }
    }

    public void SetIsPushing(bool isPushing)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetBool("IsPushing", isPushing);
        }
    }

    public void Dash()
    {
        if (ActorArtRootAnim != null)
        {
            ActorArtRootAnim.SetTrigger("Dash");
        }
    }
}