using UnityEngine;
using System.Collections;

public class ActorCommonHelpers : MonoBehaviour
{
    public ActorPushHelper ActorPushHelper;
    public ActorFaceHelper ActorFaceHelper;
    public ActorSkinHelper ActorSkinHelper;
    public ActorLaunchArcRendererHelper ActorLaunchArcRendererHelper;
    public ActorBattleHelper ActorBattleHelper;
    public ActorSkillHelper ActorSkillHelper;
    public Transform LiftBoxPivot;

    private void Awake()
    {
    }
}