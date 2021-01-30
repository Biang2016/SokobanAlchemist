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
    public EntityBuffHelper EntityBuffHelper;
    public ActorFrozenHelper ActorFrozenHelper;
    public EntityTriggerZoneHelper EntityTriggerZoneHelper;
    public List<EntityFlamethrowerHelper> EntityFlamethrowerHelpers;
    public Transform LiftBoxPivot;

    private void Awake()
    {
    }
}