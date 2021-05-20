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

    public override void ApplyEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
        base.ApplyEntityExtraStates(entityDataExtraStates);
        if (entityDataExtraStates.R_TransportBoxClosed)
        {
            Open = !entityDataExtraStates.TransportBoxClosed;
        }
    }

    public override void RecordEntityExtraStates(EntityDataExtraStates entityDataExtraStates)
    {
        base.RecordEntityExtraStates(entityDataExtraStates);
        entityDataExtraStates.R_TransportBoxClosed = true;
        entityDataExtraStates.TransportBoxClosed = !Open;
    }
}