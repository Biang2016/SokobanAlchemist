using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class EntityFrozenHelper : EntityMonoHelper
{
    [BoxGroup("冻结")]
    [LabelText("冻结Root")]
    public GameObject FrozeModelRoot;

    [BoxGroup("冻结")]
    [LabelText("冻结模型")]
    public GameObject[] FrozeModels;

    public SmoothMove IceBlockSmoothMove;

    void Awake()
    {
        OnFrozeIntoIceBlockAction = FrozeIntoIceBlock;
    }

    public override void OnHelperRecycled()
    {
        base.OnHelperRecycled();
        Thaw();
    }

    public override void OnHelperUsed()
    {
        base.OnHelperUsed();
        Thaw();
    }

    public UnityAction<int, int> OnFrozeIntoIceBlockAction;
    public virtual void FrozeIntoIceBlock(int beforeFrozenLevel, int afterFrozenLevel)
    {
    }

    protected void Thaw()
    {
        for (int index = 0; index < FrozeModels.Length; index++)
        {
            GameObject frozeModel = FrozeModels[index];
            frozeModel.SetActive(false);
        }

        FrozeModelRoot.SetActive(false);
    }
}