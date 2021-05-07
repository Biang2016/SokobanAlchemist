using System.Collections.Generic;
using BiangLibrary.GameDataFormat.Grid;
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
        Open = !entityDataExtraStates.TransportBoxClosed;
    }
}