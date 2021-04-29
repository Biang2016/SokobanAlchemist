using System;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;

[Serializable]
public class EntitySkillAction_KickOut : EntitySkillAction, EntitySkillAction.IEntityAction
{
    public override void OnRecycled()
    {
    }

    protected override string Description => "踢出";

    [LabelText("@\"特效\t\"+FX")]
    public FXConfig FX = new FXConfig();

    [LabelText("自动识别方向_两侧")]
    public bool AutoDirection_Side = false;

    [LabelText("自动识别方向_前后")]
    public bool AutoDirection_BackForth = false;

    [LabelText("踢出局部方向")]
    [HideIf("AutoDirection_Side")]
    [HideIf("AutoDirection_BackForth")]
    public GridPosR.Orientation Direction;

    public float KickForce = 1;

    public void ExecuteOnEntity(Entity entity)
    {
        if (entity is Box box)
        {
            bool interactable = false;
            if (Entity is Actor actor)
            {
                if (actor.ActorBoxInteractHelper.CanInteract(InteractSkillType.Kick, box.EntityTypeIndex))
                {
                    interactable = true;
                }
            }
            else
            {
                interactable = true;
            }

            if (box && box.Kickable && interactable)
            {
                GridPos3D relativeDir = (box.EntityGeometryCenter - Entity.EntityGeometryCenter).ToGridPos3D().Normalized();
                GridPos3D casterForward3D = Entity.CurForward.ToGridPos3D().Normalized();

                GridPos3D kickDir = GridPos3D.Forward;
                if (AutoDirection_Side && AutoDirection_BackForth)
                {
                    kickDir = relativeDir;
                }
                else if (AutoDirection_Side)
                {
                    if (casterForward3D.x != 0)
                    {
                        kickDir = relativeDir;
                        kickDir.x = 0;
                    }
                    else if (casterForward3D.z != 0)
                    {
                        kickDir = relativeDir;
                        kickDir.z = 0;
                    }
                }
                else if (AutoDirection_BackForth)
                {
                    if (casterForward3D.x != 0)
                    {
                        kickDir = relativeDir;
                        kickDir.z = 0;
                    }
                    else if (casterForward3D.z != 0)
                    {
                        kickDir = relativeDir;
                        kickDir.x = 0;
                    }
                }
                else
                {
                    GridPos casterForward = new GridPos(casterForward3D.x, casterForward3D.z);
                    GridPos dir = GridPos.RotateGridPos(casterForward, Direction);
                    kickDir = new GridPos3D(dir.x, 0, dir.z);
                }

                box.Kick(kickDir, KickForce, Entity);
                FXManager.Instance.PlayFX(FX, box.EntityGeometryCenter);
            }
        }
    }

    protected override void ChildClone(EntitySkillAction newAction)
    {
        base.ChildClone(newAction);
        EntitySkillAction_KickOut action = ((EntitySkillAction_KickOut) newAction);
        action.FX = FX.Clone();
        action.AutoDirection_Side = AutoDirection_Side;
        action.AutoDirection_BackForth = AutoDirection_BackForth;
        action.Direction = Direction;
        action.KickForce = KickForce;
    }

    public override void CopyDataFrom(EntitySkillAction srcData)
    {
        base.CopyDataFrom(srcData);
        EntitySkillAction_KickOut action = ((EntitySkillAction_KickOut) srcData);
        FX.CopyDataFrom(action.FX);
        AutoDirection_Side = action.AutoDirection_Side;
        AutoDirection_BackForth = action.AutoDirection_BackForth;
        Direction = action.Direction;
        KickForce = action.KickForce;
    }
}