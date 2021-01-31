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
            box.PlayFXOnEachGrid(box.ThawFX, box.ThawFXScale);
        }
        else
        {
            FrozeModelRoot.SetActive(true);

            for (int index = 0; index < FrozeModels.Length; index++)
            {
                GameObject frozeModel = FrozeModels[index];
                frozeModel.SetActive(index == afterFrozenLevel - 1);
            }

            box.PlayFXOnEachGrid(beforeFrozenLevel < afterFrozenLevel ? box.FrozeFX : box.ThawFX, beforeFrozenLevel < afterFrozenLevel ? box.FrozeFXScale : box.ThawFXScale);
        }
    }
}