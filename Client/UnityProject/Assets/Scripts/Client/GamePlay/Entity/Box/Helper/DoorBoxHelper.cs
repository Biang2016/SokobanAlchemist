using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class DoorBoxHelper : BoxMonoHelper
{
    public Animator DoorAnim;

    public List<EntityIndicator> DoorEntityIndicators = new List<EntityIndicator>();

    [ShowInInspector]
    private bool open;

    public bool Open
    {
        get { return open; }
        set
        {
            if (open != value)
            {
                open = value;
                DoorAnim.ResetTrigger(open ? "Close" : "Open");
                DoorAnim.SetTrigger(open ? "Open" : "Close");
                foreach (EntityIndicator doorEntityIndicator in DoorEntityIndicators)
                {
                    GridPos3D offset = doorEntityIndicator.Offset;
                    GridPos offset_rotated = GridPos.RotateGridPos(new GridPos(offset.x, offset.z), Entity.EntityOrientation);
                    GridPos3D offset3D_rotated = new GridPos3D(offset_rotated.x, offset.y, offset_rotated.z);
                    WorldManager.Instance.CurrentWorld.GetBoxByGridPosition(offset3D_rotated + Entity.WorldGP, 0, out WorldModule module, out GridPos3D boxGridLocalGP);
                    if (module)
                    {
                        Box existedBox = (Box) module[TypeDefineType.Box, boxGridLocalGP];
                        if (!value && existedBox != null && existedBox != Entity) existedBox.DestroySelf(); // 被门夹的箱子自行摧毁 todo 先这样处理
                        module[TypeDefineType.Box, boxGridLocalGP, false, true, Entity.IsTriggerEntity, Entity.GUID] = value ? null : Entity;
                    }

                    doorEntityIndicator.gameObject.SetActive(!value);
                }
            }
        }
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        Open = false;
    }

    public override void RecordEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
        base.RecordEntityExtraStates(entityDataExtraStates);
        Entity.CurrentEntityData.RawEntityExtraSerializeData.EntityDataExtraStates.DoorOpen = Open;
    }

    public override void ApplyEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
        base.ApplyEntityExtraStates(entityDataExtraStates);
        Open = entityDataExtraStates.DoorOpen;
    }
}