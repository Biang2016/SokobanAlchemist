using DG.Tweening;
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

    void FixedUpdate()
    {
        if (Actor.IsNotNullAndAlive())
        {
            if (IsHitStop)
            {
                HitStopTick += Time.fixedDeltaTime;
                float beCollidedHitStopDuration = Actor.EntityStatPropSet.BeCollidedHitStopDuration.GetModifiedValue / 1000f;
                if (HitStopTick >= beCollidedHitStopDuration)
                {
                    HitStopTick = 0;
                    IsHitStop = false;
                }
            }
            else
            {
                HitStopTick = 0;
            }
        }
    }

    #region 第一优先级

    private float HitStopTick = 0;
    private bool isHitStop = false;

    public bool IsHitStop
    {
        get { return isHitStop; }
        set
        {
            if (isHitStop != value)
            {
                if (value)
                {
                    if (ActorModelAnim != null)
                    {
                        ActorModelAnim.speed = 0;
                    }
                }
                else
                {
                    if (ActorModelAnim != null)
                    {
                        ActorModelAnim.speed = 1;
                    }
                }

                isHitStop = value;
            }
        }
    }

    public void SetIsFrozen(bool isFrozen)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.speed = isFrozen ? 0 : 1;
        }
    }

    #endregion

    #region 第二优先级

    public void PlaySkill(EntitySkillIndex skillIndex)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetTrigger(skillIndex.ToString());
        }
    }

    /// <summary>
    /// Executed by animation
    /// </summary>
    public void TriggerSkill(EntitySkillIndex skillIndex)
    {
        if (Actor.EntityActiveSkillDict.TryGetValue(skillIndex, out EntityActiveSkill eas))
        {
            eas.TriggerActiveSkill();
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

    public void SetPFMoveGridSpeed(int PFMoveGridSpeed)
    {
        if (ActorModelAnim != null)
        {
            ActorModelAnim.SetInteger("PFMoveGridSpeed", PFMoveGridSpeed);
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