using UnityEngine;
using System.Collections;

public class ActorCommonHelpers : MonoBehaviour
{
    public GameObject ActorMoveColliderRoot;
    public ActorArtHelper ActorArtHelper;
    public ActorPushHelper ActorPushHelper;
    public ActorFaceHelper ActorFaceHelper;
    public ActorSkinHelper ActorSkinHelper;
    public ActorLaunchArcRendererHelper ActorLaunchArcRendererHelper;
    public ActorBattleHelper ActorBattleHelper;
    public ActorSkillHelper ActorSkillHelper;
    public ActorBuffHelper ActorBuffHelper;
    public ActorFrozenHelper ActorFrozenHelper;
    public Transform LiftBoxPivot;

    private void Awake()
    {
    }
}