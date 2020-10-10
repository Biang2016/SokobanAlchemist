using BiangStudio.Singleton;
using UnityEngine;

public class FXManager : TSingletonBaseManager<FXManager>
{
    private Transform Root;

    public void Init(Transform root)
    {
        Root = root;
    }

    public FX PlayFX(string fxName, Vector3 position, float scale = 1.0f)
    {
        ushort fxTypeIndex = ConfigManager.GetFXTypeIndex(fxName);
        if (GameObjectPoolManager.Instance.FXDict.ContainsKey(fxTypeIndex))
        {
            FX fx = GameObjectPoolManager.Instance.FXDict[fxTypeIndex].AllocateGameObject<FX>(Root);
            fx.transform.position = position;
            fx.transform.localScale = Vector3.one * scale;
            fx.transform.rotation = Quaternion.identity;
            fx.Play();
            return fx;
        }

        return null;
    }
}