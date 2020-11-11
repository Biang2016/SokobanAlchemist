using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ActorBuffHelper : EntityBuffHelper
{
    protected override void MaxDominantBuffProcess(EntityBuff newBuff, List<EntityBuff> existedBuffList)
    {
        base.MaxDominantBuffProcess(newBuff, existedBuffList);
        if (newBuff is ActorBuff_ActorPropertyMultiplyModifier newBuff_multi)
        {
            Property.MultiplyModifier newModifier = newBuff_multi.MultiplyModifier;
            foreach (EntityBuff oldBuff in existedBuffList)
            {
                if (oldBuff is ActorBuff_ActorPropertyMultiplyModifier oldBuff_multi)
                {
                    Property.MultiplyModifier oldModifier = oldBuff_multi.MultiplyModifier;
                    if (newBuff_multi.PropertyType == oldBuff_multi.PropertyType)
                    {
                        if (newModifier.CanCover(oldModifier))
                        {
                            oldModifier.CoverModifiersGUID.Add(newModifier.GUID);
                        }
                        else
                        {
                            newModifier.CoverModifiersGUID.Add(oldModifier.GUID);
                        }
                    }
                }
            }
        }
        else if (newBuff is ActorBuff_ActorPropertyPlusModifier newBuff_plus)
        {
            Property.PlusModifier newModifier = newBuff_plus.PlusModifier;
            foreach (EntityBuff oldBuff in existedBuffList)
            {
                if (oldBuff is ActorBuff_ActorPropertyPlusModifier oldBuff_multi)
                {
                    Property.PlusModifier oldModifier = oldBuff_multi.PlusModifier;
                    if (newBuff_plus.PropertyType == oldBuff_multi.PropertyType)
                    {
                        if (newModifier.CanCover(oldModifier))
                        {
                            oldModifier.CoverModifiersGUID.Add(newModifier.GUID);
                        }
                        else
                        {
                            newModifier.CoverModifiersGUID.Add(oldModifier.GUID);
                        }
                    }
                }
            }
        }
    }
}