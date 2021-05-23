using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
using Sirenix.OdinInspector;
using UnityEngine;

public class DoorBoxHelper : BoxMonoHelper
{
    public Animator DoorAnim;

    public List<EntityIndicator> DoorEntityIndicators = new List<EntityIndicator>();

    public AK.Wwise.Event OnDoorOpen;
    public AK.Wwise.Event OnDoorClose;

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
                if (playSound)
                {
                    if (value) OnDoorOpen?.Post(Entity.gameObject);
                    else OnDoorClose?.Post(Entity.gameObject);
                }

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

    private bool playSound = false;

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        playSound = false;
        Open = false;
        playSound = true;
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        playSound = false;
        Open = false;
        playSound = true;
    }

    public override void RecordEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.RecordEntityExtraSerializeData(entityExtraSerializeData);
        entityExtraSerializeData.EntityDataExtraStates.R_DoorOpen = true;
        entityExtraSerializeData.EntityDataExtraStates.DoorOpen = Open;
    }

    public override void ApplyEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.ApplyEntityExtraSerializeData(entityExtraSerializeData);
        if (entityExtraSerializeData.EntityDataExtraStates.R_DoorOpen)
        {
            playSound = false;
            Open = entityExtraSerializeData.EntityDataExtraStates.DoorOpen;
            playSound = true;
        }
    }
}