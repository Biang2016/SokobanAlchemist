using BiangLibrary.GameDataFormat.Grid;
using UnityEngine;

public class BoxFrozenHelper : EntityFrozenHelper
{
    public override void FrozeIntoIceBlock(int beforeFrozenLevel, int afterFrozenLevel)
    {
        Box box = (Box) Entity;
        if (afterFrozenLevel == 0)
        {
            Thaw();
            foreach (GridPos3D offset in box.GetBoxOccupationGPs())
            {
                FXManager.Instance.PlayFX(box.ThawFX, transform.position + offset, 1f);
            }
        }
        else
        {
            FrozeModelRoot.SetActive(true);

            for (int index = 0; index < FrozeModels.Length; index++)
            {
                GameObject frozeModel = FrozeModels[index];
                frozeModel.SetActive(index == afterFrozenLevel - 1);
            }

            foreach (GridPos3D offset in box.GetBoxOccupationGPs())
            {
                FXManager.Instance.PlayFX(beforeFrozenLevel < afterFrozenLevel ? box.FrozeFX : box.ThawFX, transform.position + offset, 1f);
            }
        }
    }
}