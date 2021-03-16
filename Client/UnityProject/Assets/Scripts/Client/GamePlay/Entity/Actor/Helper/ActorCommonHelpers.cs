using System.Collections.Generic;
using UnityEngine;

public class ActorCommonHelpers : MonoBehaviour
{
    public GameObject ActorMoveColliderRoot;
    public ActorArtHelper ActorArtHelper;
    public ActorPushHelper ActorPushHelper;
    public ActorFaceHelper ActorFaceHelper;
    public ActorSkinHelper ActorSkinHelper;
    public ActorLaunchArcRendererHelper ActorLaunchArcRendererHelper;
    public ActorBattleHelper ActorBattleHelper;
    public ActorBoxInteractHelper ActorBoxInteractHelper;
    public EntityIndicatorHelper EntityIndicatorHelper;
    public EntityModelHelper EntityModelHelper;
    public EntityBuffHelper EntityBuffHelper;
    public ActorFrozenHelper ActorFrozenHelper;
    public EntityTriggerZoneHelper EntityTriggerZoneHelper;
    public EntityGrindTriggerZoneHelper EntityGrindTriggerZoneHelper;
    public List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers;
    public List<EntityLightningGeneratorHelper> ActorLightningGeneratorHelpers;
    public Transform LiftBoxPivot;

    private void Awake()
    {
    }
}