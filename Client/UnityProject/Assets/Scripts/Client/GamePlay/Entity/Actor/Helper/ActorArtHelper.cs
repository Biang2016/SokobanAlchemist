using UnityEngine;

public class ActorArtHelper : ActorMonoHelper
{
    [SerializeField]
    private Animator ActorArtRootAnim;

    public Animator ActorModelAnim;

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
}