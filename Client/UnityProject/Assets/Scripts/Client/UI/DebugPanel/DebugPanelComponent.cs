using System.Collections.Generic;
using BiangStudio.ObjectPool;

public class DebugPanelComponent : PoolObject
{
    public Dictionary<string, DebugPanelComponent> DebugComponentDictTree = new Dictionary<string, DebugPanelComponent>();

    public override void PoolRecycle()
    {
        foreach (KeyValuePair<string, DebugPanelComponent> kv in DebugComponentDictTree)
        {
            kv.Value.PoolRecycle();
        }

        DebugComponentDictTree.Clear();
        base.PoolRecycle();
    }

    public virtual bool IsOpen { get; set; }
}