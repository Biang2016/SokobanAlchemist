using Sirenix.OdinInspector;
using UnityEngine;

public class TransportBoxHelper : BoxMonoHelper
{
    public GameObject CloseModel;
    public GameObject OpenTrigger;

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
                CloseModel.SetActive(!open);
                OpenTrigger.SetActive(open);
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
        Open = true;
    }

    public override void ApplyEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.ApplyEntityExtraSerializeData(entityExtraSerializeData);
        if (entityExtraSerializeData.EntityDataExtraStates.R_TransportBoxClosed)
        {
            Open = !entityExtraSerializeData.EntityDataExtraStates.TransportBoxClosed;
        }
    }

    public override void RecordEntityExtraSerializeData(EntityExtraSerializeData entityExtraSerializeData)
    {
        base.RecordEntityExtraSerializeData(entityExtraSerializeData);
        entityExtraSerializeData.EntityDataExtraStates.R_TransportBoxClosed = true;
        entityExtraSerializeData.EntityDataExtraStates.TransportBoxClosed = !Open;
    }
}