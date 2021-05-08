using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class ActorArtAnim : MonoBehaviour
{
    public ActorArtHelper ActorArtHelper;

    void Start()
    {
        ActorArtHelper = GetComponentInParent<ActorArtHelper>();
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SwapBox()
    {
        ActorArtHelper.SwapBox();
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void VaultEnd()
    {
        ActorArtHelper.VaultEnd();
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void Kick()
    {
        ActorArtHelper.Kick();
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void TriggerSkill(EntitySkillIndex skillIndex)
    {
        ActorArtHelper.TriggerSkill(skillIndex);
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetCanTurn()
    {
        ActorArtHelper.CanTurn = true;
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetCannotTurn()
    {
        ActorArtHelper.CanTurn = false;
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetCanPlayOtherAnimSkill()
    {
        ActorArtHelper.CanPlayOtherAnimSkill = true;
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetCannotPlayOtherAnimSkill()
    {
        ActorArtHelper.CanPlayOtherAnimSkill = false;
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetCanPan()
    {
        ActorArtHelper.CanPan = true;
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetCannotPan()
    {
        ActorArtHelper.CanPan = false;
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetActorState(Actor.ActorBehaviourStates actorBehaviourState)
    {
        if (ActorArtHelper.Entity is Actor actor)
        {
            actor.ActorBehaviourState = actorBehaviourState;
        }
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void SetActorPosY(float relativePosY)
    {
        if (ActorArtHelper.Entity is Actor actor)
        {
            if (WorldManager.Instance.CurrentWorld.CheckIsGroundByPos(actor.transform.position, 30f, true, out GridPos3D nearestGroundGp))
            {
                Vector3 pos = actor.transform.position;
                pos.y = nearestGroundGp.y + relativePosY;
                actor.transform.position = pos;
            }
        }
    }
}