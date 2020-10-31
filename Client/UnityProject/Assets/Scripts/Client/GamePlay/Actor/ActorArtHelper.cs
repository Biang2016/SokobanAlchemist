using UnityEngine;

public class ActorArtHelper : ActorMonoHelper
{
    [SerializeField]
    private Animator ActorArtAnimator;

    public void Vault()
    {
        if (ActorArtAnimator != null)
        {
            ActorArtAnimator.SetTrigger("Vault");
        }
    }

    public void SwapBox()
    {
        Actor.SwapBox();
    }
}
