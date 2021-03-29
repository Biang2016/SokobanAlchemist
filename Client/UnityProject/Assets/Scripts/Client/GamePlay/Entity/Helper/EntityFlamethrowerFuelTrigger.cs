using UnityEngine;

public class EntityFlamethrowerFuelTrigger : MonoBehaviour
{
    public EntityFlamethrowerHelper EntityFlamethrowerHelper;

    private Collider[] FuelTriggers;

    public void Init()
    {
        FuelTriggers = GetComponents<Collider>();
    }

    public void EnableTrigger(bool enable)
    {
        foreach (Collider fuelTrigger in FuelTriggers)
        {
            fuelTrigger.enabled = enable;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!EntityFlamethrowerHelper.Entity.IsNotNullAndAlive()) return;
        Entity entity = collision.collider.GetComponentInParent<Entity>();
        if (entity.IsNotNullAndAlive() && entity is Box box)
        {
            if (box.State == Box.States.BeingKicked || box.State == Box.States.BeingKickedToGrind) // 只有踢状态的箱子可以触发此功能
            {
                if (box.BoxFrozenBoxHelper?.FrozenActor != null)
                {
                    // todo 特例，冻结敌人的箱子推入，还没想好逻辑
                }
                else
                {
                    // 从对象配置里面读取关联的被动技能行为，并拷贝作为本技能的行为
                    if (box.RawFlamethrowerFuelData?.RawEntityPassiveSkillActions_ForFlamethrower != null && box.RawFlamethrowerFuelData.RawEntityPassiveSkillActions_ForFlamethrower.Count > 0)
                    {
                        EntityFlamethrowerHelper.TurnOnFire(box.RawFlamethrowerFuelData.Clone());
                        box.FuelBox();
                    }
                }
            }
        }
    }
}