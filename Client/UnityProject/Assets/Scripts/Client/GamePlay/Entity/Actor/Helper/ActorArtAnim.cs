﻿using UnityEngine;

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
}