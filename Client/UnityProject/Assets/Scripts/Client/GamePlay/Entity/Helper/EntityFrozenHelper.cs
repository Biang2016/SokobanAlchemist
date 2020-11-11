using UnityEngine;
using Sirenix.OdinInspector;

public class EntityFrozenHelper : EntityMonoHelper
{
    [BoxGroup("冻结")]
    [LabelText("冻结Root")]
    public GameObject FrozeModelRoot;

    [BoxGroup("冻结")]
    [LabelText("冻结MeshRenderer")]
    public GameObject[] FrozeModels;

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