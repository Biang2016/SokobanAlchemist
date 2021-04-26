using BiangLibrary.GameDataFormat.Grid;
using BiangLibrary.ObjectPool;
using UnityEngine;

public class WorldZoneTrigger : PoolObject
{
    internal WorldModule WorldModule;
    private BoxCollider BoxCollider;

    public override void OnUsed()
    {
        base.OnUsed();
        BoxCollider.enabled = true;
        gameObject.SetActive(true);
    }

    public override void OnRecycled()
    {
        base.OnRecycled();
        BoxCollider.enabled = false;
        gameObject.SetActive(false);
        WorldModule = null;
    }

    void Awake()
    {
        BoxCollider = GetComponent<BoxCollider>();
    }

    public void Initialize(GridPos3D gp)
    {
        transform.position = gp * WorldModule.MODULE_SIZE;
        BoxCollider.size = Vector3.one * (WorldModule.MODULE_SIZE - 0.5f);
        BoxCollider.center = 0.5f * Vector3.one * (WorldModule.MODULE_SIZE - 1);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerManager.Instance.Layer_ActorIndicator_Player)
        {
            Actor player = collider.gameObject.GetComponentInParent<Actor>();
            if (player == BattleManager.Instance.Player1)
            {
                WwiseAudioManager.Instance.WwiseBGMConfiguration.SwitchBGMTheme(WorldModule.WorldModuleData.BGM_ThemeState);
            }
        }
    }
}